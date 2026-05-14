using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Leave;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface ILeaveRequestRepository
{
    Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int leaveRequestId, CancellationToken cancellationToken = default);
    Task<int?> GetEmployeeIdAsync(int leaveRequestId, CancellationToken cancellationToken = default);
    Task<LeaveRequest?> GetByIdAsync(int leaveRequestId, bool asTracking = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequestSummaryQueryResult>> GetSummariesByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequestSummaryQueryResult>> GetPendingSummariesAsync(CancellationToken cancellationToken = default);
}
