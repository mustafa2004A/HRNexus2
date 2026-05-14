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
                document.UploadedDate))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeDocumentQueryResult?> GetByIdAsync(
        int employeeId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeDocuments
            .AsNoTracking()
            .Where(document => document.EmployeeId == employeeId && document.DocumentId == documentId)
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
                document.Remarks))
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
