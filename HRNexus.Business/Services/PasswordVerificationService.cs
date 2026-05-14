using HRNexus.Business.Interfaces;
using HRNexus.Business.Options;
using HRNexus.Business.Security;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Options;

namespace HRNexus.Business.Services;

public sealed class PasswordVerificationService : IPasswordVerificationService, IPasswordHashingService
{
    private readonly AuthSecurityOptions _options;

    public PasswordVerificationService(IOptions<AuthSecurityOptions> options)
    {
        _options = options.Value;
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return PasswordVerificationResult.Failed("Missing credentials.");
        }

        if (passwordHash.StartsWith("$argon2", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Argon2.Verify(passwordHash, password)
                    ? PasswordVerificationResult.Success()
                    : PasswordVerificationResult.Failed("Password mismatch.");
            }
            catch
            {
                return PasswordVerificationResult.Failed("Invalid Argon2 password hash format.");
            }
        }

        return PasswordVerificationResult.Failed("Unsupported password hash format.");
    }

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return Argon2.Hash(
            password,
            Math.Max(1, _options.Argon2TimeCost),
            Math.Max(8192, _options.Argon2MemoryCost),
            Math.Max(1, _options.Argon2Parallelism),
            Argon2Type.HybridAddressing,
            Math.Max(16, _options.Argon2HashLength));
    }
}
