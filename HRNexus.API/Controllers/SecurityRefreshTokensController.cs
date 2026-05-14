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
[Route("api/security/refresh-tokens")]
public sealed class SecurityRefreshTokensController : ControllerBase
{
    private readonly ISecurityAdminService _securityAdminService;

    public SecurityRefreshTokensController(ISecurityAdminService securityAdminService)
    {
        _securityAdminService = securityAdminService;
    }

    [HttpPost("{refreshTokenId:int}/revoke")]
    [ProducesResponseType(typeof(RefreshTokenMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefreshTokenMetadataDto>> RevokeRefreshToken(
        [FromRoute, Range(1, int.MaxValue)] int refreshTokenId,
        CancellationToken cancellationToken)
    {
        var result = await _securityAdminService.RevokeRefreshTokenAsync(refreshTokenId, cancellationToken);
        return Ok(result);
    }
}
