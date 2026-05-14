using HRNexus.Business.Models.Auth;
using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.Business.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(int userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<(RefreshToken ExistingToken, RefreshTokenResult NewToken)> RotateAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task<int?> RevokeAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
}
