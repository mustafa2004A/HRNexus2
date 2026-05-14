namespace HRNexus.DataAccess.Entities.Security;

public sealed class UserActivityLog
{
    public int ActivityLogId { get; set; }
    public int? UserId { get; set; }
    public int ActivityTypeId { get; set; }
    public string? ActivityDetails { get; set; }
    public string? IpAddress { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsSuccess { get; set; }

    public User? User { get; set; }
    public ActivityType ActivityType { get; set; } = null!;
}
