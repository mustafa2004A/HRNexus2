using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Employee;
using HRNexus.Business.Models.Files;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/employees")]
public sealed class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeJobHistoryService _employeeJobHistoryService;
    private readonly IEmployeeDocumentService _employeeDocumentService;
    private readonly IEmployeeFamilyMemberService _employeeFamilyMemberService;

    public EmployeeController(
        IEmployeeService employeeService,
        IEmployeeJobHistoryService employeeJobHistoryService,
        IEmployeeDocumentService employeeDocumentService,
        IEmployeeFamilyMemberService employeeFamilyMemberService)
    {
        _employeeService = employeeService;
        _employeeJobHistoryService = employeeJobHistoryService;
        _employeeDocumentService = employeeDocumentService;
        _employeeFamilyMemberService = employeeFamilyMemberService;
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<EmployeeSummaryDto>>> List(
        [FromQuery] string? search,
        [FromQuery] bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.ListAsync(search, includeDeleted, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}")]
    [ProducesResponseType(typeof(EmployeeDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDetailsDto>> GetDetails([FromRoute, Range(1, int.MaxValue)] int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetDetailsAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDetailsDto>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDetails), new { employeeId = result.EmployeeId }, result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPut("{employeeId:int}")]
    [ProducesResponseType(typeof(EmployeeDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDetailsDto>> Update(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateAsync(employeeId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpDelete("{employeeId:int}")]
    [ProducesResponseType(typeof(EmployeeDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDetailsDto>> Delete([FromRoute, Range(1, int.MaxValue)] int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeleteAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/current-context")]
    [ProducesResponseType(typeof(EmployeeCurrentContextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeCurrentContextDto>> GetCurrentContext([FromRoute, Range(1, int.MaxValue)] int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetCurrentContextAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/job-history")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeJobHistoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<EmployeeJobHistoryItemDto>>> GetJobHistory([FromRoute, Range(1, int.MaxValue)] int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeJobHistoryService.GetEmployeeJobHistoryAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/job-history/{jobHistoryId:int}")]
    [ProducesResponseType(typeof(EmployeeJobHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeJobHistoryDto>> GetJobHistoryById(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int jobHistoryId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeJobHistoryService.GetByIdAsync(employeeId, jobHistoryId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPost("{employeeId:int}/job-history")]
    [ProducesResponseType(typeof(EmployeeJobHistoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeJobHistoryDto>> CreateJobHistory(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromBody] CreateEmployeeJobHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeJobHistoryService.CreateAsync(employeeId, request, cancellationToken);
        return CreatedAtAction(nameof(GetJobHistoryById), new { employeeId, jobHistoryId = result.JobHistoryId }, result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPut("{employeeId:int}/job-history/{jobHistoryId:int}")]
    [ProducesResponseType(typeof(EmployeeJobHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeJobHistoryDto>> UpdateJobHistory(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int jobHistoryId,
        [FromBody] UpdateEmployeeJobHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeJobHistoryService.UpdateAsync(employeeId, jobHistoryId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpDelete("{employeeId:int}/job-history/{jobHistoryId:int}")]
    [ProducesResponseType(typeof(EmployeeJobHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeJobHistoryDto>> DeleteJobHistory(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int jobHistoryId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeJobHistoryService.DeleteAsync(employeeId, jobHistoryId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/documents")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDocumentItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDocumentItemDto>>> GetDocuments([FromRoute, Range(1, int.MaxValue)] int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeDocumentService.GetEmployeeDocumentsAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/documents/{documentId:int}")]
    [ProducesResponseType(typeof(EmployeeDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDocumentDto>> GetDocument(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int documentId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeDocumentService.GetByIdAsync(employeeId, documentId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpPost("{employeeId:int}/documents/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EmployeeDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDocumentDto>> UploadDocument(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromForm] UploadEmployeeDocumentForm request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest("Uploaded document file is required.");
        }

        using var stream = request.File.OpenReadStream();
        var file = new FileUploadContent(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length);

        var uploadRequest = new UploadEmployeeDocumentRequest
        {
            DocumentTypeId = request.DocumentTypeId,
            DocumentName = request.DocumentName,
            ReferenceNumber = request.ReferenceNumber,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            Remarks = request.Remarks
        };

        var result = await _employeeDocumentService.UploadAsync(employeeId, uploadRequest, file, cancellationToken);
        return CreatedAtAction(nameof(GetDocument), new { employeeId, documentId = result.DocumentId }, result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPut("{employeeId:int}/documents/{documentId:int}")]
    [ProducesResponseType(typeof(EmployeeDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDocumentDto>> UpdateDocument(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int documentId,
        [FromBody] UpdateEmployeeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeDocumentService.UpdateAsync(employeeId, documentId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpDelete("{employeeId:int}/documents/{documentId:int}")]
    [ProducesResponseType(typeof(EmployeeDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDocumentDto>> DeleteDocument(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int documentId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeDocumentService.DeleteAsync(employeeId, documentId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/family-members")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeFamilyMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<EmployeeFamilyMemberDto>>> GetFamilyMembers(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeFamilyMemberService.GetByEmployeeAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("{employeeId:int}/family-members/{familyMemberId:int}")]
    [ProducesResponseType(typeof(EmployeeFamilyMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeFamilyMemberDto>> GetFamilyMember(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int familyMemberId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeFamilyMemberService.GetByIdAsync(employeeId, familyMemberId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPost("{employeeId:int}/family-members")]
    [ProducesResponseType(typeof(EmployeeFamilyMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeFamilyMemberDto>> CreateFamilyMember(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromBody] CreateEmployeeFamilyMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeFamilyMemberService.CreateAsync(employeeId, request, cancellationToken);
        return CreatedAtAction(nameof(GetFamilyMember), new { employeeId, familyMemberId = result.FamilyMemberId }, result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPut("{employeeId:int}/family-members/{familyMemberId:int}")]
    [ProducesResponseType(typeof(EmployeeFamilyMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeFamilyMemberDto>> UpdateFamilyMember(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int familyMemberId,
        [FromBody] UpdateEmployeeFamilyMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeFamilyMemberService.UpdateAsync(employeeId, familyMemberId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpDelete("{employeeId:int}/family-members/{familyMemberId:int}")]
    [ProducesResponseType(typeof(EmployeeFamilyMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeFamilyMemberDto>> DeleteFamilyMember(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromRoute, Range(1, int.MaxValue)] int familyMemberId,
        CancellationToken cancellationToken)
    {
        var result = await _employeeFamilyMemberService.DeleteAsync(employeeId, familyMemberId, cancellationToken);
        return Ok(result);
    }
}

public sealed class UploadEmployeeDocumentForm
{
    [Required]
    public IFormFile? File { get; set; }

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
