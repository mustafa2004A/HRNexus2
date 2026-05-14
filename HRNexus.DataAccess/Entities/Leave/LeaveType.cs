namespace HRNexus.DataAccess.Entities.Leave;

public sealed class LeaveType
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultDaysPerYear { get; set; }
    public bool IsPaid { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
