namespace HRNexus.DataAccess.Entities.Leave;

public sealed class Holiday
{
    public int HolidayId { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool IsRecurringAnnual { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
