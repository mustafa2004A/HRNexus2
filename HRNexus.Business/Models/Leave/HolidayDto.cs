namespace HRNexus.Business.Models.Leave;

public sealed record HolidayDto(
    int HolidayId,
    string HolidayName,
    DateOnly HolidayDate,
    string? Description,
    bool IsRecurringAnnual,
    bool IsActive);
