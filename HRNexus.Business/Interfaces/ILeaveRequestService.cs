using HRNexus.Business.Models.Leave;

namespace HRNexus.Business.Interfaces;

public interface ILeaveRequestService
{
    Task<IReadOnlyList<LeaveTypeDto>> GetLeaveTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RequestStatusDto>> GetRequestStatusesAsync(CancellationToken cancellationToken = default);
    Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequestDto>> GetEmployeeLeaveRequestsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveRequestDto>> GetPendingLeaveRequestsAsync(CancellationToken cancellationToken = default);
    Task<LeaveRequestDto> UpdateLeaveRequestStatusAsync(int leaveRequestId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default);
}
