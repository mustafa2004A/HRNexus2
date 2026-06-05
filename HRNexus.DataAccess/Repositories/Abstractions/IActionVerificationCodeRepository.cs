using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IActionVerificationCodeRepository
{
    Task AddAsync(ActionVerificationCode verificationCode, CancellationToken cancellationToken = default);
    Task<ActionVerificationCode?> GetByIdForUpdateAsync(Guid verificationCodeId, CancellationToken cancellationToken = default);
}
