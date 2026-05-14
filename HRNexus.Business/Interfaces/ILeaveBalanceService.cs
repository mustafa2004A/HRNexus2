using HRNexus.Business.Models.Leave;

namespace HRNexus.Business.Interfaces;

public interface ILeaveBalanceService
{
    Task<IReadOnlyList<LeaveBalanceDto>> ListBalancesAsync(int? employeeId = null, int? leaveTypeId = null, int? balanceYear = null, CancellationToken cancellationToken = default);
    Task<LeaveBalanceDto> GetBalanceAsync(int leaveBalanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveBalanceDto>> GetEmployeeBalancesAsync(int employeeId, int? balanceYear = null, CancellationToken cancellationToken = default);
    Task<LeaveBalanceDto> UpsertBalanceAsync(UpsertLeaveBalanceRequest request, CancellationToken cancellationToken = default);
}
