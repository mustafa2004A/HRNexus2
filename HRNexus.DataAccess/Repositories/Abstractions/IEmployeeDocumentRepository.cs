using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Employee;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IEmployeeDocumentRepository
{
    Task<PagedQueryResult<EmployeeDocumentListItemQueryResult>> SearchAsync(
    string? search,
    int? employeeId,
    int? documentTypeId,
    string? verificationStatus,
    string? integrityStatus,
    string? expiryStatus,
    bool includeInactive,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeDocumentItemQueryResult>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeDocumentQueryResult?> GetByIdAsync(int employeeId, int documentId, CancellationToken cancellationToken = default);
    Task<EmployeeDocument?> GetByIdForUpdateAsync(int employeeId, int documentId, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeDocument document, CancellationToken cancellationToken = default);
}
