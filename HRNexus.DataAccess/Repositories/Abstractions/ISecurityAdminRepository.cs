using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Security;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface ISecurityAdminRepository
{
    Task<IReadOnlyList<SecurityUserQueryResult>> ListUsersAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default);
    Task<SecurityUserQueryResult?> GetUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserForUpdateAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, int? exceptUserId = null, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<AccountStatus?> GetAccountStatusAsync(int accountStatusId, CancellationToken cancellationToken = default);
    Task<AccountStatus?> GetAccountStatusByCodeAsync(string statusCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SecurityUserRoleQueryResult>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleForAssignmentAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> UserRoleExistsAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default);
    Task<UserRole?> GetUserRoleForUpdateAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    void RemoveUserRole(UserRole userRole);
    Task<int> CountActiveAdminUsersExceptAsync(int excludedUserId, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<Module?> GetModuleAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePermissionMaskQueryResult>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<RolePermission?> GetRolePermissionForUpdateAsync(int roleId, int moduleId, CancellationToken cancellationToken = default);
    Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);
    void RemoveRolePermission(RolePermission rolePermission);
    Task AddPermissionAuditAsync(PermissionAudit permissionAudit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserPermissionOverrideQueryResult>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserPermission?> GetUserPermissionForUpdateAsync(int userId, int moduleId, CancellationToken cancellationToken = default);
    Task AddUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken = default);
    void RemoveUserPermission(UserPermission userPermission);

    Task<IReadOnlyList<UserActivityLogQueryResult>> ListActivityLogsAsync(int? userId, int? activityTypeId, bool? isSuccess, DateTime? from, DateTime? to, int skip, int take, CancellationToken cancellationToken = default);
    Task<UserActivityLogQueryResult?> GetActivityLogAsync(int activityLogId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshTokenMetadataQueryResult>> GetRefreshTokensByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenForUpdateAsync(int refreshTokenId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionAuditQueryResult>> ListPermissionAuditsAsync(int? roleId, int? moduleId, DateTime? from, DateTime? to, int skip, int take, CancellationToken cancellationToken = default);
    Task<PermissionAuditQueryResult?> GetPermissionAuditAsync(int auditId, CancellationToken cancellationToken = default);
}
