using HRNexus.Business.Models.Dashboard;

namespace HRNexus.Business.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
