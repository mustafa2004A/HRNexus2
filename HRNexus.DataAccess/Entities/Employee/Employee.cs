using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Entities.Security;

namespace HRNexus.DataAccess.Entities.Employee;

public sealed class Employee
{
    public int EmployeeId { get; set; }
    public int PersonId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public int CurrentEmploymentStatusId { get; set; }
    public int? TerminationReasonId { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public bool IsEligibleForRehire { get; set; }
    public bool IsDeleted { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Person Person { get; set; } = null!;
    public EmploymentStatus CurrentEmploymentStatus { get; set; } = null!;
    public TerminationReason? TerminationReason { get; set; }
    public User? User { get; set; }
    public ICollection<EmployeeJobHistory> JobHistories { get; set; } = new List<EmployeeJobHistory>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeFamilyMember> FamilyMembers { get; set; } = new List<EmployeeFamilyMember>();
}

public sealed class EmployeeFamilyMember
{
    public int FamilyMemberId { get; set; }
    public int EmployeeId { get; set; }
    public int PersonId { get; set; }
    public int RelationshipTypeId { get; set; }

    public Employee Employee { get; set; } = null!;
    public Person Person { get; set; } = null!;
    public RelationshipType RelationshipType { get; set; } = null!;
}
