using HRNexus.Business.Models.Files;
using HRNexus.Business.Models.Employee;

namespace HRNexus.Business.Interfaces;

public interface IEmployeeDocumentService
{
    Task<IReadOnlyList<EmployeeDocumentItemDto>> GetEmployeeDocumentsAsync(
        int employeeId,
        CancellationToken cancellationToken = default);
    Task<EmployeeDocumentDto> GetByIdAsync(int employeeId, int documentId, CancellationToken cancellationToken = default);
    Task<EmployeeDocumentDto> UploadAsync(int employeeId, UploadEmployeeDocumentRequest request, FileUploadContent file, CancellationToken cancellationToken = default);
    Task<EmployeeDocumentDto> UpdateAsync(int employeeId, int documentId, UpdateEmployeeDocumentRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDocumentDto> DeleteAsync(int employeeId, int documentId, CancellationToken cancellationToken = default);
}
