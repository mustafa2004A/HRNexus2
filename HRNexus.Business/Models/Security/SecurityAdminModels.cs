using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Security;

public sealed record SecurityUserDto(
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

public sealed class CreateSecurityUserRequest
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AccountStatusId { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateSecurityUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AccountStatusId { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class ResetSecurityUserPasswordRequest
{
    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;
}

public sealed record SecurityUserRoleDto(
    int RoleId,
    string RoleName,
    string? RoleDescription,
    bool IsActive,
    DateTime AssignedDate,
    int? AssignedBy,
    string? AssignedByUsername);

public sealed class AssignUserRoleRequest
{
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }
}

public sealed record RolePermissionMaskDto(
    int RoleId,
    string RoleName,
    int ModuleId,
    string ModuleName,
    int PermissionMask);

public sealed record UserPermissionOverrideDto(
    int UserId,
    string Username,
    int ModuleId,
    string ModuleName,
    int PermissionMask);

public sealed class SetPermissionMaskRequest
{
    [Range(-1, int.MaxValue)]
    public int PermissionMask { get; set; }
}

public sealed class SecurityActivityLogFilter
{
    [Range(1, int.MaxValue)]
    public int? UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int? ActivityTypeId { get; set; }

    public bool? IsSuccess { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    [Range(0, int.MaxValue)]
    public int Skip { get; set; }

    [Range(1, 200)]
    public int Take { get; set; } = 50;
}

public sealed record UserActivityLogDto(
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

public sealed record RefreshTokenMetadataDto(
    int RefreshTokenId,
    int UserId,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string? CreatedByIp,
    string? RevokedByIp,
    int? ReplacedByTokenId,
    bool IsExpired,
    bool IsActive);

public sealed record PermissionAuditDto(
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

public sealed class PermissionAuditFilter
{
    [Range(1, int.MaxValue)]
    public int? RoleId { get; set; }

    [Range(1, int.MaxValue)]
    public int? ModuleId { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    [Range(0, int.MaxValue)]
    public int Skip { get; set; }

    [Range(1, 200)]
    public int Take { get; set; } = 50;
}
