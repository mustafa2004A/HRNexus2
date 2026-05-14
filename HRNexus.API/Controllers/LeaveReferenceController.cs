using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/leave-reference")]
public sealed class LeaveReferenceController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveReferenceController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [AllowAnonymous]
    [HttpGet("leave-types")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaveTypeDto>>> GetLeaveTypes(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetLeaveTypesAsync(cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("request-statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<RequestStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<RequestStatusDto>>> GetRequestStatuses(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetRequestStatusesAsync(cancellationToken);
        return Ok(result);
    }
}
