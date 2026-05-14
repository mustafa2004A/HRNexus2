namespace HRNexus.DataAccess.Repositories.Dashboard;

public sealed record DashboardSummaryQueryResult(
    DashboardKpisQueryResult Kpis,
    IReadOnlyList<DashboardLeaveRequestItemQueryResult> LatestLeaveRequests,
    IReadOnlyList<DashboardRecentHireItemQueryResult> RecentHires,
    IReadOnlyList<DashboardDepartmentCountQueryResult> EmployeesByDepartment,
    IReadOnlyList<DashboardExpiringDocumentItemQueryResult> ExpiringDocuments);

public sealed record DashboardKpisQueryResult(
    int TotalEmployees,
    int ActiveEmployees,
    int PendingLeaveRequests,
    int ExpiringDocumentsCount);

public sealed record DashboardLeaveRequestItemQueryResult(
    int LeaveRequestId,
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string LeaveTypeName,
    string RequestStatusName,
    decimal RequestedDays,
    DateTime RequestedAt);

public sealed record DashboardRecentHireItemQueryResult(
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    DateOnly HireDate,
    string? DepartmentName,
    string? PositionName);

public sealed record DashboardDepartmentCountQueryResult(
    int DepartmentId,
    string DepartmentName,
    int EmployeeCount);

public sealed record DashboardExpiringDocumentItemQueryResult(
    int DocumentId,
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string DocumentName,
    string DocumentTypeName,
    DateOnly ExpiryDate);
