using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Leave;

namespace HRNexus.Business.Services;

public sealed class LeaveRequestService : ILeaveRequestService
{
    private const string PendingStatusCode = "P";
    private static readonly string[] HrOrAdminRoles = ["Admin", "HRManager", "HRClerk"];

    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IRequestStatusRepository _requestStatusRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public LeaveRequestService(
        IEmployeeRepository employeeRepository,
        ILeaveTypeRepository leaveTypeRepository,
        IRequestStatusRepository requestStatusRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _requestStatusRepository = requestStatusRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LeaveTypeDto>> GetLeaveTypesAsync(CancellationToken cancellationToken = default)
    {
        var leaveTypes = await _leaveTypeRepository.GetActiveAsync(cancellationToken);

        return leaveTypes
            .Select(leaveType => new LeaveTypeDto(
                leaveType.LeaveTypeId,
                leaveType.LeaveTypeName,
                leaveType.LeaveTypeCode,
                leaveType.Description,
                leaveType.DefaultDaysPerYear,
                leaveType.IsPaid,
                leaveType.RequiresApproval,
                leaveType.IsActive))
            .ToList();
    }

    public async Task<IReadOnlyList<RequestStatusDto>> GetRequestStatusesAsync(CancellationToken cancellationToken = default)
    {
        var statuses = await _requestStatusRepository.GetActiveAsync(cancellationToken);

        return statuses
            .Select(status => new RequestStatusDto(
                status.RequestStatusId,
                status.StatusName,
                status.StatusCode,
                status.Description,
                status.IsFinalState,
                status.IsActive))
            .ToList();
    }

    public async Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestRequest request, CancellationToken cancellationToken = default)
    {
        LeaveValidation.EnsureValid(request);
        EnsureCanCreateForEmployee(request.EmployeeId);

        var employee = await GetEmployeeAsync(request.EmployeeId, cancellationToken);
        var leaveType = await GetActiveLeaveTypeAsync(request.LeaveTypeId, cancellationToken);
        var pendingStatus = await GetPendingStatusAsync(cancellationToken);

        await EnsureSufficientBalanceAsync(
            request.EmployeeId,
            request.LeaveTypeId,
            request.StartDate.Year,
            request.RequestedDays,
            cancellationToken);

        var leaveRequest = CreatePendingLeaveRequest(request, pendingStatus.RequestStatusId);

        await _leaveRequestRepository.AddAsync(leaveRequest, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        leaveRequest.Employee = employee;
        leaveRequest.LeaveType = leaveType;
        leaveRequest.RequestStatus = pendingStatus;

        return MapSummary(leaveRequest);
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetEmployeeLeaveRequestsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        _ = await GetEmployeeAsync(employeeId, cancellationToken);
        var requests = await _leaveRequestRepository.GetSummariesByEmployeeAsync(employeeId, cancellationToken);

        return requests.Select(MapSummary).ToList();
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetPendingLeaveRequestsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _leaveRequestRepository.GetPendingSummariesAsync(cancellationToken);
        return requests.Select(MapSummary).ToList();
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestStatusAsync(int leaveRequestId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(leaveRequestId, asTracking: true, cancellationToken)
            ?? throw new EntityNotFoundException($"Leave request {leaveRequestId} was not found.");

        var requestStatus = await _requestStatusRepository.GetByIdAsync(request.RequestStatusId, cancellationToken)
            ?? throw new EntityNotFoundException($"Request status {request.RequestStatusId} was not found.");

        if (!requestStatus.IsActive)
        {
            throw new BusinessRuleException($"Request status {requestStatus.StatusName} is inactive.");
        }

        var reviewerUserId = request.ReviewedByUserId ?? _currentUserContext.UserId;

        if (reviewerUserId.HasValue)
        {
            var reviewer = await _userRepository.GetByIdAsync(reviewerUserId.Value, cancellationToken)
                ?? throw new EntityNotFoundException($"Reviewer user {reviewerUserId.Value} was not found.");

            leaveRequest.ReviewedBy = reviewer.UserId;
            leaveRequest.ReviewedByUser = reviewer;
            leaveRequest.ReviewedAt = DateTime.UtcNow;
        }

        leaveRequest.RequestStatusId = request.RequestStatusId;
        leaveRequest.RequestStatus = requestStatus;
        leaveRequest.ReviewNotes = BusinessValidation.NormalizeOptionalText(request.ReviewNotes);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSummary(leaveRequest);
    }

    private async Task<LeaveType> GetActiveLeaveTypeAsync(int leaveTypeId, CancellationToken cancellationToken)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId, cancellationToken)
            ?? throw new EntityNotFoundException($"Leave type {leaveTypeId} was not found.");

        if (!leaveType.IsActive)
        {
            throw new BusinessRuleException($"Leave type {leaveType.LeaveTypeName} is inactive.");
        }

        return leaveType;
    }

    private async Task<RequestStatus> GetPendingStatusAsync(CancellationToken cancellationToken)
    {
        return await _requestStatusRepository.GetByCodeAsync(PendingStatusCode, cancellationToken)
            ?? throw new BusinessRuleException("Pending leave request status is not configured.");
    }

    private async Task EnsureSufficientBalanceAsync(
        int employeeId,
        int leaveTypeId,
        int balanceYear,
        decimal requestedDays,
        CancellationToken cancellationToken)
    {
        var balance = await _leaveBalanceRepository.GetByEmployeeLeaveTypeYearAsync(
            employeeId,
            leaveTypeId,
            balanceYear,
            cancellationToken: cancellationToken);

        if (balance is not null && balance.RemainingDays < requestedDays)
        {
            throw new BusinessRuleException("Requested leave days exceed the employee's remaining balance.");
        }
    }

    private async Task<Employee> GetEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new EntityNotFoundException($"Employee {employeeId} was not found.");

        if (employee.IsDeleted)
        {
            throw new BusinessRuleException($"Employee {employeeId} is deleted and cannot submit leave requests.");
        }

        return employee;
    }

    private void EnsureCanCreateForEmployee(int employeeId)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            throw new AuthorizationFailedException("Authentication is required to create leave requests.");
        }

        if (IsInAnyRole(HrOrAdminRoles)
            || (_currentUserContext.EmployeeId.HasValue && _currentUserContext.EmployeeId.Value == employeeId))
        {
            return;
        }

        throw new AuthorizationFailedException("You are not allowed to create a leave request for this employee.");
    }

    private bool IsInAnyRole(IEnumerable<string> roleNames)
    {
        return _currentUserContext.Roles.Any(currentRole =>
            roleNames.Any(roleName => string.Equals(roleName, currentRole, StringComparison.OrdinalIgnoreCase)));
    }

    private static LeaveRequest CreatePendingLeaveRequest(CreateLeaveRequestRequest request, int pendingStatusId)
    {
        return new LeaveRequest
        {
            LeaveTypeId = request.LeaveTypeId,
            EmployeeId = request.EmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedDays = request.RequestedDays,
            Reason = BusinessValidation.NormalizeOptionalText(request.Reason),
            RequestStatusId = pendingStatusId,
            RequestedAt = DateTime.UtcNow
        };
    }

    private static LeaveRequestDto MapSummary(LeaveRequest leaveRequest)
    {
        var currentJob = leaveRequest.Employee.JobHistories.FirstOrDefault(job => job.IsCurrent);
        var employeeName = leaveRequest.Employee.Person.FullName;

        return new LeaveRequestDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.EmployeeId,
            leaveRequest.Employee.EmployeeCode,
            employeeName,
            currentJob?.Department.DepartmentName,
            currentJob?.Position.PositionName,
            leaveRequest.LeaveTypeId,
            leaveRequest.LeaveType.LeaveTypeName,
            leaveRequest.LeaveType.LeaveTypeCode,
            leaveRequest.RequestStatusId,
            leaveRequest.RequestStatus.StatusName,
            leaveRequest.RequestStatus.StatusCode,
            leaveRequest.RequestStatus.Description,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.RequestedDays,
            leaveRequest.Reason,
            leaveRequest.RequestedAt,
            leaveRequest.ReviewedBy,
            leaveRequest.ReviewedByUser?.Username,
            leaveRequest.ReviewedAt,
            leaveRequest.ReviewNotes);
    }

    private static LeaveRequestDto MapSummary(LeaveRequestSummaryQueryResult leaveRequest)
    {
        return new LeaveRequestDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.EmployeeId,
            leaveRequest.EmployeeCode,
            leaveRequest.EmployeeName,
            leaveRequest.DepartmentName,
            leaveRequest.PositionName,
            leaveRequest.LeaveTypeId,
            leaveRequest.LeaveTypeName,
            leaveRequest.LeaveTypeCode,
            leaveRequest.RequestStatusId,
            leaveRequest.RequestStatusName,
            leaveRequest.RequestStatusCode,
            leaveRequest.RequestStatusDescription,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.RequestedDays,
            leaveRequest.Reason,
            leaveRequest.RequestedAt,
            leaveRequest.ReviewedByUserId,
            leaveRequest.ReviewedByUsername,
            leaveRequest.ReviewedAt,
            leaveRequest.ReviewNotes);
    }
}
