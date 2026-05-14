namespace HRNexus.Business.Models.Dashboard;

public sealed record DashboardSummaryDto(
    DashboardKpisDto Kpis,
    IReadOnlyList<DashboardLeaveRequestItemDto> LatestLeaveRequests,
    IReadOnlyList<DashboardRecentHireItemDto> RecentHires,
    IReadOnlyList<DashboardDepartmentCountDto> EmployeesByDepartment,
    IReadOnlyList<DashboardExpiringDocumentItemDto> ExpiringDocuments,
    DateTime GeneratedAt);
