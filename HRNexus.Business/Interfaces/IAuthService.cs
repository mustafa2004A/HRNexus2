using HRNexus.Business.Models.Auth;

namespace HRNexus.Business.Interfaces;

public interface IAuthService
{
    Task<AuthTokenResponseDto> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AuthTokenResponseDto> RefreshAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
