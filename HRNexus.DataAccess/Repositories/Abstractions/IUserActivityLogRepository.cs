using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IUserActivityLogRepository
{
    Task AddAsync(UserActivityLog activityLog, CancellationToken cancellationToken = default);
}
