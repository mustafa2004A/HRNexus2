namespace HRNexus.Business.Options;

public sealed class AuthSecurityOptions
{
    public int RefreshTokenExpirationDays { get; set; } = 14;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int Argon2TimeCost { get; set; } = 3;
    public int Argon2MemoryCost { get; set; } = 65536;
    public int Argon2Parallelism { get; set; } = 1;
    public int Argon2HashLength { get; set; } = 32;
}
