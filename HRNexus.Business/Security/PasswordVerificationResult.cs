namespace HRNexus.Business.Security;

public sealed record PasswordVerificationResult(
    bool Succeeded,
    string? FailureReason)
{
    public static PasswordVerificationResult Success() => new(true, null);
    public static PasswordVerificationResult Failed(string? failureReason = null) => new(false, failureReason);
}
