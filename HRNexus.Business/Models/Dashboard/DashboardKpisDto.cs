namespace HRNexus.Business.Models.Dashboard;

public sealed record DashboardKpisDto(
    int TotalEmployees,
    int ActiveEmployees,
    int PendingLeaveRequests,
    int ExpiringDocumentsCount);
