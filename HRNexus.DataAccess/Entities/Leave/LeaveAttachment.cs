using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Entities.Core;

namespace HRNexus.DataAccess.Entities.Leave;

public sealed class LeaveAttachment
{
    public int LeaveAttachmentId { get; set; }
    public int LeaveRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileExtension { get; set; }
    public int? FileStorageItemId { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsActive { get; set; }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public User UploadedByUser { get; set; } = null!;
    public FileStorageItem? FileStorageItem { get; set; }
}
