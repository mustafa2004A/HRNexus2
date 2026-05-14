namespace HRNexus.Business.Options;

public sealed class FileStorageOptions
{
    public string RootPath { get; set; } = @"C:\HRNexusStorage";
    public string LeaveAttachmentsFolder { get; set; } = "LeaveAttachments";
    public string PersonPhotosFolder { get; set; } = "PersonPhotos";
    public string EmployeeDocumentsFolder { get; set; } = "EmployeeDocuments";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedAttachmentExtensions { get; set; } =
    [
        ".pdf",
        ".doc",
        ".docx",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".txt"
    ];

    public string[] AllowedPhotoExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];
}
