using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Models.Lookup;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

public abstract class HrLookupCrudController<TDto, TCreateRequest, TUpdateRequest> : ControllerBase
{
    private readonly ILookupCrudService<TDto, TCreateRequest, TUpdateRequest> _service;

    protected HrLookupCrudController(ILookupCrudService<TDto, TCreateRequest, TUpdateRequest> service)
    {
        _service = service;
    }

    [Authorize(Policy = AuthorizationPolicyNames.AuthenticatedUser)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<TDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.AuthenticatedUser)]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> GetById([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> Create([FromBody] TCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result.Item);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] TUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> Delete([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}

public abstract class SecurityLookupCrudController<TDto, TCreateRequest, TUpdateRequest> : ControllerBase
{
    private readonly ILookupCrudService<TDto, TCreateRequest, TUpdateRequest> _service;

    protected SecurityLookupCrudController(ILookupCrudService<TDto, TCreateRequest, TUpdateRequest> service)
    {
        _service = service;
    }

    [Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<TDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> GetById([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> Create([FromBody] TCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result.Item);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] TUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TDto>> Delete([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}

[ApiController]
[Produces("application/json")]
[Route("api/countries")]
public sealed class CountriesController : HrLookupCrudController<CountryDto, CreateCountryRequest, UpdateCountryRequest>
{
    public CountriesController(ILookupCrudService<CountryDto, CreateCountryRequest, UpdateCountryRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/cities")]
public sealed class CitiesController : HrLookupCrudController<CityDto, CreateCityRequest, UpdateCityRequest>
{
    public CitiesController(ILookupCrudService<CityDto, CreateCityRequest, UpdateCityRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/genders")]
public sealed class GendersController : HrLookupCrudController<GenderDto, CreateGenderRequest, UpdateGenderRequest>
{
    public GendersController(ILookupCrudService<GenderDto, CreateGenderRequest, UpdateGenderRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/marital-statuses")]
public sealed class MaritalStatusesController : HrLookupCrudController<MaritalStatusDto, CreateMaritalStatusRequest, UpdateMaritalStatusRequest>
{
    public MaritalStatusesController(ILookupCrudService<MaritalStatusDto, CreateMaritalStatusRequest, UpdateMaritalStatusRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/contact-types")]
public sealed class ContactTypesController : HrLookupCrudController<ContactTypeDto, CreateContactTypeRequest, UpdateContactTypeRequest>
{
    public ContactTypesController(ILookupCrudService<ContactTypeDto, CreateContactTypeRequest, UpdateContactTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/address-types")]
public sealed class AddressTypesController : HrLookupCrudController<AddressTypeDto, CreateAddressTypeRequest, UpdateAddressTypeRequest>
{
    public AddressTypesController(ILookupCrudService<AddressTypeDto, CreateAddressTypeRequest, UpdateAddressTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/identifier-types")]
public sealed class IdentifierTypesController : HrLookupCrudController<IdentifierTypeDto, CreateIdentifierTypeRequest, UpdateIdentifierTypeRequest>
{
    public IdentifierTypesController(ILookupCrudService<IdentifierTypeDto, CreateIdentifierTypeRequest, UpdateIdentifierTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/employment-types")]
public sealed class EmploymentTypesController : HrLookupCrudController<EmploymentTypeDto, CreateEmploymentTypeRequest, UpdateEmploymentTypeRequest>
{
    public EmploymentTypesController(ILookupCrudService<EmploymentTypeDto, CreateEmploymentTypeRequest, UpdateEmploymentTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/employment-statuses")]
public sealed class EmploymentStatusesController : HrLookupCrudController<EmploymentStatusDto, CreateEmploymentStatusRequest, UpdateEmploymentStatusRequest>
{
    public EmploymentStatusesController(ILookupCrudService<EmploymentStatusDto, CreateEmploymentStatusRequest, UpdateEmploymentStatusRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/departments")]
public sealed class DepartmentsController : HrLookupCrudController<DepartmentDto, CreateDepartmentRequest, UpdateDepartmentRequest>
{
    public DepartmentsController(ILookupCrudService<DepartmentDto, CreateDepartmentRequest, UpdateDepartmentRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/positions")]
public sealed class PositionsController : HrLookupCrudController<PositionDto, CreatePositionRequest, UpdatePositionRequest>
{
    public PositionsController(ILookupCrudService<PositionDto, CreatePositionRequest, UpdatePositionRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/job-grades")]
public sealed class JobGradesController : HrLookupCrudController<JobGradeDto, CreateJobGradeRequest, UpdateJobGradeRequest>
{
    public JobGradesController(ILookupCrudService<JobGradeDto, CreateJobGradeRequest, UpdateJobGradeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/termination-reasons")]
public sealed class TerminationReasonsController : HrLookupCrudController<TerminationReasonDto, CreateTerminationReasonRequest, UpdateTerminationReasonRequest>
{
    public TerminationReasonsController(ILookupCrudService<TerminationReasonDto, CreateTerminationReasonRequest, UpdateTerminationReasonRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/relationship-types")]
public sealed class RelationshipTypesController : HrLookupCrudController<RelationshipTypeDto, CreateRelationshipTypeRequest, UpdateRelationshipTypeRequest>
{
    public RelationshipTypesController(ILookupCrudService<RelationshipTypeDto, CreateRelationshipTypeRequest, UpdateRelationshipTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/document-types")]
public sealed class DocumentTypesController : HrLookupCrudController<DocumentTypeDto, CreateDocumentTypeRequest, UpdateDocumentTypeRequest>
{
    public DocumentTypesController(ILookupCrudService<DocumentTypeDto, CreateDocumentTypeRequest, UpdateDocumentTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/leave-types")]
public sealed class LeaveTypesController : HrLookupCrudController<LeaveTypeDto, CreateLeaveTypeRequest, UpdateLeaveTypeRequest>
{
    public LeaveTypesController(ILookupCrudService<LeaveTypeDto, CreateLeaveTypeRequest, UpdateLeaveTypeRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/holidays")]
public sealed class HolidayCrudController : HrLookupCrudController<HolidayDto, CreateHolidayRequest, UpdateHolidayRequest>
{
    public HolidayCrudController(ILookupCrudService<HolidayDto, CreateHolidayRequest, UpdateHolidayRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/request-statuses")]
public sealed class RequestStatusesController : HrLookupCrudController<RequestStatusDto, CreateRequestStatusRequest, UpdateRequestStatusRequest>
{
    public RequestStatusesController(ILookupCrudService<RequestStatusDto, CreateRequestStatusRequest, UpdateRequestStatusRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/security/roles")]
public sealed class SecurityRolesController : SecurityLookupCrudController<RoleDto, CreateRoleRequest, UpdateRoleRequest>
{
    public SecurityRolesController(ILookupCrudService<RoleDto, CreateRoleRequest, UpdateRoleRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/security/modules")]
public sealed class SecurityModulesController : SecurityLookupCrudController<SecurityModuleDto, CreateSecurityModuleRequest, UpdateSecurityModuleRequest>
{
    public SecurityModulesController(ILookupCrudService<SecurityModuleDto, CreateSecurityModuleRequest, UpdateSecurityModuleRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/security/permissions")]
public sealed class SecurityPermissionsController : SecurityLookupCrudController<PermissionDto, CreatePermissionRequest, UpdatePermissionRequest>
{
    public SecurityPermissionsController(ILookupCrudService<PermissionDto, CreatePermissionRequest, UpdatePermissionRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/security/account-statuses")]
public sealed class SecurityAccountStatusesController : SecurityLookupCrudController<AccountStatusDto, CreateAccountStatusRequest, UpdateAccountStatusRequest>
{
    public SecurityAccountStatusesController(ILookupCrudService<AccountStatusDto, CreateAccountStatusRequest, UpdateAccountStatusRequest> service) : base(service) { }
}

[ApiController]
[Produces("application/json")]
[Route("api/security/activity-types")]
public sealed class SecurityActivityTypesController : SecurityLookupCrudController<ActivityTypeDto, CreateActivityTypeRequest, UpdateActivityTypeRequest>
{
    public SecurityActivityTypesController(ILookupCrudService<ActivityTypeDto, CreateActivityTypeRequest, UpdateActivityTypeRequest> service) : base(service) { }
}
