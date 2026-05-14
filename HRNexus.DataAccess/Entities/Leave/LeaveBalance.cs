using HRNexus.DataAccess.Entities.Employee;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Entities.Leave;

public sealed class LeaveBalance
{
    public int LeaveBalanceId { get; set; }
    public int LeaveTypeId { get; set; }
    public int EmployeeId { get; set; }
    public int BalanceYear { get; set; }
    public decimal EntitledDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public DateTime LastUpdated { get; set; }

    public LeaveType LeaveType { get; set; } = null!;
    public EmployeeEntity Employee { get; set; } = null!;
}
