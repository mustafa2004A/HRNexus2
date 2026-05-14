namespace HRNexus.Business.Models.Leave;

public sealed record LeaveAttachmentDto(
    int LeaveAttachmentId,
    int LeaveRequestId,
    string FileName,
    string FilePath,
    string? FileExtension,
    int? FileStorageItemId,
    int UploadedByUserId,
    string UploadedByUsername,
    DateTime UploadedAt,
    bool IsActive);
