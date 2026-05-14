using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Security;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
[Route("api/security/activity-logs")]
public sealed class SecurityActivityLogsController : ControllerBase
{
    private readonly ISecurityAdminService _securityAdminService;

    public SecurityActivityLogsController(ISecurityAdminService securityAdminService)
    {
        _securityAdminService = securityAdminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<UserActivityLogDto>>> ListActivityLogs(
        [FromQuery] SecurityActivityLogFilter filter,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.ListActivityLogsAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{activityLogId:int}")]
    [ProducesResponseType(typeof(UserActivityLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserActivityLogDto>> GetActivityLog(
        [FromRoute, Range(1, int.MaxValue)] int activityLogId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetActivityLogAsync(activityLogId, cancellationToken);
        return Ok(result);
    }
}
