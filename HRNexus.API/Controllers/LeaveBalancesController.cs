using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/leave-balances")]
public sealed class LeaveBalancesController : ControllerBase
{
    private readonly ILeaveBalanceService _leaveBalanceService;

    public LeaveBalancesController(ILeaveBalanceService leaveBalanceService)
    {
        _leaveBalanceService = leaveBalanceService;
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaveBalanceDto>>> ListBalances(
        [FromQuery, Range(1, int.MaxValue)] int? employeeId,
        [FromQuery, Range(1, int.MaxValue)] int? leaveTypeId,
        [FromQuery, Range(2000, 2100)] int? year,
        CancellationToken cancellationToken)
    {
        var result = await _leaveBalanceService.ListBalancesAsync(employeeId, leaveTypeId, year, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpGet("{leaveBalanceId:int}")]
    [ProducesResponseType(typeof(LeaveBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveBalanceDto>> GetBalance(
        [FromRoute, Range(1, int.MaxValue)] int leaveBalanceId,
        CancellationToken cancellationToken)
    {
        var result = await _leaveBalanceService.GetBalanceAsync(leaveBalanceId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.SelfOrHr)]
    [HttpGet("employees/{employeeId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaveBalanceDto>>> GetEmployeeBalances(
        [FromRoute, Range(1, int.MaxValue)] int employeeId,
        [FromQuery, Range(2000, 2100)] int? year,
        CancellationToken cancellationToken)
    {
        var result = await _leaveBalanceService.GetEmployeeBalancesAsync(employeeId, year, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
    [HttpPut]
    [ProducesResponseType(typeof(LeaveBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeaveBalanceDto>> UpsertBalance([FromBody] UpsertLeaveBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaveBalanceService.UpsertBalanceAsync(request, cancellationToken);
        return Ok(result);
    }
}
