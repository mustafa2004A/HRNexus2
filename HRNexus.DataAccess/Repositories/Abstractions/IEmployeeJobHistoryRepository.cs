using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Employee;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IEmployeeJobHistoryRepository
{
    Task<IReadOnlyList<EmployeeJobHistoryItemQueryResult>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeJobHistoryQueryResult>> GetOperationalByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeJobHistoryQueryResult?> GetByIdAsync(int employeeId, int jobHistoryId, CancellationToken cancellationToken = default);
    Task<EmployeeJobHistory?> GetByIdForUpdateAsync(int employeeId, int jobHistoryId, CancellationToken cancellationToken = default);
    Task<EmployeeJobHistory?> GetCurrentForUpdateAsync(int employeeId, int? exceptJobHistoryId = null, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeJobHistory jobHistory, CancellationToken cancellationToken = default);
    void Remove(EmployeeJobHistory jobHistory);
}
