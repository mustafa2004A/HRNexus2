using HRNexus.DataAccess.Entities.Leave;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface ILeaveAttachmentRepository
{
    Task AddAsync(LeaveAttachment leaveAttachment, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveAttachment>> GetByLeaveRequestAsync(int leaveRequestId, CancellationToken cancellationToken = default);
    Task<LeaveAttachment?> GetByIdAsync(int leaveAttachmentId, CancellationToken cancellationToken = default);
    Task<LeaveAttachment?> GetByIdForUpdateAsync(int leaveAttachmentId, CancellationToken cancellationToken = default);
}
