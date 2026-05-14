namespace HRNexus.DataAccess.Repositories.Security;

public sealed record SecurityUserQueryResult(
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
    DateTime? ModifiedDate,
    IReadOnlyList<string> Roles);

public sealed record SecurityUserRoleQueryResult(
    int RoleId,
    string RoleName,
    string? RoleDescription,
    bool IsActive,
    DateTime AssignedDate,
    int? AssignedBy,
    string? AssignedByUsername);

public sealed record RolePermissionMaskQueryResult(
    int RoleId,
    string RoleName,
    int ModuleId,
    string ModuleName,
    int PermissionMask);

public sealed record UserPermissionOverrideQueryResult(
    int UserId,
    string Username,
    int ModuleId,
    string ModuleName,
    int PermissionMask);

public sealed record UserActivityLogQueryResult(
    int ActivityLogId,
    int? UserId,
    string? Username,
    int ActivityTypeId,
    string ActivityTypeName,
    string ActivityTypeCode,
    string? ActivityDetails,
    string? IpAddress,
    DateTime OccurredAt,
    bool IsSuccess);

public sealed record RefreshTokenMetadataQueryResult(
    int RefreshTokenId,
    int UserId,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string? CreatedByIp,
    string? RevokedByIp,
    int? ReplacedByTokenId);

public sealed record PermissionAuditQueryResult(
    int AuditId,
    int RoleId,
    string RoleName,
    int ModuleId,
    string ModuleName,
    int OldMask,
    int NewMask,
    int ChangedBy,
    string ChangedByUsername,
    DateTime ChangedDate);
