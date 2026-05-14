namespace HRNexus.Business.Models.Employee;

public sealed record EmployeeDocumentItemDto(
    int DocumentId,
    string DocumentName,
    string DocumentTypeName,
    string? ReferenceNumber,
    string FileExtension,
    int? FileStorageItemId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    bool IsVerified,
    string? VerifiedByUsername,
    DateTime? VerifiedDate,
    bool IsActive,
    DateTime UploadedDate);
