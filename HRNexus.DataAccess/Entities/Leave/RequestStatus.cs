namespace HRNexus.DataAccess.Entities.Leave;

public sealed class RequestStatus
{
    public int RequestStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFinalState { get; set; }
    public bool IsActive { get; set; }

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
