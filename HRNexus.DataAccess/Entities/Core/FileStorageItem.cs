using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Entities.Core;

public sealed class FileStorageItem
{
    public int FileStorageItemId { get; set; }
    public string FileCategory { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileHashSha256 { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "SHA-256";
    public int? UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastIntegrityCheckAt { get; set; }

    public User? UploadedByUser { get; set; }
    public ICollection<LeaveAttachment> LeaveAttachments { get; set; } = new List<LeaveAttachment>();
    public ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();
    public ICollection<Person> PeopleUsingAsPhoto { get; set; } = new List<Person>();
}
