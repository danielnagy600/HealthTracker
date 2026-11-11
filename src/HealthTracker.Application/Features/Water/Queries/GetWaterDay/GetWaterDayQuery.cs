using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetWaterDay;

/// <param name="Date">A lekérdezett nap; null esetén a mai nap.</param>
public record GetWaterDayQuery(DateOnly? Date = null) : IRequest<DailySummaryResponse>;
