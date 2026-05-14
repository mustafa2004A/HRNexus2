namespace HRNexus.DataAccess.Repositories.Security;

public sealed record UserAuthQueryResult(
    int UserId,
    int? EmployeeId,
    string Username,
    string PasswordHash,
    bool IsActive,
    int FailedLoginAttempts,
    string AccountStatusCode,
    string AccountStatusName,
    IReadOnlyList<string> Roles);

public sealed record UserIdentityQueryResult(
    int UserId,
    int? EmployeeId,
    string Username,
    bool IsActive,
    string AccountStatusCode,
    string AccountStatusName,
    IReadOnlyList<string> Roles);

public sealed record UserPermissionQueryResult(
    string ModuleName,
    int PermissionMask,
    string Source);
