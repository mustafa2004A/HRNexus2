using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Leave;

public sealed class CreateLeaveRequestRequest
{
    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Range(typeof(decimal), "0.01", "999.99")]
    public decimal RequestedDays { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
