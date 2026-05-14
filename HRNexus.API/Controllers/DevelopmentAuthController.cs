using HRNexus.API.Security;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/dev/auth")]
public sealed class DevelopmentAuthController : ControllerBase
{
    private readonly IDevelopmentPasswordBootstrapService _passwordBootstrapService;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IClientIpAddressProvider _clientIpAddressProvider;

    public DevelopmentAuthController(
        IDevelopmentPasswordBootstrapService passwordBootstrapService,
        IConfiguration configuration,
        IHostEnvironment environment,
        IClientIpAddressProvider clientIpAddressProvider)
    {
        _passwordBootstrapService = passwordBootstrapService;
        _configuration = configuration;
        _environment = environment;
        _clientIpAddressProvider = clientIpAddressProvider;
    }

    [AllowAnonymous]
    [HttpPost("reseed-demo-passwords")]
    [EnableRateLimiting("AuthToken")]
    [ProducesResponseType(typeof(DevelopmentPasswordBootstrapResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DevelopmentPasswordBootstrapResultDto>> ReseedDemoPasswords(
        [FromBody] DevelopmentPasswordBootstrapRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsPasswordBootstrapAllowed())
        {
            return NotFound();
        }

        var result = await _passwordBootstrapService.ReseedDemoPasswordsAsync(request, cancellationToken);
        return Ok(result);
    }

    private bool IsPasswordBootstrapAllowed()
    {
        if (!_environment.IsDevelopment())
        {
            return false;
        }

        if (!_configuration.GetValue<bool>("DevelopmentAuth:PasswordBootstrapEnabled"))
        {
            return false;
        }

        return string.Equals(_clientIpAddressProvider.GetClientIpAddress(), "127.0.0.1", StringComparison.Ordinal);
    }
}
