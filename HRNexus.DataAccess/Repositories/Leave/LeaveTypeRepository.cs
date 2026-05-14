using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Leave;

public sealed class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly HRNexusDbContext _dbContext;

    public LeaveTypeRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LeaveType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaveTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.LeaveTypeName)
            .ToListAsync(cancellationToken);
    }

    public Task<LeaveType?> GetByIdAsync(int leaveTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LeaveTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LeaveTypeId == leaveTypeId, cancellationToken);
    }
}
