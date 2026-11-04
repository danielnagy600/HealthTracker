using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Application.Models;
using HealthTracker.Modules.Calories.Domain;
using HealthTracker.Modules.Calories.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthTracker.Modules.Calories;

[ApiController]
[Route("api/calories")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CaloriesController(ICalorieService svc, ILogger<CaloriesController> logger) : ControllerBase
{
    [HttpGet("day")]
    [ProducesResponseType<DayCaloriesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DayCaloriesResponse>> GetDay(DateOnly? date, CancellationToken ct) =>
        await svc.GetDayAsync(date, ct);

    [HttpGet("entries/{id:guid}")]
    [ProducesResponseType<FoodEntryItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodEntryItem>> GetEntry(Guid id, CancellationToken ct) =>
        await svc.GetAsync(id, ct) is { } item ? item : NotFound();

    [HttpPost("entries")]
    [ProducesResponseType<FoodEntryItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FoodEntryItem>> AddEntry(SaveFoodEntryRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return Rejected(nameof(AddEntry), error);

        var item = await svc.AddAsync(req, ct);
        Log.FoodEntryCreated(logger, item.Id);
        return CreatedAtAction(nameof(GetEntry), new { id = item.Id }, item);
    }

    [HttpPut("entries/{id:guid}")]
    [ProducesResponseType<FoodEntryItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodEntryItem>> UpdateEntry(Guid id, SaveFoodEntryRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return Rejected(nameof(UpdateEntry), error);

        var item = await svc.UpdateAsync(id, req, ct);
        if (item is null)
        {
            Log.FoodEntryNotFound(logger, nameof(UpdateEntry), id);
            return NotFound();
        }

        Log.FoodEntryUpdated(logger, id);
        return item;
    }

    [HttpDelete("entries/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEntry(Guid id, CancellationToken ct)
    {
        if (!await svc.DeleteAsync(id, ct))
        {
            Log.FoodEntryNotFound(logger, nameof(DeleteEntry), id);
            return NotFound();
        }

        Log.FoodEntryDeleted(logger, id);
        return NoContent();
    }

    [HttpGet("goal")]
    [ProducesResponseType<GoalResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GoalResponse>> GetGoal(CancellationToken ct) =>
        await svc.GetGoalAsync(ct);

    [HttpPut("goal")]
    [ProducesResponseType<GoalResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GoalResponse>> UpdateGoal(UpdateGoalRequest req, CancellationToken ct)
    {
        if (CalorieGoal.Validate(req.DailyTargetKcal) is { } error)
            return Rejected(nameof(UpdateGoal), error);

        var goal = await svc.UpdateGoalAsync(req, ct);
        Log.GoalUpdated(logger);
        return goal;
    }

    [HttpGet("meals")]
    [ProducesResponseType<string[]>(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetMeals() => Ok(Enum.GetNames<MealType>());

    private ObjectResult Rejected(string action, string reason)
    {
        Log.RequestRejected(logger, action, reason);
        return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: reason);
    }

    private static string? Validate(SaveFoodEntryRequest req)
    {
        if (!CalorieService.IsKnownMeal(req.Meal))
            return $"Unknown meal: '{req.Meal}'. Available: {string.Join(", ", Enum.GetNames<MealType>())}.";

        return FoodEntry.Validate(req.Name, req.Calories);
    }
}
