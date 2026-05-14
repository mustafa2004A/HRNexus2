namespace HRNexus.Business.Models.Leave;

public sealed record LeaveTypeDto(
    int LeaveTypeId,
    string LeaveTypeName,
    string LeaveTypeCode,
    string? Description,
    decimal DefaultDaysPerYear,
    bool IsPaid,
    bool RequiresApproval,
    bool IsActive);
