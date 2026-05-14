using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Employee;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IEmployeeRepository
{
    Task<EmployeeEntity?> GetByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeSummaryQueryResult>> ListAsync(string? search, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsQueryResult?> GetDetailsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeCurrentContextQueryResult?> GetCurrentContextAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeEntity?> GetByIdForUpdateAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<int> GetNextEmployeeCodeNumberAsync(CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeEntity employee, CancellationToken cancellationToken = default);
}
