namespace HRNexus.Business.Models.Dashboard;

public sealed record DashboardLeaveRequestItemDto(
    int LeaveRequestId,
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string LeaveTypeName,
    string RequestStatusName,
    decimal RequestedDays,
    DateTime RequestedAt);
