using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Files;

public static class FileStorageCategories
{
    public const string LeaveAttachment = "LeaveAttachment";
    public const string PersonPhoto = "PersonPhoto";
    public const string EmployeeDocument = "EmployeeDocument";
}

public sealed record FileUploadContent(
    Stream Content,
    string OriginalFileName,
    string? ContentType,
    long Length);

public sealed record FileStorageItemDto(
    int FileStorageItemId,
    string FileCategory,
    string OriginalFileName,
    string StoredFileName,
    string RelativePath,
    string? ContentType,
    string FileExtension,
    long FileSizeBytes,
    string FileHashSha256,
    string HashAlgorithm,
    int? UploadedByUserId,
    DateTime UploadedAt,
    bool IsActive,
    DateTime? LastIntegrityCheckAt);

public sealed record FileIntegrityVerificationResultDto(
    int FileStorageItemId,
    string FileCategory,
    string RelativePath,
    bool FileExists,
    string HashAlgorithm,
    bool IsHashValid,
    bool IsSizeValid,
    bool IsIntegrityValid,
    DateTime CheckedAt);

public sealed class UploadLeaveAttachmentRequest
{
    [Range(1, int.MaxValue)]
    public int LeaveRequestId { get; set; }

    [Range(1, int.MaxValue)]
    public int? UploadedByUserId { get; set; }
}
