using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Security;

public sealed class ActionVerificationCodeRepository : IActionVerificationCodeRepository
{
    private readonly HRNexusDbContext _dbContext;

    public ActionVerificationCodeRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ActionVerificationCode verificationCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.ActionVerificationCodes.AddAsync(verificationCode, cancellationToken).AsTask();
    }

    public Task<ActionVerificationCode?> GetByIdForUpdateAsync(
        Guid verificationCodeId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ActionVerificationCodes
            .FirstOrDefaultAsync(
                verificationCode => verificationCode.ActionVerificationCodeId == verificationCodeId,
                cancellationToken);
    }
}
