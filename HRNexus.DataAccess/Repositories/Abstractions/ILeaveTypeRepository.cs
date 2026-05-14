using HRNexus.DataAccess.Entities.Leave;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface ILeaveTypeRepository
{
    Task<IReadOnlyList<LeaveType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<LeaveType?> GetByIdAsync(int leaveTypeId, CancellationToken cancellationToken = default);
}
