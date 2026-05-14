using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Security;

public sealed class UserRepository : IUserRepository
{
    private readonly HRNexusDbContext _dbContext;

    public UserRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<User?> GetByIdForUpdateAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<User?> GetByUsernameForUpdateAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(x => x.AccountStatus)
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByUsernamesForUpdateAsync(
        IReadOnlyCollection<string> usernames,
        CancellationToken cancellationToken = default)
    {
        if (usernames.Count == 0)
        {
            return [];
        }

        return await _dbContext.Users
            .Where(x => usernames.Contains(x.Username))
            .OrderBy(x => x.Username)
            .ToListAsync(cancellationToken);
    }

    public Task<UserAuthQueryResult?> GetAuthByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Username == username)
            .Select(x => new UserAuthQueryResult(
                x.UserId,
                x.EmployeeId,
                x.Username,
                x.PasswordHash,
                x.IsActive,
                x.FailedLoginAttempts,
                x.AccountStatus.StatusCode,
                x.AccountStatus.StatusName,
                x.UserRoles
                    .Where(userRole => userRole.Role.IsActive)
                    .Select(userRole => userRole.Role.RoleName)
                    .OrderBy(roleName => roleName)
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<UserIdentityQueryResult?> GetIdentityByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new UserIdentityQueryResult(
                x.UserId,
                x.EmployeeId,
                x.Username,
                x.IsActive,
                x.AccountStatus.StatusCode,
                x.AccountStatus.StatusName,
                x.UserRoles
                    .Where(userRole => userRole.Role.IsActive)
                    .Select(userRole => userRole.Role.RoleName)
                    .OrderBy(roleName => roleName)
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Role.IsActive)
            .OrderBy(x => x.Role.RoleName)
            .Select(x => x.Role.RoleName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserPermissionQueryResult>> GetPermissionSummariesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var activeModules = await _dbContext.Modules
            .AsNoTracking()
            .Where(module => module.IsActive)
            .OrderBy(module => module.ModuleName)
            .Select(module => new
            {
                module.ModuleId,
                module.ModuleName
            })
            .ToListAsync(cancellationToken);

        var userState = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserId == userId)
            .Select(user => new
            {
                user.IsActive,
                AccountStatusCode = user.AccountStatus.StatusCode,
                AccountStatusIsActive = user.AccountStatus.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userState is null)
        {
            return [];
        }

        if (!IsPermissionEligibleUser(userState.IsActive, userState.AccountStatusCode, userState.AccountStatusIsActive))
        {
            return activeModules
                .Select(module => new UserPermissionQueryResult(module.ModuleName, 0, "Effective"))
                .ToList();
        }

        var rolePermissions = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId && userRole.Role.IsActive)
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Where(permission => permission.Module.IsActive)
            .Select(permission => new
            {
                permission.ModuleId,
                permission.PermissionMask
            })
            .ToListAsync(cancellationToken);

        var userPermissions = await _dbContext.UserPermissions
            .AsNoTracking()
            .Where(permission => permission.UserId == userId && permission.Module.IsActive)
            .Select(permission => new
            {
                permission.ModuleId,
                permission.PermissionMask
            })
            .ToListAsync(cancellationToken);

        return activeModules
            .Select(module =>
            {
                var masks = rolePermissions
                    .Where(permission => permission.ModuleId == module.ModuleId)
                    .Select(permission => permission.PermissionMask)
                    .Concat(userPermissions
                        .Where(permission => permission.ModuleId == module.ModuleId)
                        .Select(permission => permission.PermissionMask));

                return new UserPermissionQueryResult(
                    module.ModuleName,
                    CalculateEffectivePermissionMask(masks),
                    "Effective");
            })
            .ToList();
    }

    public Task<AccountStatus?> GetAccountStatusByCodeAsync(string statusCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccountStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StatusCode == statusCode, cancellationToken);
    }

    private static bool IsPermissionEligibleUser(bool isActive, string accountStatusCode, bool accountStatusIsActive)
    {
        return isActive
            && accountStatusIsActive
            && string.Equals(accountStatusCode, "A", StringComparison.OrdinalIgnoreCase);
    }

    private static int CalculateEffectivePermissionMask(IEnumerable<int> permissionMasks)
    {
        var effectiveMask = 0;

        foreach (var permissionMask in permissionMasks)
        {
            if (permissionMask == -1)
            {
                return -1;
            }

            if (permissionMask > 0)
            {
                effectiveMask |= permissionMask;
            }
        }

        return effectiveMask;
    }
}
