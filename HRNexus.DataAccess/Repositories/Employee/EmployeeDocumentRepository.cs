using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Employee;

public sealed class EmployeeDocumentRepository : IEmployeeDocumentRepository
{
    private readonly HRNexusDbContext _dbContext;

    public EmployeeDocumentRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedQueryResult<EmployeeDocumentListItemQueryResult>> SearchAsync(
    string? search,
    int? employeeId,
    int? documentTypeId,
    string? verificationStatus,
    string? integrityStatus,
    string? expiryStatus,
    bool includeInactive,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var nearExpiryUntil = today.AddDays(30);

        var query = _dbContext.EmployeeDocuments
            .AsNoTracking()
            .Where(document =>
                !document.Employee.IsDeleted &&
                !document.Employee.Person.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(document => document.IsActive);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(document => document.EmployeeId == employeeId.Value);
        }

        if (documentTypeId.HasValue)
        {
            query = query.Where(document => document.DocumentTypeId == documentTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(verificationStatus))
        {
            var status = verificationStatus.Trim().ToLowerInvariant();

            if (status == "verified")
            {
                query = query.Where(document => document.IsVerified);
            }
            else if (status == "unverified")
            {
                query = query.Where(document => !document.IsVerified);
            }
        }

        if (!string.IsNullOrWhiteSpace(expiryStatus))
        {
            var status = expiryStatus.Trim().ToLowerInvariant();

            if (status == "expired")
            {
                query = query.Where(document =>
                    document.ExpiryDate.HasValue &&
                    document.ExpiryDate.Value < today);
            }
            else if (status == "near-expiry")
            {
                query = query.Where(document =>
                    document.ExpiryDate.HasValue &&
                    document.ExpiryDate.Value >= today &&
                    document.ExpiryDate.Value <= nearExpiryUntil);
            }
            else if (status == "no-expiry")
            {
                query = query.Where(document => !document.ExpiryDate.HasValue);
            }
            else if (status == "valid")
            {
                query = query.Where(document =>
                    document.ExpiryDate.HasValue &&
                    document.ExpiryDate.Value > nearExpiryUntil);
            }
        }

        if (!string.IsNullOrWhiteSpace(integrityStatus))
        {
            var status = integrityStatus.Trim().ToLowerInvariant();

            if (status == "unknown")
            {
                query = query.Where(document =>
                    document.FileStorageItem == null ||
                    document.FileStorageItem.LastIntegrityCheckAt == null);
            }
            else if (status == "passed")
            {
                query = query.Where(document =>
                    document.FileStorageItem != null &&
                    document.FileStorageItem.LastIntegrityCheckAt != null);
            }
            else if (status == "failed")
            {
                query = query.Where(document => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();

            query = query.Where(document =>
                document.DocumentName.Contains(trimmedSearch) ||
                document.Employee.EmployeeCode.Contains(trimmedSearch) ||
                document.Employee.Person.FullName.Contains(trimmedSearch) ||
                (document.ReferenceNumber != null &&
                 document.ReferenceNumber.Contains(trimmedSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(document => document.UploadedDate)
            .ThenBy(document => document.Employee.Person.FullName)
            .ThenBy(document => document.DocumentName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(document => new EmployeeDocumentListItemQueryResult(
                document.DocumentId,
                document.EmployeeId,
                document.Employee.EmployeeCode,
                document.Employee.Person.FullName,
                document.DocumentTypeId,
                document.DocumentType.Name,
                document.DocumentName,
                document.ReferenceNumber,
                document.FileExtension,
                document.FileStorageItemId,
                document.IssueDate,
                document.ExpiryDate,
                document.IsVerified,
                document.IsActive,
                document.UploadedDate,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.OriginalFileName,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.ContentType,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.FileSizeBytes,
                document.FileStorageItem == null
                    ? "Unknown"
                    : document.FileStorageItem.LastIntegrityCheckAt == null
                        ? "Unknown"
                        : "Passed",
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.LastIntegrityCheckAt))
            .ToListAsync(cancellationToken);

        return new PagedQueryResult<EmployeeDocumentListItemQueryResult>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }
    public async Task<IReadOnlyList<EmployeeDocumentItemQueryResult>> GetByEmployeeAsync(
    int employeeId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmployeeDocuments
            .AsNoTracking()
            .Where(document => document.EmployeeId == employeeId)
            .OrderByDescending(document => document.IsActive)
            .ThenByDescending(document => document.UploadedDate)
            .ThenBy(document => document.DocumentName)
            .Select(document => new EmployeeDocumentItemQueryResult(
                document.DocumentId,
                document.DocumentTypeId,
                document.DocumentName,
                document.DocumentType.Name,
                document.ReferenceNumber,
                document.FileExtension,
                document.FileStorageItemId,
                document.IssueDate,
                document.ExpiryDate,
                document.IsVerified,
                document.VerifiedByUser == null ? null : document.VerifiedByUser.Username,
                document.VerifiedDate,
                document.IsActive,
                document.UploadedDate,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.OriginalFileName,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.ContentType,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.FileSizeBytes,
                document.FileStorageItem == null
                    ? "Unknown"
                    : document.FileStorageItem.LastIntegrityCheckAt == null
                        ? "Unknown"
                        : "Passed",
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.LastIntegrityCheckAt))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeDocumentQueryResult?> GetByIdAsync(
     int employeeId,
     int documentId,
     CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeDocuments
            .AsNoTracking()
            .Where(document =>
                document.EmployeeId == employeeId &&
                document.DocumentId == documentId)
            .Select(document => new EmployeeDocumentQueryResult(
                document.DocumentId,
                document.EmployeeId,
                document.DocumentTypeId,
                document.DocumentType.Name,
                document.DocumentName,
                document.ReferenceNumber,
                document.FilePath,
                document.FileExtension,
                document.FileStorageItemId,
                document.IssueDate,
                document.ExpiryDate,
                document.IsVerified,
                document.VerifiedBy,
                document.VerifiedByUser == null ? null : document.VerifiedByUser.Username,
                document.VerifiedDate,
                document.IsActive,
                document.UploadedBy,
                document.UploadedByUser == null ? null : document.UploadedByUser.Username,
                document.UploadedDate,
                document.Remarks,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.OriginalFileName,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.ContentType,
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.FileSizeBytes,
                document.FileStorageItem == null
                    ? "Unknown"
                    : document.FileStorageItem.LastIntegrityCheckAt == null
                        ? "Unknown"
                        : "Passed",
                document.FileStorageItem == null
                    ? null
                    : document.FileStorageItem.LastIntegrityCheckAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<EmployeeDocument?> GetByIdForUpdateAsync(
        int employeeId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeDocuments
            .FirstOrDefaultAsync(document => document.EmployeeId == employeeId && document.DocumentId == documentId, cancellationToken);
    }

    public Task AddAsync(EmployeeDocument document, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeDocuments.AddAsync(document, cancellationToken).AsTask();
    }
}
