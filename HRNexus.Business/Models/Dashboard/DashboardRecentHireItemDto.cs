namespace HRNexus.Business.Models.Dashboard;

public sealed record DashboardRecentHireItemDto(
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    DateOnly HireDate,
    string? DepartmentName,
    string? PositionName);
