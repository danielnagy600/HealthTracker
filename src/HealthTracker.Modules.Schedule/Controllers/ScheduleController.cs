using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Schedule.Controllers;

// Controller-alapú megfelelője a ScheduleModule.MapScheduleModule minimal API
// végpontjainak – ugyanazok az útvonalak, ugyanaz az IScheduleService, csak
// attribútum-routing-gal és osztályba szervezve, nem lambdákkal.
[ApiController]
[Route("api/schedule")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _service;

    public ScheduleController(IScheduleService service)
    {
        _service = service;
    }

    [HttpGet("day")]
    public async Task<IActionResult> GetDay([FromQuery] DateOnly? date, CancellationToken ct)
    {
        return Ok(await _service.GetDayAsync(date, ct));
    }

    [HttpPost("activities")]
    public async Task<IActionResult> Add([FromBody] SaveActivityRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await _service.AddAsync(req, ct);
        return Created($"/api/schedule/activities/{item.Id}", item);
    }

    [HttpPut("activities/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveActivityRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await _service.UpdateAsync(id, req, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("activities/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }

    [HttpGet("colors")]
    public IActionResult GetColors()
    {
        return Ok(Enum.GetNames<ActivityColor>());
    }

    private static string? Validate(SaveActivityRequest req)
    {
        if (!ScheduleService.IsKnownColor(req.Color))
            return $"Unknown color: '{req.Color}'. Available: {string.Join(", ", Enum.GetNames<ActivityColor>())}.";

        return Activity.Validate(req.StartTime, req.EndTime, req.Title, req.Note);
    }
}
