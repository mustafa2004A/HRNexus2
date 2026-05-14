using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Leave;

public sealed class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly HRNexusDbContext _dbContext;

    public LeaveBalanceRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LeaveBalance>> ListAsync(
        int? employeeId = null,
        int? leaveTypeId = null,
        int? balanceYear = null,
        CancellationToken cancellationToken = default)
    {
        var query = CreateReadQuery();

        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        if (leaveTypeId.HasValue)
        {
            query = query.Where(x => x.LeaveTypeId == leaveTypeId.Value);
        }

        if (balanceYear.HasValue)
        {
            query = query.Where(x => x.BalanceYear == balanceYear.Value);
        }

        return await query
            .OrderByDescending(x => x.BalanceYear)
            .ThenBy(x => x.Employee.EmployeeCode)
            .ThenBy(x => x.LeaveType.LeaveTypeName)
            .ToListAsync(cancellationToken);
    }

    public Task<LeaveBalance?> GetByIdAsync(int leaveBalanceId, CancellationToken cancellationToken = default)
    {
        return CreateReadQuery()
            .FirstOrDefaultAsync(x => x.LeaveBalanceId == leaveBalanceId, cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveBalance>> GetByEmployeeAsync(int employeeId, int? balanceYear = null, CancellationToken cancellationToken = default)
    {
        IQueryable<LeaveBalance> query = CreateReadQuery()
            .Where(x => x.EmployeeId == employeeId);

        if (balanceYear.HasValue)
        {
            query = query.Where(x => x.BalanceYear == balanceYear.Value);
        }

        return await query
            .OrderBy(x => x.BalanceYear)
            .ThenBy(x => x.LeaveType.LeaveTypeName)
            .ToListAsync(cancellationToken);
    }

    public Task<LeaveBalance?> GetByEmployeeLeaveTypeYearAsync(int employeeId, int leaveTypeId, int balanceYear, bool asTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<LeaveBalance> query = _dbContext.LeaveBalances
            .Where(x => x.EmployeeId == employeeId && x.LeaveTypeId == leaveTypeId && x.BalanceYear == balanceYear);

        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveBalances.AddAsync(leaveBalance, cancellationToken).AsTask();
    }

    private IQueryable<LeaveBalance> CreateReadQuery()
    {
        return _dbContext.LeaveBalances
            .AsNoTracking()
            .Include(x => x.LeaveType)
            .Include(x => x.Employee)
            .ThenInclude(x => x.Person);
    }
}
