using System.Data;
using System.Globalization;
using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Repositories.Employee;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly HRNexusDbContext _dbContext;

    public EmployeeRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EmployeeEntity?> GetByIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .Include(x => x.Person)
            .Include(x => x.CurrentEmploymentStatus)
            .Include(x => x.JobHistories.Where(job => job.IsCurrent))
                .ThenInclude(job => job.Department)
            .Include(x => x.JobHistories.Where(job => job.IsCurrent))
                .ThenInclude(job => job.Position)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);
    }

    public Task<bool> ExistsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return CreateReadableEmployeeQuery()
            .AnyAsync(employee => employee.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeSummaryQueryResult>> ListAsync(
        string? search,
        bool includeDeleted,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Employees.AsNoTracking();

        if (!includeDeleted)
        {
            query = query.Where(employee => !employee.IsDeleted && !employee.Person.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(employee =>
                employee.EmployeeCode.Contains(trimmedSearch)
                || employee.Person.FullName.Contains(trimmedSearch)
                || employee.Person.FirstName.Contains(trimmedSearch)
                || employee.Person.LastName.Contains(trimmedSearch));
        }

        return await query
            .OrderBy(employee => employee.Person.LastName)
            .ThenBy(employee => employee.Person.FirstName)
            .ThenBy(employee => employee.EmployeeCode)
            .Select(employee => new EmployeeSummaryQueryResult(
                employee.EmployeeId,
                employee.PersonId,
                employee.EmployeeCode,
                employee.Person.FullName,
                employee.HireDate,
                employee.CurrentEmploymentStatusId,
                employee.CurrentEmploymentStatus.Name,
                employee.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeDetailsQueryResult?> GetDetailsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return CreateReadableEmployeeQuery()
            .Where(employee => employee.EmployeeId == employeeId)
            .Select(employee => new EmployeeDetailsQueryResult(
                employee.EmployeeId,
                employee.EmployeeCode,
                employee.PersonId,
                employee.Person.FullName,
                employee.HireDate,
                employee.CurrentEmploymentStatus.Name,
                employee.TerminationDate,
                employee.IsEligibleForRehire,
                employee.Person.PhotoUrl,
                employee.Person.PhotoFileStorageItemId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<EmployeeCurrentContextQueryResult?> GetCurrentContextAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return CreateReadableEmployeeQuery()
            .Where(employee => employee.EmployeeId == employeeId)
            .Select(employee => new EmployeeCurrentContextQueryResult(
                employee.EmployeeId,
                employee.EmployeeCode,
                employee.Person.FullName,
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Department.DepartmentName)
                    .FirstOrDefault(),
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Position.PositionName)
                    .FirstOrDefault(),
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.EmploymentType.Name)
                    .FirstOrDefault(),
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.EmploymentStatus.Name)
                    .FirstOrDefault() ?? employee.CurrentEmploymentStatus.Name,
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.ManagerId)
                    .FirstOrDefault(),
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => job.Manager == null ? null : job.Manager.Person.FullName)
                    .FirstOrDefault(),
                employee.JobHistories
                    .Where(job => job.IsCurrent)
                    .Select(job => (DateOnly?)job.StartDate)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<EmployeeEntity?> GetByIdForUpdateAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .Include(employee => employee.Person)
            .FirstOrDefaultAsync(employee => employee.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<int> GetNextEmployeeCodeNumberAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State == ConnectionState.Closed;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR emp.EmployeeCodeSequence;";

            var currentTransaction = _dbContext.Database.CurrentTransaction;
            if (currentTransaction is not null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            var value = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    public Task AddAsync(EmployeeEntity employee, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees.AddAsync(employee, cancellationToken).AsTask();
    }

    private IQueryable<EmployeeEntity> CreateReadableEmployeeQuery()
    {
        return _dbContext.Employees
            .AsNoTracking()
            .Where(employee => !employee.IsDeleted && !employee.Person.IsDeleted);
    }
}
