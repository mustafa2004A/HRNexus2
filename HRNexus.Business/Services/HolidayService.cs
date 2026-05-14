using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class HolidayService : IHolidayService
{
    private readonly IHolidayRepository _holidayRepository;

    public HolidayService(IHolidayRepository holidayRepository)
    {
        _holidayRepository = holidayRepository;
    }

    public async Task<IReadOnlyList<HolidayDto>> GetHolidayListAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        var holidays = await _holidayRepository.GetActiveAsync(year, cancellationToken);

        return holidays
            .Select(holiday => new HolidayDto(
                holiday.HolidayId,
                holiday.HolidayName,
                ResolveHolidayDate(holiday, year),
                holiday.Description,
                holiday.IsRecurringAnnual,
                holiday.IsActive))
            .OrderBy(holiday => holiday.HolidayDate)
            .ThenBy(holiday => holiday.HolidayName)
            .ToList();
    }

    private static DateOnly ResolveHolidayDate(DataAccess.Entities.Leave.Holiday holiday, int? year)
    {
        if (!year.HasValue || !holiday.IsRecurringAnnual || holiday.HolidayDate.Year == year.Value)
        {
            return holiday.HolidayDate;
        }

        var month = holiday.HolidayDate.Month;
        var day = holiday.HolidayDate.Day;
        var maxDay = DateTime.DaysInMonth(year.Value, month);

        if (day > maxDay)
        {
            return holiday.HolidayDate;
        }

        return new DateOnly(year.Value, month, day);
    }
}
