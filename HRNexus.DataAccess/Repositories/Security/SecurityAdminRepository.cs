using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Security;

public sealed class SecurityAdminRepository : ISecurityAdminRepository
{
    private readonly HRNexusDbContext _dbContext;

    public SecurityAdminRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SecurityUserQueryResult>> ListUsersAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(user => user.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmed = search.Trim();
            query = query.Where(user =>
                user.Username.Contains(trimmed)
                || (user.Employee != null && user.Employee.EmployeeCode.Contains(trimmed))
                || (user.Employee != null && user.Employee.Person.FullName.Contains(trimmed)));
        }

        var users = await ProjectUserBasics(query.OrderBy(user => user.Username))
            .ToListAsync(cancellationToken);

        return await AttachRoleNamesAsync(users, cancellationToken);
    }

    public async Task<SecurityUserQueryResult?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await ProjectUserBasics(_dbContext.Users.AsNoTracking().Where(user => user.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var users = await AttachRoleNamesAsync([user], cancellationToken);
        return users.FirstOrDefault();
    }

    public Task<User?> GetUserForUpdateAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(string username, int? exceptUserId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user =>
                user.Username == username
                && (!exceptUserId.HasValue || user.UserId != exceptUserId.Value),
                cancellationToken);
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public Task<AccountStatus?> GetAccountStatusAsync(int accountStatusId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccountStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(status => status.AccountStatusId == accountStatusId && status.IsActive, cancellationToken);
    }

    public Task<AccountStatus?> GetAccountStatusByCodeAsync(string statusCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccountStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(status => status.StatusCode == statusCode && status.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<SecurityUserRoleQueryResult>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .OrderBy(userRole => userRole.Role.RoleName)
            .Select(userRole => new SecurityUserRoleQueryResult(
                userRole.RoleId,
                userRole.Role.RoleName,
                userRole.Role.RoleDescription,
                userRole.Role.IsActive,
                userRole.AssignedDate,
                userRole.AssignedBy,
                userRole.AssignedByUser == null ? null : userRole.AssignedByUser.Username))
            .ToListAsync(cancellationToken);
    }

    public Task<Role?> GetRoleForAssignmentAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.RoleId == roleId, cancellationToken);
    }

    public Task<bool> UserRoleExistsAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserRoles
            .AsNoTracking()
            .AnyAsync(userRole => userRole.UserId == userId && userRole.RoleId == roleId, cancellationToken);
    }

    public Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserRoles.AddAsync(userRole, cancellationToken).AsTask();
    }

    public Task<UserRole?> GetUserRoleForUpdateAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserRoles
            .Include(userRole => userRole.Role)
            .FirstOrDefaultAsync(userRole => userRole.UserId == userId && userRole.RoleId == roleId, cancellationToken);
    }

    public void RemoveUserRole(UserRole userRole)
    {
        _dbContext.UserRoles.Remove(userRole);
    }

    public Task<int> CountActiveAdminUsersExceptAsync(int excludedUserId, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole =>
                userRole.UserId != excludedUserId
                && userRole.Role.RoleName == "Admin"
                && userRole.Role.IsActive
                && userRole.User.IsActive
                && userRole.User.AccountStatus.StatusCode == "A")
            .Select(userRole => userRole.UserId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public Task<Role?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.RoleId == roleId, cancellationToken);
    }

    public Task<Module?> GetModuleAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(module => module.ModuleId == moduleId, cancellationToken);
    }

    public async Task<IReadOnlyList<RolePermissionMaskQueryResult>> GetRolePermissionsAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(permission => permission.RoleId == roleId)
            .OrderBy(permission => permission.Module.ModuleName)
            .Select(permission => new RolePermissionMaskQueryResult(
                permission.RoleId,
                permission.Role.RoleName,
                permission.ModuleId,
                permission.Module.ModuleName,
                permission.PermissionMask))
            .ToListAsync(cancellationToken);
    }

    public Task<RolePermission?> GetRolePermissionForUpdateAsync(int roleId, int moduleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.RolePermissions
            .FirstOrDefaultAsync(permission => permission.RoleId == roleId && permission.ModuleId == moduleId, cancellationToken);
    }

    public Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken = default)
    {
        return _dbContext.RolePermissions.AddAsync(rolePermission, cancellationToken).AsTask();
    }

    public void RemoveRolePermission(RolePermission rolePermission)
    {
        _dbContext.RolePermissions.Remove(rolePermission);
    }

    public Task AddPermissionAuditAsync(PermissionAudit permissionAudit, CancellationToken cancellationToken = default)
    {
        return _dbContext.PermissionAudits.AddAsync(permissionAudit, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<UserPermissionOverrideQueryResult>> GetUserPermissionsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserPermissions
            .AsNoTracking()
            .Where(permission => permission.UserId == userId)
            .OrderBy(permission => permission.Module.ModuleName)
            .Select(permission => new UserPermissionOverrideQueryResult(
                permission.UserId,
                permission.User.Username,
                permission.ModuleId,
                permission.Module.ModuleName,
                permission.PermissionMask))
            .ToListAsync(cancellationToken);
    }

    public Task<UserPermission?> GetUserPermissionForUpdateAsync(int userId, int moduleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserPermissions
            .FirstOrDefaultAsync(permission => permission.UserId == userId && permission.ModuleId == moduleId, cancellationToken);
    }

    public Task AddUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserPermissions.AddAsync(userPermission, cancellationToken).AsTask();
    }

    public void RemoveUserPermission(UserPermission userPermission)
    {
        _dbContext.UserPermissions.Remove(userPermission);
    }

    public async Task<IReadOnlyList<UserActivityLogQueryResult>> ListActivityLogsAsync(
        int? userId,
        int? activityTypeId,
        bool? isSuccess,
        DateTime? from,
        DateTime? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = CreateActivityLogQuery(userId, activityTypeId, isSuccess, from, to);

        return await query
            .OrderByDescending(log => log.OccurredAt)
            .ThenByDescending(log => log.ActivityLogId)
            .Skip(skip)
            .Take(take)
            .Select(log => new UserActivityLogQueryResult(
                log.ActivityLogId,
                log.UserId,
                log.User == null ? null : log.User.Username,
                log.ActivityTypeId,
                log.ActivityType.ActivityTypeName,
                log.ActivityType.ActivityTypeCode,
                log.ActivityDetails,
                log.IpAddress,
                log.OccurredAt,
                log.IsSuccess))
            .ToListAsync(cancellationToken);
    }

    public Task<UserActivityLogQueryResult?> GetActivityLogAsync(int activityLogId, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserActivityLogs
            .AsNoTracking()
            .Where(log => log.ActivityLogId == activityLogId)
            .Select(log => new UserActivityLogQueryResult(
                log.ActivityLogId,
                log.UserId,
                log.User == null ? null : log.User.Username,
                log.ActivityTypeId,
                log.ActivityType.ActivityTypeName,
                log.ActivityType.ActivityTypeCode,
                log.ActivityDetails,
                log.IpAddress,
                log.OccurredAt,
                log.IsSuccess))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshTokenMetadataQueryResult>> GetRefreshTokensByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId)
            .OrderByDescending(token => token.CreatedAt)
            .Select(token => new RefreshTokenMetadataQueryResult(
                token.RefreshTokenId,
                token.UserId,
                token.CreatedAt,
                token.ExpiresAt,
                token.RevokedAt,
                token.CreatedByIp,
                token.RevokedByIp,
                token.ReplacedByTokenId))
            .ToListAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenForUpdateAsync(int refreshTokenId, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.RefreshTokenId == refreshTokenId, cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionAuditQueryResult>> ListPermissionAuditsAsync(
        int? roleId,
        int? moduleId,
        DateTime? from,
        DateTime? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PermissionAudits.AsNoTracking();

        if (roleId.HasValue)
        {
            query = query.Where(audit => audit.RoleId == roleId.Value);
        }

        if (moduleId.HasValue)
        {
            query = query.Where(audit => audit.ModuleId == moduleId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(audit => audit.ChangedDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(audit => audit.ChangedDate <= to.Value);
        }

        return await query
            .OrderByDescending(audit => audit.ChangedDate)
            .ThenByDescending(audit => audit.AuditId)
            .Skip(skip)
            .Take(take)
            .Select(audit => new PermissionAuditQueryResult(
                audit.AuditId,
                audit.RoleId,
                audit.Role.RoleName,
                audit.ModuleId,
                audit.Module.ModuleName,
                audit.OldMask,
                audit.NewMask,
                audit.ChangedBy,
                audit.ChangedByUser.Username,
                audit.ChangedDate))
            .ToListAsync(cancellationToken);
    }

    public Task<PermissionAuditQueryResult?> GetPermissionAuditAsync(int auditId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PermissionAudits
            .AsNoTracking()
            .Where(audit => audit.AuditId == auditId)
            .Select(audit => new PermissionAuditQueryResult(
                audit.AuditId,
                audit.RoleId,
                audit.Role.RoleName,
                audit.ModuleId,
                audit.Module.ModuleName,
                audit.OldMask,
                audit.NewMask,
                audit.ChangedBy,
                audit.ChangedByUser.Username,
                audit.ChangedDate))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<SecurityUserQueryResult>> AttachRoleNamesAsync(
        IReadOnlyList<SecurityUserBasicQueryResult> users,
        CancellationToken cancellationToken)
    {
        if (users.Count == 0)
        {
            return [];
        }

        var userIds = users.Select(user => user.UserId).ToArray();

        var roleRows = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userIds.Contains(userRole.UserId) && userRole.Role.IsActive)
            .OrderBy(userRole => userRole.Role.RoleName)
            .Select(userRole => new
            {
                userRole.UserId,
                userRole.Role.RoleName
            })
            .ToListAsync(cancellationToken);

        var rolesByUserId = roleRows
            .GroupBy(role => role.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(role => role.RoleName).ToArray());

        return users
            .Select(user => new SecurityUserQueryResult(
                user.UserId,
                user.EmployeeId,
                user.EmployeeCode,
                user.EmployeeName,
                user.Username,
                user.IsActive,
                user.AccountStatusId,
                user.AccountStatusName,
                user.AccountStatusCode,
                user.FailedLoginAttempts,
                user.LastLoginAt,
                user.CreatedDate,
                user.ModifiedDate,
                rolesByUserId.TryGetValue(user.UserId, out var roles) ? roles : []))
            .ToList();
    }

    private static IQueryable<SecurityUserBasicQueryResult> ProjectUserBasics(IQueryable<User> query)
    {
        return query.Select(user => new SecurityUserBasicQueryResult(
            user.UserId,
            user.EmployeeId,
            user.Employee == null ? null : user.Employee.EmployeeCode,
            user.Employee == null ? null : user.Employee.Person.FullName,
            user.Username,
            user.IsActive,
            user.AccountStatusId,
            user.AccountStatus.StatusName,
            user.AccountStatus.StatusCode,
            user.FailedLoginAttempts,
            user.LastLoginAt,
            user.CreatedDate,
            user.ModifiedDate));
    }

    private IQueryable<UserActivityLog> CreateActivityLogQuery(
        int? userId,
        int? activityTypeId,
        bool? isSuccess,
        DateTime? from,
        DateTime? to)
    {
        var query = _dbContext.UserActivityLogs.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(log => log.UserId == userId.Value);
        }

        if (activityTypeId.HasValue)
        {
            query = query.Where(log => log.ActivityTypeId == activityTypeId.Value);
        }

        if (isSuccess.HasValue)
        {
            query = query.Where(log => log.IsSuccess == isSuccess.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(log => log.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(log => log.OccurredAt <= to.Value);
        }

        return query;
    }

}

sealed record SecurityUserBasicQueryResult(
    int UserId,
    int? EmployeeId,
    string? EmployeeCode,
    string? EmployeeName,
    string Username,
    bool IsActive,
    int AccountStatusId,
    string AccountStatusName,
    string AccountStatusCode,
    int FailedLoginAttempts,
    DateTime? LastLoginAt,
    DateTime CreatedDate,
    DateTime? ModifiedDate);
