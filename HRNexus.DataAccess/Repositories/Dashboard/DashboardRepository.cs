using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Repositories.Dashboard;

public sealed class DashboardRepository : IDashboardRepository
{
    private const string ActiveEmploymentStatusCode = "ACTIVE";
    private const string PendingRequestStatusCode = "P";

    private readonly HRNexusDbContext _dbContext;

    public DashboardRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryQueryResult> GetSummaryAsync(
        DateOnly currentDate,
        int expiringWithinDays,
        int latestLeaveRequestCount,
        int recentHireCount,
        int expiringDocumentCount,
        CancellationToken cancellationToken = default)
    {
        var expiryCutoffDate = currentDate.AddDays(expiringWithinDays);
        var employeeQuery = CreateCurrentEmployeesQuery();
        var expiringDocumentQuery = CreateExpiringDocumentsQuery(currentDate, expiryCutoffDate);

        var totalEmployees = await employeeQuery.CountAsync(cancellationToken);
        var activeEmployees = await employeeQuery
            .CountAsync(employee => employee.CurrentEmploymentStatus.EmploymentStatusCode == ActiveEmploymentStatusCode, cancellationToken);
        var pendingLeaveRequests = await _dbContext.LeaveRequests
            .AsNoTracking()
            .CountAsync(request => request.RequestStatus.StatusCode == PendingRequestStatusCode, cancellationToken);
        var expiringDocumentsCount = await expiringDocumentQuery.CountAsync(cancellationToken);

        var latestLeaveRequests = await GetLatestLeaveRequestsAsync(latestLeaveRequestCount, cancellationToken);
        var recentHires = await GetRecentHiresAsync(recentHireCount, cancellationToken);
        var employeesByDepartment = await GetEmployeesByDepartmentAsync(cancellationToken);
        var expiringDocuments = await GetExpiringDocumentsAsync(expiringDocumentQuery, expiringDocumentCount, cancellationToken);

        return new DashboardSummaryQueryResult(
            new DashboardKpisQueryResult(
                totalEmployees,
                activeEmployees,
                pendingLeaveRequests,
                expiringDocumentsCount),
            latestLeaveRequests,
            recentHires,
            employeesByDepartment,
            expiringDocuments);
    }

    private IQueryable<EmployeeEntity> CreateCurrentEmployeesQuery()
    {
        return _dbContext.Employees
            .AsNoTracking()
            .Where(employee => !employee.IsDeleted && !employee.Person.IsDeleted);
    }

    private IQueryable<EmployeeDocument> CreateExpiringDocumentsQuery(DateOnly currentDate, DateOnly expiryCutoffDate)
    {
        return _dbContext.EmployeeDocuments
            .AsNoTracking()
            .Where(document =>
                document.IsActive
                && document.DocumentType.IsActive
                && document.DocumentType.IsExpiryTracked
                && !document.Employee.IsDeleted
                && !document.Employee.Person.IsDeleted
                && document.ExpiryDate.HasValue
                && document.ExpiryDate.Value >= currentDate
                && document.ExpiryDate.Value <= expiryCutoffDate);
    }

    private async Task<IReadOnlyList<DashboardLeaveRequestItemQueryResult>> GetLatestLeaveRequestsAsync(
        int take,
        CancellationToken cancellationToken)
    {
        return await _dbContext.LeaveRequests
            .AsNoTracking()
            .OrderByDescending(request => request.RequestedAt)
            .ThenByDescending(request => request.LeaveRequestId)
            .Take(take)
            .Select(request => new DashboardLeaveRequestItemQueryResult(
                request.LeaveRequestId,
                request.EmployeeId,
                request.Employee.EmployeeCode,
                request.Employee.Person.FullName,
                request.LeaveType.LeaveTypeName,
                request.RequestStatus.StatusName,
                request.RequestedDays,
                request.RequestedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<DashboardRecentHireItemQueryResult>> GetRecentHiresAsync(
        int take,
        CancellationToken cancellationToken)
    {
        return await CreateCurrentEmployeesQuery()
            .OrderByDescending(employee => employee.HireDate)
            .ThenByDescending(employee => employee.EmployeeId)
            .Take(take)
            .Select(employee => new DashboardRecentHireItemQueryResult(
                employee.EmployeeId,
                employee.EmployeeCode,
                employee.Person.FullName,
                employee.HireDate,
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Department.DepartmentName)
                    .FirstOrDefault(),
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Position.PositionName)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<DashboardDepartmentCountQueryResult>> GetEmployeesByDepartmentAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EmployeeJobHistories
            .AsNoTracking()
            .Where(job => job.IsCurrent && !job.Employee.IsDeleted && !job.Employee.Person.IsDeleted)
            .GroupBy(job => new
            {
                job.DepartmentId,
                job.Department.DepartmentName
            })
            .Select(group => new
            {
                group.Key.DepartmentId,
                group.Key.DepartmentName,
                EmployeeCount = group.Count()
            })
            .OrderByDescending(department => department.EmployeeCount)
            .ThenBy(department => department.DepartmentName)
            .Select(department => new DashboardDepartmentCountQueryResult(
                department.DepartmentId,
                department.DepartmentName,
                department.EmployeeCount))
            .ToListAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<DashboardExpiringDocumentItemQueryResult>> GetExpiringDocumentsAsync(
        IQueryable<EmployeeDocument> expiringDocumentQuery,
        int take,
        CancellationToken cancellationToken)
    {
        return await expiringDocumentQuery
            .OrderBy(document => document.ExpiryDate)
            .ThenBy(document => document.Employee.EmployeeCode)
            .ThenBy(document => document.DocumentName)
            .Take(take)
            .Select(document => new DashboardExpiringDocumentItemQueryResult(
                document.DocumentId,
                document.EmployeeId,
                document.Employee.EmployeeCode,
                document.Employee.Person.FullName,
                document.DocumentName,
                document.DocumentType.Name,
                document.ExpiryDate!.Value))
            .ToListAsync(cancellationToken);
    }
}
