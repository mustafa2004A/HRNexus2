using HRNexus.DataAccess.Repositories.Dashboard;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IDashboardRepository
{
    Task<DashboardSummaryQueryResult> GetSummaryAsync(
        DateOnly currentDate,
        int expiringWithinDays,
        int latestLeaveRequestCount,
        int recentHireCount,
        int expiringDocumentCount,
        CancellationToken cancellationToken = default);
}
