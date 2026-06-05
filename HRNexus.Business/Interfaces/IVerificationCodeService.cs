using HRNexus.Business.Models.Security;

namespace HRNexus.Business.Interfaces;

public interface IVerificationCodeService
{
    Task<ActionVerificationCodeIssueResult> IssueCodeAsync(
        ActionVerificationCodeIssueRequest request,
        CancellationToken cancellationToken = default);

    Task VerifyCodeAsync(
        ActionVerificationCodeVerifyRequest request,
        CancellationToken cancellationToken = default);
}
