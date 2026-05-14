using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Auth;

public sealed class LoginRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;
}

public sealed class RefreshTokenRequest
{
    [Required]
    [StringLength(512)]
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LogoutRequest
{
    [StringLength(512)]
    public string? RefreshToken { get; set; }
}

public sealed record AuthenticatedUserDto(
    int UserId,
    int? EmployeeId,
    string Username,
    IReadOnlyList<string> Roles);

public sealed record AuthTokenResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AuthenticatedUserDto User);

public sealed record CurrentUserDto(
    int UserId,
    int? EmployeeId,
    string Username,
    IReadOnlyList<string> Roles,
    IReadOnlyList<UserPermissionDto> Permissions);

public sealed record UserPermissionDto(
    string ModuleName,
    int PermissionMask,
    string Source);

public sealed record AccessTokenUser(
    int UserId,
    int? EmployeeId,
    string Username,
    IReadOnlyList<string> Roles);

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAt);

public sealed record RefreshTokenResult(
    string Token,
    DateTime ExpiresAt);
