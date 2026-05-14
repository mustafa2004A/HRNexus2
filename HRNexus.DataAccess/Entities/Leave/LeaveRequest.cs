using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Security;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Entities.Leave;

public sealed class LeaveRequest
{
    public int LeaveRequestId { get; set; }
    public int LeaveTypeId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal RequestedDays { get; set; }
    public string? Reason { get; set; }
    public int RequestStatusId { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public LeaveType LeaveType { get; set; } = null!;
    public EmployeeEntity Employee { get; set; } = null!;
    public RequestStatus RequestStatus { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
    public ICollection<LeaveAttachment> Attachments { get; set; } = new List<LeaveAttachment>();
}
