using HealthTracker.Application.Features.Calories.Commands.AddFoodEntry;
using HealthTracker.Application.Features.Calories.Commands.DeleteFoodEntry;
using HealthTracker.Application.Features.Calories.Commands.UpdateCalorieGoal;
using HealthTracker.Application.Features.Calories.Commands.UpdateFoodEntry;
using HealthTracker.Application.Features.Calories.Models;
using HealthTracker.Application.Features.Calories.Queries.GetCalorieGoal;
using HealthTracker.Application.Features.Calories.Queries.GetCaloriesDay;
using HealthTracker.Application.Features.Calories.Queries.GetFoodEntry;
using HealthTracker.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Api.Controllers;

[ApiController]
[Route("api/calories")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CaloriesController(IMediator mediator, ILogger<CaloriesController> logger) : ControllerBase
{
    [HttpGet("day")]
    [ProducesResponseType<DayCaloriesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DayCaloriesResponse>> GetDay(DateOnly? date, CancellationToken ct) =>
        await mediator.Send(new GetCaloriesDayQuery(date), ct);

    [HttpGet("entries/{id:guid}")]
    [ProducesResponseType<FoodEntryItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodEntryItem>> GetEntry(Guid id, CancellationToken ct) =>
        await mediator.Send(new GetFoodEntryQuery(id), ct) is { } item ? item : NotFound();

    [HttpPost("entries")]
    [ProducesResponseType<FoodEntryItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FoodEntryItem>> AddEntry(SaveFoodEntryRequest req, CancellationToken ct)
    {
        var item = await mediator.Send(new AddFoodEntryCommand(req.Date, req.Meal, req.Name, req.Calories), ct);
        Log.FoodEntryCreated(logger, item.Id);
        return CreatedAtAction(nameof(GetEntry), new { id = item.Id }, item);
    }

    [HttpPut("entries/{id:guid}")]
    [ProducesResponseType<FoodEntryItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodEntryItem>> UpdateEntry(Guid id, SaveFoodEntryRequest req, CancellationToken ct)
    {
        var item = await mediator.Send(
            new UpdateFoodEntryCommand(id, req.Date, req.Meal, req.Name, req.Calories), ct);
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
        if (!await mediator.Send(new DeleteFoodEntryCommand(id), ct))
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
        await mediator.Send(new GetCalorieGoalQuery(), ct);

    [HttpPut("goal")]
    [ProducesResponseType<GoalResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GoalResponse>> UpdateGoal(UpdateGoalRequest req, CancellationToken ct)
    {
        var goal = await mediator.Send(new UpdateCalorieGoalCommand(req.DailyTargetKcal), ct);
        Log.CalorieGoalUpdated(logger);
        return goal;
    }

    [HttpGet("meals")]
    [ProducesResponseType<string[]>(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetMeals() => Ok(Enum.GetNames<MealType>());
}
