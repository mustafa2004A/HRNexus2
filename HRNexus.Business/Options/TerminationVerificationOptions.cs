namespace HRNexus.Business.Options;

public sealed class TerminationVerificationOptions
{
    public bool Enabled { get; set; } = true;
    public int CodeLength { get; set; } = 6;
    public int ExpiryMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
    public string PreferredDeliveryMethod { get; set; } = "Email";
    public string? FallbackEmail { get; set; }
    public string? FallbackPhoneNumber { get; set; }
}
