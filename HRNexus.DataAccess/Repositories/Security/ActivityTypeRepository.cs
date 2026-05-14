using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Security;

public sealed class ActivityTypeRepository : IActivityTypeRepository
{
    private readonly HRNexusDbContext _dbContext;

    public ActivityTypeRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ActivityType?> GetByCodeAsync(string activityTypeCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.ActivityTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ActivityTypeCode == activityTypeCode && x.IsActive, cancellationToken);
    }
}
