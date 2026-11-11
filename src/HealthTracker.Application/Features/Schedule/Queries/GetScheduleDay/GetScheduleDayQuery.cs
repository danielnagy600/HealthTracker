using HealthTracker.Application.Features.Schedule.Models;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Queries.GetScheduleDay;

/// <param name="Date">A lekérdezett nap; null esetén a mai nap.</param>
public record GetScheduleDayQuery(DateOnly? Date = null) : IRequest<DayScheduleResponse>;
