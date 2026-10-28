using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Domain;
using HealthTracker.Modules.Calories.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Calories;

[ApiController]
[Route("api/calories")]
[Authorize]
public sealed class CaloriesController(ICalorieService svc) : ControllerBase
{
    [HttpGet("day")]
    public async Task<IActionResult> GetDay(DateOnly? date, CancellationToken ct) =>
        Ok(await svc.GetDayAsync(date, ct));

    [HttpPost("entries")]
    public async Task<IActionResult> AddEntry(SaveFoodEntryRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await svc.AddAsync(req, ct);
        return Created($"/api/calories/entries/{item.Id}", item);
    }

    [HttpPut("entries/{id:guid}")]
    public async Task<IActionResult> UpdateEntry(Guid id, SaveFoodEntryRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return BadRequest(error);

        var item = await svc.UpdateAsync(id, req, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("entries/{id:guid}")]
    public async Task<IActionResult> DeleteEntry(Guid id, CancellationToken ct) =>
        await svc.DeleteAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("goal")]
    public async Task<IActionResult> GetGoal(CancellationToken ct) =>
        Ok(await svc.GetGoalAsync(ct));

    [HttpPut("goal")]
    public async Task<IActionResult> UpdateGoal(UpdateGoalRequest req, CancellationToken ct)
    {
        if (CalorieGoal.Validate(req.DailyTargetKcal) is { } error)
            return BadRequest(error);

        return Ok(await svc.UpdateGoalAsync(req, ct));
    }

    [HttpGet("meals")]
    public IActionResult GetMeals() => Ok(Enum.GetNames<MealType>());

    private static string? Validate(SaveFoodEntryRequest req)
    {
        if (!CalorieService.IsKnownMeal(req.Meal))
            return $"Unknown meal: '{req.Meal}'. Available: {string.Join(", ", Enum.GetNames<MealType>())}.";

        return FoodEntry.Validate(req.Name, req.Calories);
    }
}
