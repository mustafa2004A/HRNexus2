using HRNexus.Business.Security;

namespace HRNexus.Business.Interfaces;

public interface IPasswordVerificationService
{
    PasswordVerificationResult Verify(string password, string passwordHash);
}
