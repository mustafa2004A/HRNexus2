using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.DataAccess.Repositories.Security;

public sealed class UserActivityLogRepository : IUserActivityLogRepository
{
    private readonly HRNexusDbContext _dbContext;

    public UserActivityLogRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(UserActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserActivityLogs.AddAsync(activityLog, cancellationToken).AsTask();
    }
}
