namespace HRNexus.Business.Models.Dashboard;

public sealed record DashboardDepartmentCountDto(
    int DepartmentId,
    string DepartmentName,
    int EmployeeCount);
