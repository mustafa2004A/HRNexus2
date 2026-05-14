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
[Route("api/security/permission-audits")]
public sealed class SecurityPermissionAuditsController : ControllerBase
{
    private readonly ISecurityAdminService _securityAdminService;

    public SecurityPermissionAuditsController(ISecurityAdminService securityAdminService)
    {
        _securityAdminService = securityAdminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionAuditDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PermissionAuditDto>>> ListPermissionAudits(
        [FromQuery] PermissionAuditFilter filter,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.ListPermissionAuditsAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{auditId:int}")]
    [ProducesResponseType(typeof(PermissionAuditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PermissionAuditDto>> GetPermissionAudit(
        [FromRoute, Range(1, int.MaxValue)] int auditId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetPermissionAuditAsync(auditId, cancellationToken);
        return Ok(result);
    }
}
