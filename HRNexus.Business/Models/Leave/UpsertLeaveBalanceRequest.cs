using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Leave;

public sealed class UpsertLeaveBalanceRequest
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [Range(2000, 2100)]
    public int BalanceYear { get; set; }

    [Range(typeof(decimal), "0", "999.99")]
    public decimal EntitledDays { get; set; }

    [Range(typeof(decimal), "0", "999.99")]
    public decimal UsedDays { get; set; }
}
