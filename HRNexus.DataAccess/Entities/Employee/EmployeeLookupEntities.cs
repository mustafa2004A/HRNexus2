namespace HRNexus.DataAccess.Entities.Employee;

public sealed class EmploymentStatus
{
    public int EmploymentStatusId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmploymentStatusCode { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<EmployeeJobHistory> JobHistories { get; set; } = new List<EmployeeJobHistory>();
}

public sealed class EmploymentType
{
    public int EmploymentTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmploymentTypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<EmployeeJobHistory> JobHistories { get; set; } = new List<EmployeeJobHistory>();
}

public sealed class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public int? ParentDepartmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    public ICollection<EmployeeJobHistory> JobHistories { get; set; } = new List<EmployeeJobHistory>();
}

public sealed class Position
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public ICollection<EmployeeJobHistory> JobHistories { get; set; } = new List<EmployeeJobHistory>();
}

public sealed class JobGrade
{
    public int JobGradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public decimal MinimumSalary { get; set; }
    public decimal MaximumSalary { get; set; }

    public ICollection<EmployeeJobHistory> JobHistories { get; set; } = new List<EmployeeJobHistory>();
}

public sealed class TerminationReason
{
    public int TerminationReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public bool IsEligibleForRehire { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public sealed class RelationshipType
{
    public int RelationshipTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RelationshipCode { get; set; } = string.Empty;
    public int? MaxEligibleAge { get; set; }
    public bool IsEligibleForVisa { get; set; }
    public bool IsActive { get; set; }
}

public sealed class DocumentType
{
    public int DocumentTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpiryTracked { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();
}
