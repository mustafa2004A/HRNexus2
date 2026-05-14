using HRNexus.Business.Models.Security;

namespace HRNexus.Business.Interfaces;

public interface ISecurityAdminService
{
    Task<IReadOnlyList<SecurityUserDto>> ListUsersAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default);
    Task<SecurityUserDto> GetUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<SecurityUserDto> CreateUserAsync(CreateSecurityUserRequest request, CancellationToken cancellationToken = default);
    Task<SecurityUserDto> UpdateUserAsync(int userId, UpdateSecurityUserRequest request, CancellationToken cancellationToken = default);
    Task<SecurityUserDto> ResetPasswordAsync(int userId, ResetSecurityUserPasswordRequest request, CancellationToken cancellationToken = default);
    Task<SecurityUserDto> LockUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<SecurityUserDto> UnlockUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SecurityUserRoleDto>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
    Task<SecurityUserRoleDto> AssignRoleAsync(int userId, AssignUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<SecurityUserRoleDto> RemoveRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RolePermissionMaskDto>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<RolePermissionMaskDto> SetRolePermissionAsync(int roleId, int moduleId, SetPermissionMaskRequest request, CancellationToken cancellationToken = default);
    Task<RolePermissionMaskDto> RemoveRolePermissionAsync(int roleId, int moduleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserPermissionOverrideDto>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserPermissionOverrideDto> SetUserPermissionAsync(int userId, int moduleId, SetPermissionMaskRequest request, CancellationToken cancellationToken = default);
    Task<UserPermissionOverrideDto> RemoveUserPermissionAsync(int userId, int moduleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserActivityLogDto>> ListActivityLogsAsync(SecurityActivityLogFilter filter, CancellationToken cancellationToken = default);
    Task<UserActivityLogDto> GetActivityLogAsync(int activityLogId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshTokenMetadataDto>> GetRefreshTokensByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<RefreshTokenMetadataDto> RevokeRefreshTokenAsync(int refreshTokenId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionAuditDto>> ListPermissionAuditsAsync(PermissionAuditFilter filter, CancellationToken cancellationToken = default);
    Task<PermissionAuditDto> GetPermissionAuditAsync(int auditId, CancellationToken cancellationToken = default);
}
