using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Models.Lookup;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Entities.Security;
using ModuleEntity = HRNexus.DataAccess.Entities.Security.Module;

namespace HRNexus.Business.Services;

public abstract class LookupCrudDefinitionBase<TEntity, TDto, TCreateRequest, TUpdateRequest>
    : ILookupCrudDefinition<TEntity, TDto, TCreateRequest, TUpdateRequest>
{
    public abstract string EntityName { get; }
    public virtual bool UsesSoftDelete => false;
    public abstract int GetId(TEntity entity);
    public abstract string GetSortText(TEntity entity);
    public abstract TDto ToDto(TEntity entity);
    public abstract TEntity CreateEntity(TCreateRequest request);
    public abstract void UpdateEntity(TEntity entity, TUpdateRequest request);
    public virtual void ValidateCreate(TCreateRequest request) { }
    public virtual void ValidateUpdate(int id, TUpdateRequest request) { }

    public virtual void Deactivate(TEntity entity)
    {
        throw new BusinessRuleException($"{EntityName} does not support deactivation.");
    }

    protected static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    protected static string? Optional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    protected static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new BusinessRuleException(message);
        }
    }
}

public sealed class CountryLookupDefinition : LookupCrudDefinitionBase<Country, CountryDto, CreateCountryRequest, UpdateCountryRequest>
{
    public override string EntityName => "Country";
    public override int GetId(Country entity) => entity.CountryId;
    public override string GetSortText(Country entity) => entity.Name;
    public override CountryDto ToDto(Country entity) => new(entity.CountryId, entity.Name, entity.IsoCode);

    public override Country CreateEntity(CreateCountryRequest request)
    {
        return new Country
        {
            Name = Required(request.Name, "Country name"),
            IsoCode = Required(request.IsoCode, "ISO code").ToUpperInvariant()
        };
    }

    public override void UpdateEntity(Country entity, UpdateCountryRequest request)
    {
        entity.Name = Required(request.Name, "Country name");
        entity.IsoCode = Required(request.IsoCode, "ISO code").ToUpperInvariant();
    }
}

public sealed class CityLookupDefinition : LookupCrudDefinitionBase<City, CityDto, CreateCityRequest, UpdateCityRequest>
{
    public override string EntityName => "City";
    public override int GetId(City entity) => entity.CityId;
    public override string GetSortText(City entity) => entity.Name;
    public override CityDto ToDto(City entity) => new(entity.CityId, entity.Name, entity.CountryId);

    public override City CreateEntity(CreateCityRequest request)
    {
        return new City
        {
            Name = Required(request.Name, "City name"),
            CountryId = request.CountryId
        };
    }

    public override void UpdateEntity(City entity, UpdateCityRequest request)
    {
        entity.Name = Required(request.Name, "City name");
        entity.CountryId = request.CountryId;
    }
}

public sealed class GenderLookupDefinition : LookupCrudDefinitionBase<Gender, GenderDto, CreateGenderRequest, UpdateGenderRequest>
{
    public override string EntityName => "Gender";
    public override int GetId(Gender entity) => entity.GenderId;
    public override string GetSortText(Gender entity) => entity.Name;
    public override GenderDto ToDto(Gender entity) => new(entity.GenderId, entity.Name, entity.Code);

    public override Gender CreateEntity(CreateGenderRequest request)
    {
        return new Gender
        {
            Name = Required(request.Name, "Gender name"),
            Code = Required(request.Code, "Gender code").ToUpperInvariant()
        };
    }

    public override void UpdateEntity(Gender entity, UpdateGenderRequest request)
    {
        entity.Name = Required(request.Name, "Gender name");
        entity.Code = Required(request.Code, "Gender code").ToUpperInvariant();
    }
}

public sealed class MaritalStatusLookupDefinition : LookupCrudDefinitionBase<MaritalStatus, MaritalStatusDto, CreateMaritalStatusRequest, UpdateMaritalStatusRequest>
{
    public override string EntityName => "Marital status";
    public override bool UsesSoftDelete => true;
    public override int GetId(MaritalStatus entity) => entity.MaritalStatusId;
    public override string GetSortText(MaritalStatus entity) => entity.Name;
    public override MaritalStatusDto ToDto(MaritalStatus entity) => new(entity.MaritalStatusId, entity.Name, entity.Code, entity.IsActive, entity.Description);

    public override MaritalStatus CreateEntity(CreateMaritalStatusRequest request)
    {
        return new MaritalStatus
        {
            Name = Required(request.Name, "Marital status name"),
            Code = Required(request.Code, "Marital status code").ToUpperInvariant(),
            IsActive = request.IsActive,
            Description = Optional(request.Description)
        };
    }

    public override void UpdateEntity(MaritalStatus entity, UpdateMaritalStatusRequest request)
    {
        entity.Name = Required(request.Name, "Marital status name");
        entity.Code = Required(request.Code, "Marital status code").ToUpperInvariant();
        entity.IsActive = request.IsActive;
        entity.Description = Optional(request.Description);
    }

    public override void Deactivate(MaritalStatus entity) => entity.IsActive = false;
}

public sealed class ContactTypeLookupDefinition : LookupCrudDefinitionBase<ContactType, ContactTypeDto, CreateContactTypeRequest, UpdateContactTypeRequest>
{
    public override string EntityName => "Contact type";
    public override int GetId(ContactType entity) => entity.ContactTypeId;
    public override string GetSortText(ContactType entity) => entity.Name;
    public override ContactTypeDto ToDto(ContactType entity) => new(entity.ContactTypeId, entity.Name);
    public override ContactType CreateEntity(CreateContactTypeRequest request) => new() { Name = Required(request.Name, "Contact type name") };
    public override void UpdateEntity(ContactType entity, UpdateContactTypeRequest request) => entity.Name = Required(request.Name, "Contact type name");
}

public sealed class AddressTypeLookupDefinition : LookupCrudDefinitionBase<AddressType, AddressTypeDto, CreateAddressTypeRequest, UpdateAddressTypeRequest>
{
    public override string EntityName => "Address type";
    public override int GetId(AddressType entity) => entity.AddressTypeId;
    public override string GetSortText(AddressType entity) => entity.Name;
    public override AddressTypeDto ToDto(AddressType entity) => new(entity.AddressTypeId, entity.Name);
    public override AddressType CreateEntity(CreateAddressTypeRequest request) => new() { Name = Required(request.Name, "Address type name") };
    public override void UpdateEntity(AddressType entity, UpdateAddressTypeRequest request) => entity.Name = Required(request.Name, "Address type name");
}

public sealed class IdentifierTypeLookupDefinition : LookupCrudDefinitionBase<IdentifierType, IdentifierTypeDto, CreateIdentifierTypeRequest, UpdateIdentifierTypeRequest>
{
    public override string EntityName => "Identifier type";
    public override int GetId(IdentifierType entity) => entity.IdentifierTypeId;
    public override string GetSortText(IdentifierType entity) => entity.Name;
    public override IdentifierTypeDto ToDto(IdentifierType entity) => new(entity.IdentifierTypeId, entity.Name);
    public override IdentifierType CreateEntity(CreateIdentifierTypeRequest request) => new() { Name = Required(request.Name, "Identifier type name") };
    public override void UpdateEntity(IdentifierType entity, UpdateIdentifierTypeRequest request) => entity.Name = Required(request.Name, "Identifier type name");
}

public sealed class EmploymentTypeLookupDefinition : LookupCrudDefinitionBase<EmploymentType, EmploymentTypeDto, CreateEmploymentTypeRequest, UpdateEmploymentTypeRequest>
{
    public override string EntityName => "Employment type";
    public override int GetId(EmploymentType entity) => entity.EmploymentTypeId;
    public override string GetSortText(EmploymentType entity) => entity.Name;
    public override EmploymentTypeDto ToDto(EmploymentType entity) => new(entity.EmploymentTypeId, entity.Name, entity.EmploymentTypeCode, entity.Description);

    public override EmploymentType CreateEntity(CreateEmploymentTypeRequest request)
    {
        return new EmploymentType
        {
            Name = Required(request.Name, "Employment type name"),
            EmploymentTypeCode = Required(request.EmploymentTypeCode, "Employment type code"),
            Description = Optional(request.Description)
        };
    }

    public override void UpdateEntity(EmploymentType entity, UpdateEmploymentTypeRequest request)
    {
        entity.Name = Required(request.Name, "Employment type name");
        entity.EmploymentTypeCode = Required(request.EmploymentTypeCode, "Employment type code");
        entity.Description = Optional(request.Description);
    }
}

public sealed class EmploymentStatusLookupDefinition : LookupCrudDefinitionBase<EmploymentStatus, EmploymentStatusDto, CreateEmploymentStatusRequest, UpdateEmploymentStatusRequest>
{
    public override string EntityName => "Employment status";
    public override int GetId(EmploymentStatus entity) => entity.EmploymentStatusId;
    public override string GetSortText(EmploymentStatus entity) => entity.Name;
    public override EmploymentStatusDto ToDto(EmploymentStatus entity) => new(entity.EmploymentStatusId, entity.Name, entity.EmploymentStatusCode, entity.Description);

    public override EmploymentStatus CreateEntity(CreateEmploymentStatusRequest request)
    {
        return new EmploymentStatus
        {
            Name = Required(request.Name, "Employment status name"),
            EmploymentStatusCode = Required(request.EmploymentStatusCode, "Employment status code"),
            Description = Optional(request.Description)
        };
    }

    public override void UpdateEntity(EmploymentStatus entity, UpdateEmploymentStatusRequest request)
    {
        entity.Name = Required(request.Name, "Employment status name");
        entity.EmploymentStatusCode = Required(request.EmploymentStatusCode, "Employment status code");
        entity.Description = Optional(request.Description);
    }
}

public sealed class DepartmentLookupDefinition : LookupCrudDefinitionBase<Department, DepartmentDto, CreateDepartmentRequest, UpdateDepartmentRequest>
{
    public override string EntityName => "Department";
    public override bool UsesSoftDelete => true;
    public override int GetId(Department entity) => entity.DepartmentId;
    public override string GetSortText(Department entity) => entity.DepartmentName;
    public override DepartmentDto ToDto(Department entity) => new(entity.DepartmentId, entity.DepartmentName, entity.DepartmentCode, entity.ParentDepartmentId, entity.IsActive, entity.CreatedDate, entity.ModifiedDate);

    public override Department CreateEntity(CreateDepartmentRequest request)
    {
        return new Department
        {
            DepartmentName = Required(request.DepartmentName, "Department name"),
            DepartmentCode = Required(request.DepartmentCode, "Department code"),
            ParentDepartmentId = request.ParentDepartmentId,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public override void ValidateUpdate(int id, UpdateDepartmentRequest request)
    {
        Ensure(request.ParentDepartmentId != id, "A department cannot be its own parent.");
    }

    public override void UpdateEntity(Department entity, UpdateDepartmentRequest request)
    {
        entity.DepartmentName = Required(request.DepartmentName, "Department name");
        entity.DepartmentCode = Required(request.DepartmentCode, "Department code");
        entity.ParentDepartmentId = request.ParentDepartmentId;
        entity.IsActive = request.IsActive;
        entity.ModifiedDate = DateTime.UtcNow;
    }

    public override void Deactivate(Department entity)
    {
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
    }
}

public sealed class PositionLookupDefinition : LookupCrudDefinitionBase<Position, PositionDto, CreatePositionRequest, UpdatePositionRequest>
{
    public override string EntityName => "Position";
    public override bool UsesSoftDelete => true;
    public override int GetId(Position entity) => entity.PositionId;
    public override string GetSortText(Position entity) => entity.PositionName;
    public override PositionDto ToDto(Position entity) => new(entity.PositionId, entity.PositionName, entity.PositionCode, entity.Description, entity.IsActive, entity.CreatedDate, entity.ModifiedDate);

    public override Position CreateEntity(CreatePositionRequest request)
    {
        return new Position
        {
            PositionName = Required(request.PositionName, "Position name"),
            PositionCode = Required(request.PositionCode, "Position code"),
            Description = Optional(request.Description),
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public override void UpdateEntity(Position entity, UpdatePositionRequest request)
    {
        entity.PositionName = Required(request.PositionName, "Position name");
        entity.PositionCode = Required(request.PositionCode, "Position code");
        entity.Description = Optional(request.Description);
        entity.IsActive = request.IsActive;
        entity.ModifiedDate = DateTime.UtcNow;
    }

    public override void Deactivate(Position entity)
    {
        entity.IsActive = false;
        entity.ModifiedDate = DateTime.UtcNow;
    }
}

public sealed class JobGradeLookupDefinition : LookupCrudDefinitionBase<JobGrade, JobGradeDto, CreateJobGradeRequest, UpdateJobGradeRequest>
{
    public override string EntityName => "Job grade";
    public override int GetId(JobGrade entity) => entity.JobGradeId;
    public override string GetSortText(JobGrade entity) => entity.GradeName;
    public override JobGradeDto ToDto(JobGrade entity) => new(entity.JobGradeId, entity.GradeName, entity.MinimumSalary, entity.MaximumSalary);
    public override void ValidateCreate(CreateJobGradeRequest request) => ValidateSalaryRange(request.MinimumSalary, request.MaximumSalary);
    public override void ValidateUpdate(int id, UpdateJobGradeRequest request) => ValidateSalaryRange(request.MinimumSalary, request.MaximumSalary);

    public override JobGrade CreateEntity(CreateJobGradeRequest request)
    {
        return new JobGrade
        {
            GradeName = Required(request.GradeName, "Grade name"),
            MinimumSalary = request.MinimumSalary,
            MaximumSalary = request.MaximumSalary
        };
    }

    public override void UpdateEntity(JobGrade entity, UpdateJobGradeRequest request)
    {
        entity.GradeName = Required(request.GradeName, "Grade name");
        entity.MinimumSalary = request.MinimumSalary;
        entity.MaximumSalary = request.MaximumSalary;
    }

    private static void ValidateSalaryRange(decimal minimumSalary, decimal maximumSalary)
    {
        Ensure(maximumSalary >= minimumSalary, "Maximum salary must be greater than or equal to minimum salary.");
    }
}

public sealed class TerminationReasonLookupDefinition : LookupCrudDefinitionBase<TerminationReason, TerminationReasonDto, CreateTerminationReasonRequest, UpdateTerminationReasonRequest>
{
    public override string EntityName => "Termination reason";
    public override int GetId(TerminationReason entity) => entity.TerminationReasonId;
    public override string GetSortText(TerminationReason entity) => entity.ReasonName;
    public override TerminationReasonDto ToDto(TerminationReason entity) => new(entity.TerminationReasonId, entity.ReasonName, entity.IsEligibleForRehire);

    public override TerminationReason CreateEntity(CreateTerminationReasonRequest request)
    {
        return new TerminationReason
        {
            ReasonName = Required(request.ReasonName, "Reason name"),
            IsEligibleForRehire = request.IsEligibleForRehire
        };
    }

    public override void UpdateEntity(TerminationReason entity, UpdateTerminationReasonRequest request)
    {
        entity.ReasonName = Required(request.ReasonName, "Reason name");
        entity.IsEligibleForRehire = request.IsEligibleForRehire;
    }
}

public sealed class RelationshipTypeLookupDefinition : LookupCrudDefinitionBase<RelationshipType, RelationshipTypeDto, CreateRelationshipTypeRequest, UpdateRelationshipTypeRequest>
{
    public override string EntityName => "Relationship type";
    public override bool UsesSoftDelete => true;
    public override int GetId(RelationshipType entity) => entity.RelationshipTypeId;
    public override string GetSortText(RelationshipType entity) => entity.Name;
    public override RelationshipTypeDto ToDto(RelationshipType entity) => new(entity.RelationshipTypeId, entity.Name, entity.RelationshipCode, entity.MaxEligibleAge, entity.IsEligibleForVisa, entity.IsActive);

    public override RelationshipType CreateEntity(CreateRelationshipTypeRequest request)
    {
        return new RelationshipType
        {
            Name = Required(request.Name, "Relationship type name"),
            RelationshipCode = Required(request.RelationshipCode, "Relationship code"),
            MaxEligibleAge = request.MaxEligibleAge,
            IsEligibleForVisa = request.IsEligibleForVisa,
            IsActive = request.IsActive
        };
    }

    public override void UpdateEntity(RelationshipType entity, UpdateRelationshipTypeRequest request)
    {
        entity.Name = Required(request.Name, "Relationship type name");
        entity.RelationshipCode = Required(request.RelationshipCode, "Relationship code");
        entity.MaxEligibleAge = request.MaxEligibleAge;
        entity.IsEligibleForVisa = request.IsEligibleForVisa;
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(RelationshipType entity) => entity.IsActive = false;
}

public sealed class DocumentTypeLookupDefinition : LookupCrudDefinitionBase<DocumentType, DocumentTypeDto, CreateDocumentTypeRequest, UpdateDocumentTypeRequest>
{
    public override string EntityName => "Document type";
    public override bool UsesSoftDelete => true;
    public override int GetId(DocumentType entity) => entity.DocumentTypeId;
    public override string GetSortText(DocumentType entity) => entity.Name;
    public override DocumentTypeDto ToDto(DocumentType entity) => new(entity.DocumentTypeId, entity.Name, entity.Code, entity.Description, entity.IsActive, entity.IsExpiryTracked, entity.IsMandatory, entity.CreatedDate);

    public override DocumentType CreateEntity(CreateDocumentTypeRequest request)
    {
        return new DocumentType
        {
            Name = Required(request.Name, "Document type name"),
            Code = Required(request.Code, "Document type code"),
            Description = Optional(request.Description),
            IsActive = request.IsActive,
            IsExpiryTracked = request.IsExpiryTracked,
            IsMandatory = request.IsMandatory,
            CreatedDate = DateTime.UtcNow
        };
    }

    public override void UpdateEntity(DocumentType entity, UpdateDocumentTypeRequest request)
    {
        entity.Name = Required(request.Name, "Document type name");
        entity.Code = Required(request.Code, "Document type code");
        entity.Description = Optional(request.Description);
        entity.IsActive = request.IsActive;
        entity.IsExpiryTracked = request.IsExpiryTracked;
        entity.IsMandatory = request.IsMandatory;
    }

    public override void Deactivate(DocumentType entity) => entity.IsActive = false;
}

public sealed class LeaveTypeLookupDefinition : LookupCrudDefinitionBase<LeaveType, LeaveTypeDto, CreateLeaveTypeRequest, UpdateLeaveTypeRequest>
{
    public override string EntityName => "Leave type";
    public override bool UsesSoftDelete => true;
    public override int GetId(LeaveType entity) => entity.LeaveTypeId;
    public override string GetSortText(LeaveType entity) => entity.LeaveTypeName;
    public override LeaveTypeDto ToDto(LeaveType entity) => new(entity.LeaveTypeId, entity.LeaveTypeName, entity.LeaveTypeCode, entity.Description, entity.DefaultDaysPerYear, entity.IsPaid, entity.RequiresApproval, entity.IsActive);

    public override LeaveType CreateEntity(CreateLeaveTypeRequest request)
    {
        return new LeaveType
        {
            LeaveTypeName = Required(request.LeaveTypeName, "Leave type name"),
            LeaveTypeCode = Required(request.LeaveTypeCode, "Leave type code"),
            Description = Optional(request.Description),
            DefaultDaysPerYear = request.DefaultDaysPerYear,
            IsPaid = request.IsPaid,
            RequiresApproval = request.RequiresApproval,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public override void UpdateEntity(LeaveType entity, UpdateLeaveTypeRequest request)
    {
        entity.LeaveTypeName = Required(request.LeaveTypeName, "Leave type name");
        entity.LeaveTypeCode = Required(request.LeaveTypeCode, "Leave type code");
        entity.Description = Optional(request.Description);
        entity.DefaultDaysPerYear = request.DefaultDaysPerYear;
        entity.IsPaid = request.IsPaid;
        entity.RequiresApproval = request.RequiresApproval;
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(LeaveType entity) => entity.IsActive = false;
}

public sealed class HolidayLookupDefinition : LookupCrudDefinitionBase<Holiday, HolidayDto, CreateHolidayRequest, UpdateHolidayRequest>
{
    public override string EntityName => "Holiday";
    public override bool UsesSoftDelete => true;
    public override int GetId(Holiday entity) => entity.HolidayId;
    public override string GetSortText(Holiday entity) => entity.HolidayName;
    public override HolidayDto ToDto(Holiday entity) => new(entity.HolidayId, entity.HolidayName, entity.HolidayDate, entity.Description, entity.IsRecurringAnnual, entity.IsActive);

    public override Holiday CreateEntity(CreateHolidayRequest request)
    {
        return new Holiday
        {
            HolidayName = Required(request.HolidayName, "Holiday name"),
            HolidayDate = request.HolidayDate,
            Description = Optional(request.Description),
            IsRecurringAnnual = request.IsRecurringAnnual,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public override void UpdateEntity(Holiday entity, UpdateHolidayRequest request)
    {
        entity.HolidayName = Required(request.HolidayName, "Holiday name");
        entity.HolidayDate = request.HolidayDate;
        entity.Description = Optional(request.Description);
        entity.IsRecurringAnnual = request.IsRecurringAnnual;
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(Holiday entity) => entity.IsActive = false;
}

public sealed class RequestStatusLookupDefinition : LookupCrudDefinitionBase<RequestStatus, RequestStatusDto, CreateRequestStatusRequest, UpdateRequestStatusRequest>
{
    public override string EntityName => "Request status";
    public override bool UsesSoftDelete => true;
    public override int GetId(RequestStatus entity) => entity.RequestStatusId;
    public override string GetSortText(RequestStatus entity) => entity.StatusName;
    public override RequestStatusDto ToDto(RequestStatus entity) => new(entity.RequestStatusId, entity.StatusName, entity.StatusCode, entity.Description, entity.IsFinalState, entity.IsActive);

    public override RequestStatus CreateEntity(CreateRequestStatusRequest request)
    {
        return new RequestStatus
        {
            StatusName = Required(request.StatusName, "Status name"),
            StatusCode = Required(request.StatusCode, "Status code").ToUpperInvariant(),
            Description = Optional(request.Description),
            IsFinalState = request.IsFinalState,
            IsActive = request.IsActive
        };
    }

    public override void UpdateEntity(RequestStatus entity, UpdateRequestStatusRequest request)
    {
        entity.StatusName = Required(request.StatusName, "Status name");
        entity.StatusCode = Required(request.StatusCode, "Status code").ToUpperInvariant();
        entity.Description = Optional(request.Description);
        entity.IsFinalState = request.IsFinalState;
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(RequestStatus entity) => entity.IsActive = false;
}

public sealed class RoleLookupDefinition : LookupCrudDefinitionBase<Role, RoleDto, CreateRoleRequest, UpdateRoleRequest>
{
    private static readonly string[] BuiltInRoleNames =
    [
        "Admin",
        "HRManager",
        "HRClerk",
        "DepartmentManager",
        "Employee"
    ];

    public override string EntityName => "Role";
    public override bool UsesSoftDelete => true;
    public override int GetId(Role entity) => entity.RoleId;
    public override string GetSortText(Role entity) => entity.RoleName;
    public override RoleDto ToDto(Role entity) => new(entity.RoleId, entity.RoleName, entity.RoleDescription, entity.IsActive, entity.CreatedDate);

    public override Role CreateEntity(CreateRoleRequest request)
    {
        return new Role
        {
            RoleName = Required(request.RoleName, "Role name"),
            RoleDescription = Optional(request.RoleDescription),
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public override void UpdateEntity(Role entity, UpdateRoleRequest request)
    {
        var roleName = Required(request.RoleName, "Role name");

        if (IsBuiltInRole(entity.RoleName))
        {
            Ensure(string.Equals(entity.RoleName, roleName, StringComparison.Ordinal), "Built-in security roles cannot be renamed.");
            Ensure(request.IsActive, "Built-in security roles cannot be deactivated.");
        }

        entity.RoleName = roleName;
        entity.RoleDescription = Optional(request.RoleDescription);
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(Role entity)
    {
        Ensure(!IsBuiltInRole(entity.RoleName), "Built-in security roles cannot be deactivated.");
        entity.IsActive = false;
    }

    private static bool IsBuiltInRole(string roleName)
    {
        return BuiltInRoleNames.Any(builtInRoleName =>
            string.Equals(builtInRoleName, roleName, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class SecurityModuleLookupDefinition : LookupCrudDefinitionBase<ModuleEntity, SecurityModuleDto, CreateSecurityModuleRequest, UpdateSecurityModuleRequest>
{
    public override string EntityName => "Security module";
    public override bool UsesSoftDelete => true;
    public override int GetId(ModuleEntity entity) => entity.ModuleId;
    public override string GetSortText(ModuleEntity entity) => entity.ModuleName;
    public override SecurityModuleDto ToDto(ModuleEntity entity) => new(entity.ModuleId, entity.ModuleName, entity.Description, entity.IsActive);

    public override ModuleEntity CreateEntity(CreateSecurityModuleRequest request)
    {
        return new ModuleEntity
        {
            ModuleName = Required(request.ModuleName, "Module name"),
            Description = Optional(request.Description),
            IsActive = request.IsActive
        };
    }

    public override void UpdateEntity(ModuleEntity entity, UpdateSecurityModuleRequest request)
    {
        entity.ModuleName = Required(request.ModuleName, "Module name");
        entity.Description = Optional(request.Description);
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(ModuleEntity entity) => entity.IsActive = false;
}

public sealed class PermissionLookupDefinition : LookupCrudDefinitionBase<Permission, PermissionDto, CreatePermissionRequest, UpdatePermissionRequest>
{
    public override string EntityName => "Permission";
    public override bool UsesSoftDelete => true;
    public override int GetId(Permission entity) => entity.PermissionId;
    public override string GetSortText(Permission entity) => entity.PermissionName;
    public override PermissionDto ToDto(Permission entity) => new(entity.PermissionId, entity.PermissionName, entity.BitValue, entity.BitOrder, entity.Description, entity.IsActive, entity.IsSystem);
    public override void ValidateCreate(CreatePermissionRequest request) => ValidateBitValue(request.BitValue);
    public override void ValidateUpdate(int id, UpdatePermissionRequest request) => ValidateBitValue(request.BitValue);

    public override Permission CreateEntity(CreatePermissionRequest request)
    {
        return new Permission
        {
            PermissionName = Required(request.PermissionName, "Permission name"),
            BitValue = request.BitValue,
            BitOrder = request.BitOrder,
            Description = Optional(request.Description),
            IsActive = request.IsActive,
            IsSystem = request.IsSystem
        };
    }

    public override void UpdateEntity(Permission entity, UpdatePermissionRequest request)
    {
        entity.PermissionName = Required(request.PermissionName, "Permission name");
        entity.BitValue = request.BitValue;
        entity.BitOrder = request.BitOrder;
        entity.Description = Optional(request.Description);
        entity.IsActive = request.IsActive;
        entity.IsSystem = request.IsSystem;
    }

    public override void Deactivate(Permission entity) => entity.IsActive = false;

    private static void ValidateBitValue(int bitValue)
    {
        Ensure(bitValue > 0 && (bitValue & (bitValue - 1)) == 0, "Permission bit value must be a positive power of two.");
    }
}

public sealed class AccountStatusLookupDefinition : LookupCrudDefinitionBase<AccountStatus, AccountStatusDto, CreateAccountStatusRequest, UpdateAccountStatusRequest>
{
    public override string EntityName => "Account status";
    public override bool UsesSoftDelete => true;
    public override int GetId(AccountStatus entity) => entity.AccountStatusId;
    public override string GetSortText(AccountStatus entity) => entity.StatusName;
    public override AccountStatusDto ToDto(AccountStatus entity) => new(entity.AccountStatusId, entity.StatusName, entity.StatusCode, entity.Description, entity.IsActive);

    public override AccountStatus CreateEntity(CreateAccountStatusRequest request)
    {
        return new AccountStatus
        {
            StatusName = Required(request.StatusName, "Status name"),
            StatusCode = Required(request.StatusCode, "Status code").ToUpperInvariant(),
            Description = Optional(request.Description),
            IsActive = request.IsActive
        };
    }

    public override void UpdateEntity(AccountStatus entity, UpdateAccountStatusRequest request)
    {
        entity.StatusName = Required(request.StatusName, "Status name");
        entity.StatusCode = Required(request.StatusCode, "Status code").ToUpperInvariant();
        entity.Description = Optional(request.Description);
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(AccountStatus entity) => entity.IsActive = false;
}

public sealed class ActivityTypeLookupDefinition : LookupCrudDefinitionBase<ActivityType, ActivityTypeDto, CreateActivityTypeRequest, UpdateActivityTypeRequest>
{
    public override string EntityName => "Activity type";
    public override bool UsesSoftDelete => true;
    public override int GetId(ActivityType entity) => entity.ActivityTypeId;
    public override string GetSortText(ActivityType entity) => entity.ActivityTypeName;
    public override ActivityTypeDto ToDto(ActivityType entity) => new(entity.ActivityTypeId, entity.ActivityTypeName, entity.ActivityTypeCode, entity.Description, entity.IsActive);

    public override ActivityType CreateEntity(CreateActivityTypeRequest request)
    {
        return new ActivityType
        {
            ActivityTypeName = Required(request.ActivityTypeName, "Activity type name"),
            ActivityTypeCode = Required(request.ActivityTypeCode, "Activity type code"),
            Description = Optional(request.Description),
            IsActive = request.IsActive
        };
    }

    public override void UpdateEntity(ActivityType entity, UpdateActivityTypeRequest request)
    {
        entity.ActivityTypeName = Required(request.ActivityTypeName, "Activity type name");
        entity.ActivityTypeCode = Required(request.ActivityTypeCode, "Activity type code");
        entity.Description = Optional(request.Description);
        entity.IsActive = request.IsActive;
    }

    public override void Deactivate(ActivityType entity) => entity.IsActive = false;
}
