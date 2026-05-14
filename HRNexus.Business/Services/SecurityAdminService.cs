using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Security;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Security;

namespace HRNexus.Business.Services;

public sealed class SecurityAdminService : ISecurityAdminService
{
    private static readonly string[] BuiltInRoleNames =
    [
        "Admin",
        "HRManager",
        "HRClerk",
        "DepartmentManager",
        "Employee"
    ];

    private const string ActiveStatusCode = "A";
    private const string LockedStatusCode = "L";

    private readonly ISecurityAdminRepository _securityAdminRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public SecurityAdminService(
        ISecurityAdminRepository securityAdminRepository,
        IEmployeeRepository employeeRepository,
        IPasswordHashingService passwordHashingService,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _securityAdminRepository = securityAdminRepository;
        _employeeRepository = employeeRepository;
        _passwordHashingService = passwordHashingService;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SecurityUserDto>> ListUsersAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var users = await _securityAdminRepository.ListUsersAsync(search, includeInactive, cancellationToken);
        return users.Select(MapUser).ToList();
    }

    public async Task<SecurityUserDto> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _securityAdminRepository.GetUserAsync(userId, cancellationToken)
            ?? throw UserNotFound(userId);

        return MapUser(user);
    }

    public async Task<SecurityUserDto> CreateUserAsync(
        CreateSecurityUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePassword(request.Password);

        var username = NormalizeUsername(request.Username);

        if (await _securityAdminRepository.UsernameExistsAsync(username, cancellationToken: cancellationToken))
        {
            throw new BusinessRuleException($"Username '{username}' is already in use.");
        }

        if (!await _employeeRepository.ExistsAsync(request.EmployeeId, cancellationToken))
        {
            throw new BusinessRuleException($"Employee {request.EmployeeId} was not found.");
        }

        var accountStatus = await _securityAdminRepository.GetAccountStatusAsync(request.AccountStatusId, cancellationToken)
            ?? throw new BusinessRuleException($"Account status {request.AccountStatusId} was not found or is inactive.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            EmployeeId = request.EmployeeId,
            Username = username,
            PasswordHash = _passwordHashingService.HashPassword(request.Password),
            IsActive = request.IsActive,
            AccountStatusId = accountStatus.AccountStatusId,
            CreatedDate = now,
            ModifiedDate = now
        };

        await _securityAdminRepository.AddUserAsync(user, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create security user", cancellationToken);

        return await GetUserAsync(user.UserId, cancellationToken);
    }

    public async Task<SecurityUserDto> UpdateUserAsync(
        int userId,
        UpdateSecurityUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var username = NormalizeUsername(request.Username);

        if (await _securityAdminRepository.UsernameExistsAsync(username, userId, cancellationToken))
        {
            throw new BusinessRuleException($"Username '{username}' is already in use.");
        }

        var accountStatus = await _securityAdminRepository.GetAccountStatusAsync(request.AccountStatusId, cancellationToken)
            ?? throw new BusinessRuleException($"Account status {request.AccountStatusId} was not found or is inactive.");

        await EnsureNotRemovingLastAdminAsync(userId, request.IsActive, accountStatus.StatusCode, cancellationToken);

        var user = await _securityAdminRepository.GetUserForUpdateAsync(userId, cancellationToken)
            ?? throw UserNotFound(userId);

        user.Username = username;
        user.AccountStatusId = accountStatus.AccountStatusId;
        user.IsActive = request.IsActive;
        user.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update security user", cancellationToken);
        return await GetUserAsync(userId, cancellationToken);
    }

    public async Task<SecurityUserDto> ResetPasswordAsync(
        int userId,
        ResetSecurityUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePassword(request.Password);

        var user = await _securityAdminRepository.GetUserForUpdateAsync(userId, cancellationToken)
            ?? throw UserNotFound(userId);

        user.PasswordHash = _passwordHashingService.HashPassword(request.Password);
        user.FailedLoginAttempts = 0;
        user.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "reset security user password", cancellationToken);
        return await GetUserAsync(userId, cancellationToken);
    }

    public async Task<SecurityUserDto> LockUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var lockedStatus = await _securityAdminRepository.GetAccountStatusByCodeAsync(LockedStatusCode, cancellationToken)
            ?? throw new BusinessRuleException("Locked account status was not found.");

        await EnsureNotRemovingLastAdminAsync(userId, isActive: true, lockedStatus.StatusCode, cancellationToken);

        var user = await _securityAdminRepository.GetUserForUpdateAsync(userId, cancellationToken)
            ?? throw UserNotFound(userId);

        user.AccountStatusId = lockedStatus.AccountStatusId;
        user.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "lock security user", cancellationToken);
        return await GetUserAsync(userId, cancellationToken);
    }

    public async Task<SecurityUserDto> UnlockUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var activeStatus = await _securityAdminRepository.GetAccountStatusByCodeAsync(ActiveStatusCode, cancellationToken)
            ?? throw new BusinessRuleException("Active account status was not found.");

        var user = await _securityAdminRepository.GetUserForUpdateAsync(userId, cancellationToken)
            ?? throw UserNotFound(userId);

        user.AccountStatusId = activeStatus.AccountStatusId;
        user.IsActive = true;
        user.FailedLoginAttempts = 0;
        user.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "unlock security user", cancellationToken);
        return await GetUserAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<SecurityUserRoleDto>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);
        var roles = await _securityAdminRepository.GetUserRolesAsync(userId, cancellationToken);
        return roles.Select(MapUserRole).ToList();
    }

    public async Task<SecurityUserRoleDto> AssignRoleAsync(
        int userId,
        AssignUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureUserExistsAsync(userId, cancellationToken);

        var role = await _securityAdminRepository.GetRoleForAssignmentAsync(request.RoleId, cancellationToken)
            ?? throw new BusinessRuleException($"Role {request.RoleId} was not found.");

        if (!role.IsActive)
        {
            throw new BusinessRuleException($"Role {request.RoleId} is inactive and cannot be assigned.");
        }

        if (await _securityAdminRepository.UserRoleExistsAsync(userId, request.RoleId, cancellationToken))
        {
            throw new BusinessRuleException("User already has this role.");
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = request.RoleId,
            AssignedBy = _currentUserContext.UserId,
            AssignedDate = DateTime.UtcNow
        };

        await _securityAdminRepository.AddUserRoleAsync(userRole, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "assign user role", cancellationToken);

        var roles = await GetUserRolesAsync(userId, cancellationToken);
        return roles.First(roleDto => roleDto.RoleId == request.RoleId);
    }

    public async Task<SecurityUserRoleDto> RemoveRoleAsync(
        int userId,
        int roleId,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);
        var existing = (await GetUserRolesAsync(userId, cancellationToken)).FirstOrDefault(role => role.RoleId == roleId)
            ?? throw new EntityNotFoundException($"User role assignment {userId}/{roleId} was not found.");

        var userRole = await _securityAdminRepository.GetUserRoleForUpdateAsync(userId, roleId, cancellationToken)
            ?? throw new EntityNotFoundException($"User role assignment {userId}/{roleId} was not found.");

        if (string.Equals(userRole.Role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var remainingAdmins = await _securityAdminRepository.CountActiveAdminUsersExceptAsync(userId, cancellationToken);
            if (remainingAdmins == 0)
            {
                throw new BusinessRuleException("Cannot remove the last active Admin role assignment.");
            }
        }

        _securityAdminRepository.RemoveUserRole(userRole);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "remove user role", cancellationToken);
        return existing;
    }

    public async Task<IReadOnlyList<RolePermissionMaskDto>> GetRolePermissionsAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRoleExistsAsync(roleId, cancellationToken);
        var permissions = await _securityAdminRepository.GetRolePermissionsAsync(roleId, cancellationToken);
        return permissions.Select(MapRolePermission).ToList();
    }

    public async Task<RolePermissionMaskDto> SetRolePermissionAsync(
        int roleId,
        int moduleId,
        SetPermissionMaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsurePermissionMask(request.PermissionMask);

        var role = await EnsureRoleExistsAsync(roleId, cancellationToken);
        await EnsureModuleExistsAsync(moduleId, cancellationToken);

        var currentUserId = _currentUserContext.UserId
            ?? throw new AuthorizationFailedException("Authenticated admin user context was not found.");

        var rolePermission = await _securityAdminRepository.GetRolePermissionForUpdateAsync(roleId, moduleId, cancellationToken);
        var oldMask = rolePermission?.PermissionMask ?? 0;

        if (rolePermission is null)
        {
            rolePermission = new RolePermission
            {
                RoleId = roleId,
                ModuleId = moduleId,
                PermissionMask = request.PermissionMask
            };
            await _securityAdminRepository.AddRolePermissionAsync(rolePermission, cancellationToken);
        }
        else
        {
            rolePermission.PermissionMask = request.PermissionMask;
        }

        await _securityAdminRepository.AddPermissionAuditAsync(new PermissionAudit
        {
            RoleId = roleId,
            ModuleId = moduleId,
            OldMask = oldMask,
            NewMask = request.PermissionMask,
            ChangedBy = currentUserId,
            ChangedDate = DateTime.UtcNow
        }, cancellationToken);

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "set role permission mask", cancellationToken);
        return (await GetRolePermissionsAsync(role.RoleId, cancellationToken)).First(permission => permission.ModuleId == moduleId);
    }

    public async Task<RolePermissionMaskDto> RemoveRolePermissionAsync(
        int roleId,
        int moduleId,
        CancellationToken cancellationToken = default)
    {
        var role = await EnsureRoleExistsAsync(roleId, cancellationToken);

        if (IsBuiltInRole(role.RoleName))
        {
            throw new BusinessRuleException("Built-in role permission masks cannot be removed. Set a revised mask instead.");
        }

        await EnsureModuleExistsAsync(moduleId, cancellationToken);
        var existing = (await GetRolePermissionsAsync(roleId, cancellationToken)).FirstOrDefault(permission => permission.ModuleId == moduleId)
            ?? throw new EntityNotFoundException($"Role permission {roleId}/{moduleId} was not found.");

        var currentUserId = _currentUserContext.UserId
            ?? throw new AuthorizationFailedException("Authenticated admin user context was not found.");

        var rolePermission = await _securityAdminRepository.GetRolePermissionForUpdateAsync(roleId, moduleId, cancellationToken)
            ?? throw new EntityNotFoundException($"Role permission {roleId}/{moduleId} was not found.");

        _securityAdminRepository.RemoveRolePermission(rolePermission);
        await _securityAdminRepository.AddPermissionAuditAsync(new PermissionAudit
        {
            RoleId = roleId,
            ModuleId = moduleId,
            OldMask = rolePermission.PermissionMask,
            NewMask = 0,
            ChangedBy = currentUserId,
            ChangedDate = DateTime.UtcNow
        }, cancellationToken);

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "remove role permission mask", cancellationToken);
        return existing;
    }

    public async Task<IReadOnlyList<UserPermissionOverrideDto>> GetUserPermissionsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);
        var permissions = await _securityAdminRepository.GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.Select(MapUserPermission).ToList();
    }

    public async Task<UserPermissionOverrideDto> SetUserPermissionAsync(
        int userId,
        int moduleId,
        SetPermissionMaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsurePermissionMask(request.PermissionMask);
        await EnsureUserExistsAsync(userId, cancellationToken);
        await EnsureModuleExistsAsync(moduleId, cancellationToken);

        var permission = await _securityAdminRepository.GetUserPermissionForUpdateAsync(userId, moduleId, cancellationToken);

        if (permission is null)
        {
            permission = new UserPermission
            {
                UserId = userId,
                ModuleId = moduleId,
                PermissionMask = request.PermissionMask
            };
            await _securityAdminRepository.AddUserPermissionAsync(permission, cancellationToken);
        }
        else
        {
            permission.PermissionMask = request.PermissionMask;
        }

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "set user permission override", cancellationToken);
        return (await GetUserPermissionsAsync(userId, cancellationToken)).First(permissionDto => permissionDto.ModuleId == moduleId);
    }

    public async Task<UserPermissionOverrideDto> RemoveUserPermissionAsync(
        int userId,
        int moduleId,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);
        await EnsureModuleExistsAsync(moduleId, cancellationToken);

        var existing = (await GetUserPermissionsAsync(userId, cancellationToken)).FirstOrDefault(permission => permission.ModuleId == moduleId)
            ?? throw new EntityNotFoundException($"User permission override {userId}/{moduleId} was not found.");

        var permission = await _securityAdminRepository.GetUserPermissionForUpdateAsync(userId, moduleId, cancellationToken)
            ?? throw new EntityNotFoundException($"User permission override {userId}/{moduleId} was not found.");

        _securityAdminRepository.RemoveUserPermission(permission);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "remove user permission override", cancellationToken);
        return existing;
    }

    public async Task<IReadOnlyList<UserActivityLogDto>> ListActivityLogsAsync(
        SecurityActivityLogFilter filter,
        CancellationToken cancellationToken = default)
    {
        filter ??= new SecurityActivityLogFilter();
        ValidateDateRange(filter.From, filter.To);
        var take = Math.Clamp(filter.Take, 1, 200);
        var logs = await _securityAdminRepository.ListActivityLogsAsync(
            filter.UserId,
            filter.ActivityTypeId,
            filter.IsSuccess,
            filter.From,
            filter.To,
            Math.Max(0, filter.Skip),
            take,
            cancellationToken);

        return logs.Select(MapActivityLog).ToList();
    }

    public async Task<UserActivityLogDto> GetActivityLogAsync(int activityLogId, CancellationToken cancellationToken = default)
    {
        var log = await _securityAdminRepository.GetActivityLogAsync(activityLogId, cancellationToken)
            ?? throw new EntityNotFoundException($"Activity log {activityLogId} was not found.");

        return MapActivityLog(log);
    }

    public async Task<IReadOnlyList<RefreshTokenMetadataDto>> GetRefreshTokensByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);
        var tokens = await _securityAdminRepository.GetRefreshTokensByUserAsync(userId, cancellationToken);
        return tokens.Select(MapRefreshToken).ToList();
    }

    public async Task<RefreshTokenMetadataDto> RevokeRefreshTokenAsync(
        int refreshTokenId,
        CancellationToken cancellationToken = default)
    {
        var token = await _securityAdminRepository.GetRefreshTokenForUpdateAsync(refreshTokenId, cancellationToken)
            ?? throw new EntityNotFoundException($"Refresh token {refreshTokenId} was not found.");

        token.RevokedAt ??= DateTime.UtcNow;
        token.RevokedByIp ??= "admin";

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "revoke refresh token", cancellationToken);

        return MapRefreshToken(new RefreshTokenMetadataQueryResult(
            token.RefreshTokenId,
            token.UserId,
            token.CreatedAt,
            token.ExpiresAt,
            token.RevokedAt,
            token.CreatedByIp,
            token.RevokedByIp,
            token.ReplacedByTokenId));
    }

    public async Task<IReadOnlyList<PermissionAuditDto>> ListPermissionAuditsAsync(
        PermissionAuditFilter filter,
        CancellationToken cancellationToken = default)
    {
        filter ??= new PermissionAuditFilter();
        ValidateDateRange(filter.From, filter.To);
        var audits = await _securityAdminRepository.ListPermissionAuditsAsync(
            filter.RoleId,
            filter.ModuleId,
            filter.From,
            filter.To,
            Math.Max(0, filter.Skip),
            Math.Clamp(filter.Take, 1, 200),
            cancellationToken);

        return audits.Select(MapPermissionAudit).ToList();
    }

    public async Task<PermissionAuditDto> GetPermissionAuditAsync(int auditId, CancellationToken cancellationToken = default)
    {
        var audit = await _securityAdminRepository.GetPermissionAuditAsync(auditId, cancellationToken)
            ?? throw new EntityNotFoundException($"Permission audit {auditId} was not found.");

        return MapPermissionAudit(audit);
    }

    private async Task EnsureNotRemovingLastAdminAsync(
        int userId,
        bool isActive,
        string accountStatusCode,
        CancellationToken cancellationToken)
    {
        var roles = await _securityAdminRepository.GetUserRolesAsync(userId, cancellationToken);
        var isAdmin = roles.Any(role => string.Equals(role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAdmin)
        {
            return;
        }

        if (isActive && string.Equals(accountStatusCode, ActiveStatusCode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var remainingAdmins = await _securityAdminRepository.CountActiveAdminUsersExceptAsync(userId, cancellationToken);
        if (remainingAdmins == 0)
        {
            throw new BusinessRuleException("Cannot leave the system without an active Admin user.");
        }
    }

    private async Task EnsureUserExistsAsync(int userId, CancellationToken cancellationToken)
    {
        if (await _securityAdminRepository.GetUserAsync(userId, cancellationToken) is null)
        {
            throw UserNotFound(userId);
        }
    }

    private async Task<Role> EnsureRoleExistsAsync(int roleId, CancellationToken cancellationToken)
    {
        return await _securityAdminRepository.GetRoleAsync(roleId, cancellationToken)
            ?? throw new EntityNotFoundException($"Role {roleId} was not found.");
    }

    private async Task EnsureModuleExistsAsync(int moduleId, CancellationToken cancellationToken)
    {
        var module = await _securityAdminRepository.GetModuleAsync(moduleId, cancellationToken)
            ?? throw new BusinessRuleException($"Module {moduleId} was not found.");

        if (!module.IsActive)
        {
            throw new BusinessRuleException($"Module {moduleId} is inactive.");
        }
    }

    private static string NormalizeUsername(string username)
    {
        return BusinessValidation.NormalizeRequiredText(username, "Username");
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new BusinessRuleException("Password is required.");
        }

        if (password.Length < 10)
        {
            throw new BusinessRuleException("Password must be at least 10 characters long.");
        }

        if (password.Any(char.IsWhiteSpace))
        {
            throw new BusinessRuleException("Password must not contain whitespace.");
        }

        if (!password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new BusinessRuleException("Password must include uppercase, lowercase, digit, and symbol characters.");
        }
    }

    private static void EnsurePermissionMask(int permissionMask)
    {
        if (permissionMask < -1)
        {
            throw new BusinessRuleException("Permission mask must be -1 for full access or non-negative.");
        }
    }

    private static void ValidateDateRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && to.Value < from.Value)
        {
            throw new BusinessRuleException("End date cannot be earlier than start date.");
        }
    }

    private static bool IsBuiltInRole(string roleName)
    {
        return BuiltInRoleNames.Any(builtInRoleName =>
            string.Equals(builtInRoleName, roleName, StringComparison.OrdinalIgnoreCase));
    }

    private static EntityNotFoundException UserNotFound(int userId)
    {
        return new EntityNotFoundException($"User {userId} was not found.");
    }

    private static SecurityUserDto MapUser(SecurityUserQueryResult user)
    {
        return new SecurityUserDto(
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
            user.Roles);
    }

    private static SecurityUserRoleDto MapUserRole(SecurityUserRoleQueryResult role)
    {
        return new SecurityUserRoleDto(
            role.RoleId,
            role.RoleName,
            role.RoleDescription,
            role.IsActive,
            role.AssignedDate,
            role.AssignedBy,
            role.AssignedByUsername);
    }

    private static RolePermissionMaskDto MapRolePermission(RolePermissionMaskQueryResult permission)
    {
        return new RolePermissionMaskDto(
            permission.RoleId,
            permission.RoleName,
            permission.ModuleId,
            permission.ModuleName,
            permission.PermissionMask);
    }

    private static UserPermissionOverrideDto MapUserPermission(UserPermissionOverrideQueryResult permission)
    {
        return new UserPermissionOverrideDto(
            permission.UserId,
            permission.Username,
            permission.ModuleId,
            permission.ModuleName,
            permission.PermissionMask);
    }

    private static UserActivityLogDto MapActivityLog(UserActivityLogQueryResult log)
    {
        return new UserActivityLogDto(
            log.ActivityLogId,
            log.UserId,
            log.Username,
            log.ActivityTypeId,
            log.ActivityTypeName,
            log.ActivityTypeCode,
            log.ActivityDetails,
            log.IpAddress,
            log.OccurredAt,
            log.IsSuccess);
    }

    private static RefreshTokenMetadataDto MapRefreshToken(RefreshTokenMetadataQueryResult token)
    {
        var now = DateTime.UtcNow;
        var isExpired = token.ExpiresAt <= now;
        var isActive = token.RevokedAt is null && !isExpired;

        return new RefreshTokenMetadataDto(
            token.RefreshTokenId,
            token.UserId,
            token.CreatedAt,
            token.ExpiresAt,
            token.RevokedAt,
            token.CreatedByIp,
            token.RevokedByIp,
            token.ReplacedByTokenId,
            isExpired,
            isActive);
    }

    private static PermissionAuditDto MapPermissionAudit(PermissionAuditQueryResult audit)
    {
        return new PermissionAuditDto(
            audit.AuditId,
            audit.RoleId,
            audit.RoleName,
            audit.ModuleId,
            audit.ModuleName,
            audit.OldMask,
            audit.NewMask,
            audit.ChangedBy,
            audit.ChangedByUsername,
            audit.ChangedDate);
    }
}
