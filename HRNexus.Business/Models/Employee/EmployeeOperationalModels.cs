using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Models.Core;

namespace HRNexus.Business.Models.Employee;

public sealed record EmployeeSummaryDto(
    int EmployeeId,
    int PersonId,
    string EmployeeCode,
    string FullName,
    DateOnly HireDate,
    int CurrentEmploymentStatusId,
    string EmploymentStatusName,
    bool IsDeleted);

public sealed class CreateEmployeeRequest
{
    [Required]
    public CreatePersonRequest Person { get; set; } = new();

    [Required]
    public CreateEmployeeCoreRequest Employee { get; set; } = new();

    public CreateInitialJobAssignmentRequest? InitialJob { get; set; }
}

public sealed class UpdateEmployeeRequest
{
    [Required]
    public UpdatePersonRequest Person { get; set; } = new();

    [Required]
    public UpdateEmployeeCoreRequest Employee { get; set; } = new();
}

public class CreateEmployeeCoreRequest
{
    [Required]
    public DateOnly HireDate { get; set; }

    [Range(1, int.MaxValue)]
    public int CurrentEmploymentStatusId { get; set; }

    [Range(1, int.MaxValue)]
    public int? TerminationReasonId { get; set; }

    public DateOnly? TerminationDate { get; set; }

    public bool IsEligibleForRehire { get; set; } = true;
}

public sealed class UpdateEmployeeCoreRequest : CreateEmployeeCoreRequest;

public sealed class CreateInitialJobAssignmentRequest
{
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int PositionId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmploymentTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int JobGradeId { get; set; }

    [Range(1, int.MaxValue)]
    public int? ManagerId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }
}

public sealed record EmployeeJobHistoryDto(
    int JobHistoryId,
    int EmployeeId,
    int DepartmentId,
    string DepartmentName,
    int PositionId,
    string PositionName,
    int EmploymentTypeId,
    string EmploymentTypeName,
    int JobGradeId,
    string JobGradeName,
    int EmploymentStatusId,
    string EmploymentStatusName,
    int? ManagerId,
    string? ManagerName,
    bool IsCurrent,
    DateOnly StartDate,
    DateOnly? EndDate);

public class CreateEmployeeJobHistoryRequest
{
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int PositionId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmploymentTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int JobGradeId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmploymentStatusId { get; set; }

    [Range(1, int.MaxValue)]
    public int? ManagerId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }
}

public sealed class UpdateEmployeeJobHistoryRequest : CreateEmployeeJobHistoryRequest;

public sealed record EmployeeFamilyMemberDto(
    int FamilyMemberId,
    int EmployeeId,
    int PersonId,
    string FullName,
    int RelationshipTypeId,
    string RelationshipTypeName,
    PersonDto Person);

public sealed class CreateEmployeeFamilyMemberRequest
{
    [Required]
    public CreatePersonRequest Person { get; set; } = new();

    [Range(1, int.MaxValue)]
    public int RelationshipTypeId { get; set; }
}

public sealed class UpdateEmployeeFamilyMemberRequest
{
    [Required]
    public UpdatePersonRequest Person { get; set; } = new();

    [Range(1, int.MaxValue)]
    public int RelationshipTypeId { get; set; }
}

public sealed record EmployeeDocumentDto(
    int DocumentId,
    int EmployeeId,
    int DocumentTypeId,
    string DocumentTypeName,
    string DocumentName,
    string? ReferenceNumber,
    string FilePath,
    string FileExtension,
    int? FileStorageItemId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    bool IsVerified,
    int? VerifiedBy,
    string? VerifiedByUsername,
    DateTime? VerifiedDate,
    bool IsActive,
    int? UploadedBy,
    string? UploadedByUsername,
    DateTime UploadedDate,
    string? Remarks);

public sealed class UploadEmployeeDocumentRequest
{
    [Range(1, int.MaxValue)]
    public int DocumentTypeId { get; set; }

    [Required]
    [StringLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ReferenceNumber { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }
}

public sealed class UpdateEmployeeDocumentRequest
{
    [Range(1, int.MaxValue)]
    public int DocumentTypeId { get; set; }

    [Required]
    [StringLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ReferenceNumber { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool IsVerified { get; set; }

    [Range(1, int.MaxValue)]
    public int? VerifiedBy { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Remarks { get; set; }
}
