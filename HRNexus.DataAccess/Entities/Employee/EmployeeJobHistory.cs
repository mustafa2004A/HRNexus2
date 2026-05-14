namespace HRNexus.DataAccess.Entities.Employee;

public sealed class EmployeeJobHistory
{
    public int JobHistoryId { get; set; }
    public int EmployeeId { get; set; }
    public int? ManagerId { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }
    public int EmploymentTypeId { get; set; }
    public int JobGradeId { get; set; }
    public int EmploymentStatusId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public Employee Employee { get; set; } = null!;
    public Employee? Manager { get; set; }
    public Department Department { get; set; } = null!;
    public Position Position { get; set; } = null!;
    public EmploymentType EmploymentType { get; set; } = null!;
    public JobGrade JobGrade { get; set; } = null!;
    public EmploymentStatus EmploymentStatus { get; set; } = null!;
}
