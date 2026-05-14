using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Leave;

public sealed class RequestStatusRepository : IRequestStatusRepository
{
    private readonly HRNexusDbContext _dbContext;

    public RequestStatusRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RequestStatus>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.RequestStatuses
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.RequestStatusId)
            .ToListAsync(cancellationToken);
    }

    public Task<RequestStatus?> GetByIdAsync(int requestStatusId, CancellationToken cancellationToken = default)
    {
        return _dbContext.RequestStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequestStatusId == requestStatusId, cancellationToken);
    }

    public Task<RequestStatus?> GetByCodeAsync(string statusCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.RequestStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StatusCode == statusCode, cancellationToken);
    }
}
