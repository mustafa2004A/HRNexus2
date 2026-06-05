namespace HRNexus.DataAccess.Entities.Security;

public sealed class ActionVerificationCode
{
    public Guid ActionVerificationCodeId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string TargetEntityType { get; set; } = string.Empty;
    public int TargetEntityId { get; set; }
    public int RequestedByUserId { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    public string? DestinationMasked { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public User RequestedByUser { get; set; } = null!;
}
