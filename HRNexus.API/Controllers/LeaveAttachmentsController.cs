using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Files;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/leave-attachments")]
public sealed class LeaveAttachmentsController : ControllerBase
{
    private readonly ILeaveAttachmentService _leaveAttachmentService;

    public LeaveAttachmentsController(ILeaveAttachmentService leaveAttachmentService)
    {
        _leaveAttachmentService = leaveAttachmentService;
    }

    [Authorize(Policy = AuthorizationPolicyNames.AuthenticatedUser)]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(LeaveAttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveAttachmentDto>> Upload(
        [FromForm] UploadLeaveAttachmentForm request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest("Uploaded file is required.");
        }

        using var stream = request.File.OpenReadStream();
        var file = new FileUploadContent(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length);

        var result = await _leaveAttachmentService.UploadAttachmentAsync(
            request.LeaveRequestId,
            file,
            request.UploadedByUserId,
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.AuthenticatedUser)]
    [HttpGet("leave-requests/{leaveRequestId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaveAttachmentDto>>> GetForLeaveRequest([FromRoute, Range(1, int.MaxValue)] int leaveRequestId, CancellationToken cancellationToken)
    {
        var result = await _leaveAttachmentService.GetLeaveRequestAttachmentsAsync(leaveRequestId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.AuthenticatedUser)]
    [HttpGet("{leaveAttachmentId:int}")]
    [ProducesResponseType(typeof(LeaveAttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveAttachmentDto>> GetById(
        [FromRoute, Range(1, int.MaxValue)] int leaveAttachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _leaveAttachmentService.GetAttachmentAsync(leaveAttachmentId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.CanReviewLeave)]
    [HttpDelete("{leaveAttachmentId:int}")]
    [ProducesResponseType(typeof(LeaveAttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveAttachmentDto>> Deactivate(
        [FromRoute, Range(1, int.MaxValue)] int leaveAttachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _leaveAttachmentService.DeactivateAttachmentAsync(leaveAttachmentId, cancellationToken);
        return Ok(result);
    }
}

public sealed class UploadLeaveAttachmentForm
{
    [Range(1, int.MaxValue)]
    public int LeaveRequestId { get; set; }

    [Required]
    public IFormFile? File { get; set; }

    [Range(1, int.MaxValue)]
    public int? UploadedByUserId { get; set; }
}
