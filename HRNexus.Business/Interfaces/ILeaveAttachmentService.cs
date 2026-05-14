using HRNexus.Business.Models.Files;
using HRNexus.Business.Models.Leave;

namespace HRNexus.Business.Interfaces;

public interface ILeaveAttachmentService
{
    Task<LeaveAttachmentDto> UploadAttachmentAsync(int leaveRequestId, FileUploadContent file, int? uploadedByUserId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveAttachmentDto>> GetLeaveRequestAttachmentsAsync(int leaveRequestId, CancellationToken cancellationToken = default);
    Task<LeaveAttachmentDto> GetAttachmentAsync(int leaveAttachmentId, CancellationToken cancellationToken = default);
    Task<LeaveAttachmentDto> DeactivateAttachmentAsync(int leaveAttachmentId, CancellationToken cancellationToken = default);
}
