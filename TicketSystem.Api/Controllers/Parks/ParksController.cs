using Microsoft.AspNetCore.Mvc;
using TicketSystem.Services.Parks;

namespace TicketSystem.Api.Controllers.Parks;

[ApiController]
[Route("api/parks")]
public sealed class ParksController : ControllerBase
{
    private readonly ParkService _parkService;

    public ParksController(ParkService parkService)
    {
        _parkService = parkService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ParkDto>> GetParks()
    {
        return Ok(_parkService.GetParks());
    }

    [HttpGet("{uniqueName}/availability/{year:int}/{month:int}")]
    public ActionResult<IReadOnlyList<DailyTicketAvailabilityDto>> GetMonthlyAvailability(
        string uniqueName,
        int year,
        int month)
    {
        var availability = _parkService.GetMonthlyAvailability(uniqueName, year, month);

        return Ok(availability);
    }
}
