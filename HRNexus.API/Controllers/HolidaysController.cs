using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/holidays")]
public sealed class HolidaysController : ControllerBase
{
    private readonly IHolidayService _holidayService;

    public HolidaysController(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    [AllowAnonymous]
    [HttpGet("GetHolidays")]
    [ProducesResponseType(typeof(IReadOnlyList<HolidayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> Get([FromQuery, Range(2000, 2100)] int? year, CancellationToken cancellationToken)
    {
        var result = await _holidayService.GetHolidayListAsync(year, cancellationToken);
        return Ok(result);
    }
}
