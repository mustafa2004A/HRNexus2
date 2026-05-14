using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Leave;

public sealed class HolidayRepository : IHolidayRepository
{
    private readonly HRNexusDbContext _dbContext;

    public HolidayRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Holiday>> GetActiveAsync(int? year = null, CancellationToken cancellationToken = default)
    {

        IQueryable<Holiday> query = _dbContext.Holidays
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (year.HasValue)
        {
            var start = new DateOnly(year.Value, 1, 1);
            var end = new DateOnly(year.Value, 12, 31);
            query = query.Where(x => x.IsRecurringAnnual || (x.HolidayDate >= start && x.HolidayDate <= end));
        }

        return await query
            .OrderBy(x => x.HolidayDate.Month)
            .ThenBy(x => x.HolidayDate.Day)
            .ThenBy(x => x.HolidayName)
            .ToListAsync(cancellationToken);
    }
}
