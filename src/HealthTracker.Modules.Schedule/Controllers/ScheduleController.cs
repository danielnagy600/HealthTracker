using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Application.Dtos;
using HealthTracker.Modules.Schedule.Domain;
using HealthTracker.Modules.Schedule.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Schedule;

[ApiController]
[Route("api/schedule")]
[Authorize]
public sealed class ScheduleController(IScheduleService svc) : ControllerBase
{
    [HttpGet("day")]
    public async Task<IActionResult> GetDay(DateOnly? date, CancellationToken ct) =>
        Ok(await svc.GetDayAsync(date, ct));

    [HttpPost("activities")]
    public async Task<IActionResult> AddActivity(SaveActivityRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await svc.AddAsync(req, ct);
        return Created($"/api/schedule/activities/{item.Id}", item);
    }

    [HttpPut("activities/{id:guid}")]
    public async Task<IActionResult> UpdateActivity(Guid id, SaveActivityRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await svc.UpdateAsync(id, req, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("activities/{id:guid}")]
    public async Task<IActionResult> DeleteActivity(Guid id, CancellationToken ct) =>
        await svc.DeleteAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("colors")]
    public IActionResult GetColors() => Ok(Enum.GetNames<ActivityColor>());

    private static string? Validate(SaveActivityRequest req)
    {
        if (!ScheduleService.IsKnownColor(req.Color))
            return $"Unknown color: '{req.Color}'. Available: {string.Join(", ", Enum.GetNames<ActivityColor>())}.";

        return Activity.Validate(req.StartTime, req.EndTime, req.Title, req.Note);
    }
}
