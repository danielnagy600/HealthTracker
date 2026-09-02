using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Calories.Controllers;

// Controller-alapú megfelelője a CaloriesModule.MapCaloriesModule minimal API
// végpontjainak – ugyanazok az útvonalak, ugyanaz az ICalorieService, csak
// attribútum-routing-gal és osztályba szervezve, nem lambdákkal.
[ApiController]
[Route("api/calories")]
[Authorize]
public class CaloriesController : ControllerBase
{
    private readonly ICalorieService _service;

    public CaloriesController(ICalorieService service)
    {
        _service = service;
    }

    [HttpGet("day")]
    public async Task<IActionResult> GetDay([FromQuery] DateOnly? date, CancellationToken ct)
    {
        return Ok(await _service.GetDayAsync(date, ct));
    }

    [HttpPost("entries")]
    public async Task<IActionResult> Add([FromBody] SaveFoodEntryRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await _service.AddAsync(req, ct);
        return Created($"/api/calories/entries/{item.Id}", item);
    }

    [HttpPut("entries/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveFoodEntryRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await _service.UpdateAsync(id, req, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("entries/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }

    [HttpGet("goal")]
    public async Task<IActionResult> GetGoal(CancellationToken ct)
    {
        return Ok(await _service.GetGoalAsync(ct));
    }

    [HttpPut("goal")]
    public async Task<IActionResult> UpdateGoal([FromBody] UpdateGoalRequest req, CancellationToken ct)
    {
        if (CalorieGoal.Validate(req.DailyTargetKcal) is { } error)
            return BadRequest(error);

        return Ok(await _service.UpdateGoalAsync(req, ct));
    }

    [HttpGet("meals")]
    public IActionResult GetMeals()
    {
        return Ok(Enum.GetNames<MealType>());
    }

    private static string? Validate(SaveFoodEntryRequest req)
    {
        if (!CalorieService.IsKnownMeal(req.Meal))
            return $"Unknown meal: '{req.Meal}'. Available: {string.Join(", ", Enum.GetNames<MealType>())}.";

        return FoodEntry.Validate(req.Name, req.Calories);
    }
}
