using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IActivityTypeRepository
{
    Task<ActivityType?> GetByCodeAsync(string activityTypeCode, CancellationToken cancellationToken = default);
}
