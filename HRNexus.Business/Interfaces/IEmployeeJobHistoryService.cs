using HRNexus.Business.Models.Employee;

namespace HRNexus.Business.Interfaces;

public interface IEmployeeJobHistoryService
{
    Task<IReadOnlyList<EmployeeJobHistoryItemDto>> GetEmployeeJobHistoryAsync(
        int employeeId,
        CancellationToken cancellationToken = default);
    Task<EmployeeJobHistoryDto> GetByIdAsync(int employeeId, int jobHistoryId, CancellationToken cancellationToken = default);
    Task<EmployeeJobHistoryDto> CreateAsync(int employeeId, CreateEmployeeJobHistoryRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeJobHistoryDto> UpdateAsync(int employeeId, int jobHistoryId, UpdateEmployeeJobHistoryRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeJobHistoryDto> DeleteAsync(int employeeId, int jobHistoryId, CancellationToken cancellationToken = default);
}
