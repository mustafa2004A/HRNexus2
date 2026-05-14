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
[Route("api/security/roles/{roleId:int}/permissions")]
public sealed class SecurityRolePermissionsController : ControllerBase
{
    private readonly ISecurityAdminService _securityAdminService;

    public SecurityRolePermissionsController(ISecurityAdminService securityAdminService)
    {
        _securityAdminService = securityAdminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RolePermissionMaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<RolePermissionMaskDto>>> GetRolePermissions(
        [FromRoute, Range(1, int.MaxValue)] int roleId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetRolePermissionsAsync(roleId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{moduleId:int}")]
    [ProducesResponseType(typeof(RolePermissionMaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RolePermissionMaskDto>> SetRolePermission(
        [FromRoute, Range(1, int.MaxValue)] int roleId,
        [FromRoute, Range(1, int.MaxValue)] int moduleId,
        [FromBody] SetPermissionMaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.SetRolePermissionAsync(roleId, moduleId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{moduleId:int}")]
    [ProducesResponseType(typeof(RolePermissionMaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RolePermissionMaskDto>> RemoveRolePermission(
        [FromRoute, Range(1, int.MaxValue)] int roleId,
        [FromRoute, Range(1, int.MaxValue)] int moduleId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.RemoveRolePermissionAsync(roleId, moduleId, cancellationToken);
        return Ok(result);
    }
}
