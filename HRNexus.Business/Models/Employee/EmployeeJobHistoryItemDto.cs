namespace HRNexus.Business.Models.Employee;

public sealed record EmployeeJobHistoryItemDto(
    int JobHistoryId,
    string DepartmentName,
    string PositionName,
    string EmploymentTypeName,
    string EmploymentStatusName,
    string? ManagerName,
    bool IsCurrent,
    DateOnly StartDate,
    DateOnly? EndDate);
