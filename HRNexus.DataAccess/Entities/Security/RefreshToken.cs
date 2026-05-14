namespace HRNexus.DataAccess.Entities.Security;

public sealed class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public int? ReplacedByTokenId { get; set; }

    public User User { get; set; } = null!;
    public RefreshToken? ReplacedByToken { get; set; }
}
