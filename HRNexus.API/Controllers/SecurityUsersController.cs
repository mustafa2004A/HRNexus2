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
[Route("api/security/users")]
public sealed class SecurityUsersController : ControllerBase
{
    private readonly ISecurityAdminService _securityAdminService;

    public SecurityUsersController(ISecurityAdminService securityAdminService)
    {
        _securityAdminService = securityAdminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SecurityUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SecurityUserDto>>> ListUsers(
        [FromQuery, StringLength(100)] string? search,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.ListUsersAsync(search, includeInactive, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(SecurityUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserDto>> GetUser(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SecurityUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserDto>> CreateUser(
        [FromBody] CreateSecurityUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { userId = result.UserId }, result);
    }

    [HttpPut("{userId:int}")]
    [ProducesResponseType(typeof(SecurityUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserDto>> UpdateUser(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        [FromBody] UpdateSecurityUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.UpdateUserAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{userId:int}/reset-password")]
    [ProducesResponseType(typeof(SecurityUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserDto>> ResetPassword(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        [FromBody] ResetSecurityUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.ResetPasswordAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{userId:int}/lock")]
    [ProducesResponseType(typeof(SecurityUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserDto>> LockUser(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.LockUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{userId:int}/unlock")]
    [ProducesResponseType(typeof(SecurityUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserDto>> UnlockUser(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.UnlockUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:int}/roles")]
    [ProducesResponseType(typeof(IReadOnlyList<SecurityUserRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SecurityUserRoleDto>>> GetUserRoles(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetUserRolesAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{userId:int}/roles")]
    [ProducesResponseType(typeof(SecurityUserRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserRoleDto>> AssignRole(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        [FromBody] AssignUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.AssignRoleAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{userId:int}/roles/{roleId:int}")]
    [ProducesResponseType(typeof(SecurityUserRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecurityUserRoleDto>> RemoveRole(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        [FromRoute, Range(1, int.MaxValue)] int roleId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.RemoveRoleAsync(userId, roleId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:int}/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<UserPermissionOverrideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<UserPermissionOverrideDto>>> GetUserPermissions(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetUserPermissionsAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{userId:int}/permissions/{moduleId:int}")]
    [ProducesResponseType(typeof(UserPermissionOverrideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserPermissionOverrideDto>> SetUserPermission(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        [FromRoute, Range(1, int.MaxValue)] int moduleId,
        [FromBody] SetPermissionMaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.SetUserPermissionAsync(userId, moduleId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{userId:int}/permissions/{moduleId:int}")]
    [ProducesResponseType(typeof(UserPermissionOverrideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserPermissionOverrideDto>> RemoveUserPermission(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        [FromRoute, Range(1, int.MaxValue)] int moduleId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.RemoveUserPermissionAsync(userId, moduleId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:int}/refresh-tokens")]
    [ProducesResponseType(typeof(IReadOnlyList<RefreshTokenMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<RefreshTokenMetadataDto>>> GetRefreshTokens(
        [FromRoute, Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.GetRefreshTokensByUserAsync(userId, cancellationToken);
        return Ok(result);
    }
}
