using HRNexus.DataAccess.Entities.Leave;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IHolidayRepository
{
    Task<IReadOnlyList<Holiday>> GetActiveAsync(int? year = null, CancellationToken cancellationToken = default);
}
