using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Employee;

public sealed class EmployeeJobHistoryRepository : IEmployeeJobHistoryRepository
{
    private readonly HRNexusDbContext _dbContext;

    public EmployeeJobHistoryRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeJobHistoryItemQueryResult>> GetByEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmployeeJobHistories
            .AsNoTracking()
            .Where(job => job.EmployeeId == employeeId)
            .OrderByDescending(job => job.IsCurrent)
            .ThenByDescending(job => job.StartDate)
            .ThenByDescending(job => job.JobHistoryId)
            .Select(job => new EmployeeJobHistoryItemQueryResult(
                job.JobHistoryId,
                job.Department.DepartmentName,
                job.Position.PositionName,
                job.EmploymentType.Name,
                job.EmploymentStatus.Name,
                job.Manager == null ? null : job.Manager.Person.FullName,
                job.IsCurrent,
                job.StartDate,
                job.EndDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeJobHistoryQueryResult>> GetOperationalByEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        return await CreateOperationalQuery()
            .Where(job => job.EmployeeId == employeeId)
            .OrderByDescending(job => job.IsCurrent)
            .ThenByDescending(job => job.StartDate)
            .ThenByDescending(job => job.JobHistoryId)
            .Select(job => new EmployeeJobHistoryQueryResult(
                job.JobHistoryId,
                job.EmployeeId,
                job.DepartmentId,
                job.Department.DepartmentName,
                job.PositionId,
                job.Position.PositionName,
                job.EmploymentTypeId,
                job.EmploymentType.Name,
                job.JobGradeId,
                job.JobGrade.GradeName,
                job.EmploymentStatusId,
                job.EmploymentStatus.Name,
                job.ManagerId,
                job.Manager == null ? null : job.Manager.Person.FullName,
                job.IsCurrent,
                job.StartDate,
                job.EndDate))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeJobHistoryQueryResult?> GetByIdAsync(
        int employeeId,
        int jobHistoryId,
        CancellationToken cancellationToken = default)
    {
        return CreateOperationalQuery()
            .Where(job => job.EmployeeId == employeeId && job.JobHistoryId == jobHistoryId)
            .Select(job => new EmployeeJobHistoryQueryResult(
                job.JobHistoryId,
                job.EmployeeId,
                job.DepartmentId,
                job.Department.DepartmentName,
                job.PositionId,
                job.Position.PositionName,
                job.EmploymentTypeId,
                job.EmploymentType.Name,
                job.JobGradeId,
                job.JobGrade.GradeName,
                job.EmploymentStatusId,
                job.EmploymentStatus.Name,
                job.ManagerId,
                job.Manager == null ? null : job.Manager.Person.FullName,
                job.IsCurrent,
                job.StartDate,
                job.EndDate))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<EmployeeJobHistory?> GetByIdForUpdateAsync(
        int employeeId,
        int jobHistoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeJobHistories
            .FirstOrDefaultAsync(job => job.EmployeeId == employeeId && job.JobHistoryId == jobHistoryId, cancellationToken);
    }

    public Task<EmployeeJobHistory?> GetCurrentForUpdateAsync(
        int employeeId,
        int? exceptJobHistoryId = null,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeJobHistories
            .FirstOrDefaultAsync(job =>
                job.EmployeeId == employeeId
                && job.IsCurrent
                && (!exceptJobHistoryId.HasValue || job.JobHistoryId != exceptJobHistoryId.Value),
                cancellationToken);
    }

    public Task AddAsync(EmployeeJobHistory jobHistory, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeJobHistories.AddAsync(jobHistory, cancellationToken).AsTask();
    }

    public void Remove(EmployeeJobHistory jobHistory)
    {
        _dbContext.EmployeeJobHistories.Remove(jobHistory);
    }

    private IQueryable<EmployeeJobHistory> CreateOperationalQuery()
    {
        return _dbContext.EmployeeJobHistories.AsNoTracking();
    }

}
