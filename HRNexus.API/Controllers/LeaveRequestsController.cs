using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/leave-requests")]
public sealed class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [Authorize(Policy = AuthorizationPolicyNames.AuthenticatedUser)]
    [HttpPost]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.CreateLeaveRequestAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("employees/{employeeId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetEmployeeRequests([FromRoute, Range(1, int.MaxValue)] int employeeId, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetEmployeeLeaveRequestsAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.CanReviewLeave)]
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetPending(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetPendingLeaveRequestsAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.CanReviewLeave)]
    [HttpPatch("{leaveRequestId:int}/status")]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveRequestDto>> UpdateStatus([FromRoute, Range(1, int.MaxValue)] int leaveRequestId, [FromBody] ReviewLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.UpdateLeaveRequestStatusAsync(leaveRequestId, request, cancellationToken);
        return Ok(result);
    }
}
