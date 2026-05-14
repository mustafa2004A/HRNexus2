using HRNexus.DataAccess.Entities.Leave;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface ILeaveBalanceRepository
{
    Task<IReadOnlyList<LeaveBalance>> ListAsync(int? employeeId = null, int? leaveTypeId = null, int? balanceYear = null, CancellationToken cancellationToken = default);
    Task<LeaveBalance?> GetByIdAsync(int leaveBalanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveBalance>> GetByEmployeeAsync(int employeeId, int? balanceYear = null, CancellationToken cancellationToken = default);
    Task<LeaveBalance?> GetByEmployeeLeaveTypeYearAsync(int employeeId, int leaveTypeId, int balanceYear, bool asTracking = false, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default);
}
