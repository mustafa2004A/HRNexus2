using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Leave;

public sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly HRNexusDbContext _dbContext;

    public LeaveRequestRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveRequests.AddAsync(leaveRequest, cancellationToken).AsTask();
    }

    public Task<bool> ExistsAsync(int leaveRequestId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveRequests
            .AsNoTracking()
            .AnyAsync(x => x.LeaveRequestId == leaveRequestId, cancellationToken);
    }

    public Task<int?> GetEmployeeIdAsync(int leaveRequestId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(x => x.LeaveRequestId == leaveRequestId)
            .Select(x => (int?)x.EmployeeId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<LeaveRequest?> GetByIdAsync(int leaveRequestId, bool asTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<LeaveRequest> query = CreateDetailQuery()
            .Where(x => x.LeaveRequestId == leaveRequestId);

        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await CreateDetailQuery()
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await CreateDetailQuery()
            .AsNoTracking()
            .Where(x => x.RequestStatus.StatusCode == "P")
            .OrderBy(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveRequestSummaryQueryResult>> GetSummariesByEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.RequestedAt);

        return await ProjectSummary(query).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveRequestSummaryQueryResult>> GetPendingSummariesAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(x => x.RequestStatus.StatusCode == "P")
            .OrderBy(x => x.RequestedAt);

        return await ProjectSummary(query).ToListAsync(cancellationToken);
    }

    private IQueryable<LeaveRequest> CreateDetailQuery()
    {
        return _dbContext.LeaveRequests
            .Include(x => x.LeaveType)
            .Include(x => x.RequestStatus)
            .Include(x => x.ReviewedByUser)
            .Include(x => x.Employee)
                .ThenInclude(x => x.Person)
            .Include(x => x.Employee)
                .ThenInclude(x => x.JobHistories.Where(job => job.IsCurrent))
                    .ThenInclude(job => job.Department)
            .Include(x => x.Employee)
                .ThenInclude(x => x.JobHistories.Where(job => job.IsCurrent))
                    .ThenInclude(job => job.Position)
            .AsSplitQuery();
    }

    private static IQueryable<LeaveRequestSummaryQueryResult> ProjectSummary(IQueryable<LeaveRequest> query)
    {
        return query.Select(x => new LeaveRequestSummaryQueryResult(
                x.LeaveRequestId,
                x.EmployeeId,
                x.Employee.EmployeeCode,
                x.Employee.Person.FullName,
                x.Employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Department.DepartmentName)
                    .FirstOrDefault(),
                x.Employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Position.PositionName)
                    .FirstOrDefault(),
                x.LeaveTypeId,
                x.LeaveType.LeaveTypeName,
                x.LeaveType.LeaveTypeCode,
                x.RequestStatusId,
                x.RequestStatus.StatusName,
                x.RequestStatus.StatusCode,
                x.RequestStatus.Description,
                x.StartDate,
                x.EndDate,
                x.RequestedDays,
                x.Reason,
                x.RequestedAt,
                x.ReviewedBy,
                x.ReviewedByUser == null ? null : x.ReviewedByUser.Username,
                x.ReviewedAt,
                x.ReviewNotes));
    }
}
