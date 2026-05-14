using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Dashboard;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Dashboard;

namespace HRNexus.Business.Services;

public sealed class DashboardService : IDashboardService
{
    private const int ExpiringDocumentWindowDays = 30;
    private const int LatestLeaveRequestCount = 5;
    private const int RecentHireCount = 5;
    private const int ExpiringDocumentCount = 5;

    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var generatedAt = DateTime.UtcNow;
        var currentDate = DateOnly.FromDateTime(generatedAt);

        var summary = await _dashboardRepository.GetSummaryAsync(
            currentDate,
            ExpiringDocumentWindowDays,
            LatestLeaveRequestCount,
            RecentHireCount,
            ExpiringDocumentCount,
            cancellationToken);

        return new DashboardSummaryDto(
            MapKpis(summary.Kpis),
            summary.LatestLeaveRequests.Select(MapLeaveRequest).ToList(),
            summary.RecentHires.Select(MapRecentHire).ToList(),
            summary.EmployeesByDepartment.Select(MapDepartmentCount).ToList(),
            summary.ExpiringDocuments.Select(MapExpiringDocument).ToList(),
            generatedAt);
    }

    private static DashboardKpisDto MapKpis(DashboardKpisQueryResult kpis)
    {
        return new DashboardKpisDto(
            kpis.TotalEmployees,
            kpis.ActiveEmployees,
            kpis.PendingLeaveRequests,
            kpis.ExpiringDocumentsCount);
    }

    private static DashboardLeaveRequestItemDto MapLeaveRequest(DashboardLeaveRequestItemQueryResult leaveRequest)
    {
        return new DashboardLeaveRequestItemDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.EmployeeId,
            leaveRequest.EmployeeCode,
            leaveRequest.EmployeeName,
            leaveRequest.LeaveTypeName,
            leaveRequest.RequestStatusName,
            leaveRequest.RequestedDays,
            leaveRequest.RequestedAt);
    }

    private static DashboardRecentHireItemDto MapRecentHire(DashboardRecentHireItemQueryResult recentHire)
    {
        return new DashboardRecentHireItemDto(
            recentHire.EmployeeId,
            recentHire.EmployeeCode,
            recentHire.EmployeeName,
            recentHire.HireDate,
            recentHire.DepartmentName,
            recentHire.PositionName);
    }

    private static DashboardDepartmentCountDto MapDepartmentCount(DashboardDepartmentCountQueryResult department)
    {
        return new DashboardDepartmentCountDto(
            department.DepartmentId,
            department.DepartmentName,
            department.EmployeeCount);
    }

    private static DashboardExpiringDocumentItemDto MapExpiringDocument(DashboardExpiringDocumentItemQueryResult document)
    {
        return new DashboardExpiringDocumentItemDto(
            document.DocumentId,
            document.EmployeeId,
            document.EmployeeCode,
            document.EmployeeName,
            document.DocumentName,
            document.DocumentTypeName,
            document.ExpiryDate);
    }
}
