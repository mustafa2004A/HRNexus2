namespace HRNexus.Business.Models.Employee;

public sealed record EmployeeDocumentItemDto(
    int DocumentId,
    int DocumentTypeId,
    string DocumentName,
    string DocumentTypeName,
    string? ReferenceNumber,
    string? FileExtension,
    int? FileStorageItemId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    bool IsVerified,
    string? VerifiedByUsername,
    DateTime? VerifiedDate,
    bool IsActive,
    DateTime UploadedDate,
    string? OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    string IntegrityStatus,
    DateTime? LastIntegrityCheckAt);

public sealed class EmployeeDocumentListItemDto
{
    public int DocumentId { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeFullName { get; init; } = string.Empty;

    public int DocumentTypeId { get; init; }
    public string DocumentTypeName { get; init; } = string.Empty;

    public string DocumentName { get; init; } = string.Empty;
    public string? ReferenceNumber { get; init; }

    public string? FileExtension { get; init; }
    public int? FileStorageItemId { get; init; }

    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }

    public bool IsVerified { get; init; }
    public bool IsActive { get; init; }

    public DateTime UploadedDate { get; init; }

    public string? OriginalFileName { get; init; }
    public string? ContentType { get; init; }
    public long? FileSizeBytes { get; init; }

    public string IntegrityStatus { get; init; } = "Unknown";
    public DateTime? LastIntegrityCheckAt { get; init; }
}

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}