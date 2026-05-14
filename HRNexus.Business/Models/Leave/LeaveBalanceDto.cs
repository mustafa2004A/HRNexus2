namespace HRNexus.Business.Models.Leave;

public sealed record LeaveBalanceDto(
    int LeaveBalanceId,
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    int LeaveTypeId,
    string LeaveTypeName,
    string LeaveTypeCode,
    int BalanceYear,
    decimal EntitledDays,
    decimal UsedDays,
    decimal RemainingDays,
    DateTime LastUpdated);
