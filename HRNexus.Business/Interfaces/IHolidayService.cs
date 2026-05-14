using HRNexus.Business.Models.Leave;

namespace HRNexus.Business.Interfaces;

public interface IHolidayService
{
    Task<IReadOnlyList<HolidayDto>> GetHolidayListAsync(int? year = null, CancellationToken cancellationToken = default);
}
