using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Entities.Employee;

public sealed class EmployeeDocument
{
    public int DocumentId { get; set; }
    public int EmployeeId { get; set; }
    public int DocumentTypeId { get; set; }
    public int? FileStorageItemId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public bool IsVerified { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public bool IsActive { get; set; }
    public int? UploadedBy { get; set; }
    public DateTime UploadedDate { get; set; }
    public string? Remarks { get; set; }

    public Employee Employee { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
    public FileStorageItem? FileStorageItem { get; set; }
    public User? VerifiedByUser { get; set; }
    public User? UploadedByUser { get; set; }
}
