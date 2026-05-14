using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool asTracking = false, CancellationToken cancellationToken = default);
}
