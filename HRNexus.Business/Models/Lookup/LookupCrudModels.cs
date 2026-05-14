using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Lookup;

public sealed record LookupCrudResult<TDto>(int Id, TDto Item);

public sealed record CountryDto(int CountryId, string Name, string IsoCode);

public class CreateCountryRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string IsoCode { get; set; } = string.Empty;
}

public sealed class UpdateCountryRequest : CreateCountryRequest;

public sealed record CityDto(int CityId, string Name, int CountryId);

public class CreateCityRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CountryId { get; set; }
}

public sealed class UpdateCityRequest : CreateCityRequest;

public sealed record GenderDto(int GenderId, string Name, string Code);

public class CreateGenderRequest
{
    [Required]
    [StringLength(10)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;
}

public sealed class UpdateGenderRequest : CreateGenderRequest;

public sealed record MaritalStatusDto(int MaritalStatusId, string Name, string Code, bool IsActive, string? Description);

public class CreateMaritalStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [StringLength(255)]
    public string? Description { get; set; }
}

public sealed class UpdateMaritalStatusRequest : CreateMaritalStatusRequest;

public sealed record ContactTypeDto(int ContactTypeId, string Name);

public class CreateContactTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateContactTypeRequest : CreateContactTypeRequest;

public sealed record AddressTypeDto(int AddressTypeId, string Name);

public class CreateAddressTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateAddressTypeRequest : CreateAddressTypeRequest;

public sealed record IdentifierTypeDto(int IdentifierTypeId, string Name);

public class CreateIdentifierTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateIdentifierTypeRequest : CreateIdentifierTypeRequest;

public sealed record EmploymentTypeDto(int EmploymentTypeId, string Name, string EmploymentTypeCode, string? Description);

public class CreateEmploymentTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string EmploymentTypeCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }
}

public sealed class UpdateEmploymentTypeRequest : CreateEmploymentTypeRequest;

public sealed record EmploymentStatusDto(int EmploymentStatusId, string Name, string EmploymentStatusCode, string? Description);

public class CreateEmploymentStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string EmploymentStatusCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }
}

public sealed class UpdateEmploymentStatusRequest : CreateEmploymentStatusRequest;

public sealed record DepartmentDto(
    int DepartmentId,
    string DepartmentName,
    string DepartmentCode,
    int? ParentDepartmentId,
    bool IsActive,
    DateTime CreatedDate,
    DateTime? ModifiedDate);

public class CreateDepartmentRequest
{
    [Required]
    [StringLength(50)]
    public string DepartmentName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? ParentDepartmentId { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDepartmentRequest : CreateDepartmentRequest;

public sealed record PositionDto(
    int PositionId,
    string PositionName,
    string PositionCode,
    string? Description,
    bool IsActive,
    DateTime CreatedDate,
    DateTime? ModifiedDate);

public class CreatePositionRequest
{
    [Required]
    [StringLength(50)]
    public string PositionName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PositionCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdatePositionRequest : CreatePositionRequest;

public sealed record JobGradeDto(int JobGradeId, string GradeName, decimal MinimumSalary, decimal MaximumSalary);

public class CreateJobGradeRequest
{
    [Required]
    [StringLength(50)]
    public string GradeName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999.99")]
    public decimal MinimumSalary { get; set; }

    [Range(typeof(decimal), "0", "999999999999.99")]
    public decimal MaximumSalary { get; set; }
}

public sealed class UpdateJobGradeRequest : CreateJobGradeRequest;

public sealed record TerminationReasonDto(int TerminationReasonId, string ReasonName, bool IsEligibleForRehire);

public class CreateTerminationReasonRequest
{
    [Required]
    [StringLength(255)]
    public string ReasonName { get; set; } = string.Empty;

    public bool IsEligibleForRehire { get; set; }
}

public sealed class UpdateTerminationReasonRequest : CreateTerminationReasonRequest;

public sealed record RelationshipTypeDto(
    int RelationshipTypeId,
    string Name,
    string RelationshipCode,
    int? MaxEligibleAge,
    bool IsEligibleForVisa,
    bool IsActive);

public class CreateRelationshipTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string RelationshipCode { get; set; } = string.Empty;

    [Range(0, 150)]
    public int? MaxEligibleAge { get; set; }

    public bool IsEligibleForVisa { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRelationshipTypeRequest : CreateRelationshipTypeRequest;

public sealed record DocumentTypeDto(
    int DocumentTypeId,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    bool IsExpiryTracked,
    bool IsMandatory,
    DateTime CreatedDate);

public class CreateDocumentTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsExpiryTracked { get; set; }
    public bool IsMandatory { get; set; }
}

public sealed class UpdateDocumentTypeRequest : CreateDocumentTypeRequest;

public class CreateLeaveTypeRequest
{
    [Required]
    [StringLength(50)]
    public string LeaveTypeName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string LeaveTypeCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "999.99")]
    public decimal DefaultDaysPerYear { get; set; }

    public bool IsPaid { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateLeaveTypeRequest : CreateLeaveTypeRequest;

public class CreateHolidayRequest
{
    [Required]
    [StringLength(100)]
    public string HolidayName { get; set; } = string.Empty;

    [Required]
    public DateOnly HolidayDate { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsRecurringAnnual { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateHolidayRequest : CreateHolidayRequest;

public class CreateRequestStatusRequest
{
    [Required]
    [StringLength(20)]
    public string StatusName { get; set; } = string.Empty;

    [Required]
    [StringLength(1, MinimumLength = 1)]
    public string StatusCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsFinalState { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRequestStatusRequest : CreateRequestStatusRequest;

public sealed record RoleDto(int RoleId, string RoleName, string? RoleDescription, bool IsActive, DateTime CreatedDate);

public class CreateRoleRequest
{
    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(255)]
    public string? RoleDescription { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRoleRequest : CreateRoleRequest;

public sealed record SecurityModuleDto(int ModuleId, string ModuleName, string? Description, bool IsActive);

public class CreateSecurityModuleRequest
{
    [Required]
    [StringLength(50)]
    public string ModuleName { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateSecurityModuleRequest : CreateSecurityModuleRequest;

public sealed record PermissionDto(
    int PermissionId,
    string PermissionName,
    int BitValue,
    int BitOrder,
    string? Description,
    bool IsActive,
    bool IsSystem);

public class CreatePermissionRequest
{
    [Required]
    [StringLength(50)]
    public string PermissionName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int BitValue { get; set; }

    [Range(0, 30)]
    public int BitOrder { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } = true;
}

public sealed class UpdatePermissionRequest : CreatePermissionRequest;

public sealed record AccountStatusDto(
    int AccountStatusId,
    string StatusName,
    string StatusCode,
    string? Description,
    bool IsActive);

public class CreateAccountStatusRequest
{
    [Required]
    [StringLength(20)]
    public string StatusName { get; set; } = string.Empty;

    [Required]
    [StringLength(1, MinimumLength = 1)]
    public string StatusCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateAccountStatusRequest : CreateAccountStatusRequest;

public sealed record ActivityTypeDto(
    int ActivityTypeId,
    string ActivityTypeName,
    string ActivityTypeCode,
    string? Description,
    bool IsActive);

public class CreateActivityTypeRequest
{
    [Required]
    [StringLength(50)]
    public string ActivityTypeName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ActivityTypeCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateActivityTypeRequest : CreateActivityTypeRequest;
