using HRNexus.Business.Models.Employee;

namespace HRNexus.Business.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeSummaryDto>> ListAsync(string? search, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsDto> GetDetailsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeCurrentContextDto> GetCurrentContextAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsDto> UpdateAsync(int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsDto> DeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
