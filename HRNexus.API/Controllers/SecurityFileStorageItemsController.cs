using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Files;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/security/file-storage-items")]
[Authorize(Policy = AuthorizationPolicyNames.SecurityAdmin)]
public sealed class SecurityFileStorageItemsController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public SecurityFileStorageItemsController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("{fileStorageItemId:int}/verify-integrity")]
    [ProducesResponseType(typeof(FileIntegrityVerificationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FileIntegrityVerificationResultDto>> VerifyIntegrity(
        [FromRoute, Range(1, int.MaxValue)] int fileStorageItemId,
        CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.VerifyIntegrityAsync(fileStorageItemId, cancellationToken);
        return Ok(result);
    }
}
