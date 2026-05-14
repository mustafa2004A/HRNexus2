using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Leave;

public sealed class LeaveAttachmentRepository : ILeaveAttachmentRepository
{
    private readonly HRNexusDbContext _dbContext;

    public LeaveAttachmentRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(LeaveAttachment leaveAttachment, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveAttachments.AddAsync(leaveAttachment, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<LeaveAttachment>> GetByLeaveRequestAsync(int leaveRequestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaveAttachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .Where(x => x.LeaveRequestId == leaveRequestId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<LeaveAttachment?> GetByIdAsync(int leaveAttachmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveAttachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.LeaveAttachmentId == leaveAttachmentId, cancellationToken);
    }

    public Task<LeaveAttachment?> GetByIdForUpdateAsync(int leaveAttachmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveAttachments
            .FirstOrDefaultAsync(x => x.LeaveAttachmentId == leaveAttachmentId, cancellationToken);
    }
}
