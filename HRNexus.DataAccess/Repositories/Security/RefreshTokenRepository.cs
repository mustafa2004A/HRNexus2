using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Security;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly HRNexusDbContext _dbContext;

    public RefreshTokenRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool asTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<RefreshToken> query = _dbContext.RefreshTokens
            .Include(x => x.User)
                .ThenInclude(x => x.AccountStatus)
            .Where(x => x.TokenHash == tokenHash);

        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }
}
