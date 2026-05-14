namespace HRNexus.Business.Models.Employee;

public sealed record EmployeeCurrentContextDto(
    int EmployeeId,
    string EmployeeCode,
    string FullName,
    string? DepartmentName,
    string? PositionName,
    string? EmploymentTypeName,
    string EmploymentStatusName,
    int? ManagerId,
    string? ManagerName,
    DateOnly? CurrentAssignmentStartDate);
