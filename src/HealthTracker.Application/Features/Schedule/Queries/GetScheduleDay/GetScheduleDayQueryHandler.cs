using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Schedule.Models;
using HealthTracker.Domain.DomainServices;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Queries.GetScheduleDay;

public sealed class GetScheduleDayQueryHandler(
    IActivityRepository repository, IClock clock, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetScheduleDayQuery, DayScheduleResponse>
{
    public async Task<DayScheduleResponse> Handle(GetScheduleDayQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var day = request.Date ?? DateOnly.FromDateTime(clock.Now.DateTime);

        var activities = await repository.GetForDateAsync(userId, day, cancellationToken);

        var (windowStart, windowEnd) = DayPlanCalculator.WindowFor(activities);
        var plan = DayPlanCalculator.Calculate(activities, windowStart, windowEnd);

        var items = activities
            .OrderBy(a => a.StartTime)
            .ThenBy(a => a.EndTime)
            .Select(a => mapper.Map<ActivityItem>(a))
            .ToList();

        var freeSlots = plan.FreeSlots
            .Select(s => new TimeSlot(s.Start, s.End, s.DurationMinutes))
            .ToList();

        var conflicts = plan.Conflicts
            .Select(c => new ConflictItem(
                c.First.Id, c.Second.Id, c.First.Title, c.Second.Title,
                c.Overlap.Start, c.Overlap.End, c.Overlap.DurationMinutes))
            .ToList();

        return new DayScheduleResponse(
            day, plan.WindowStart, plan.WindowEnd, plan.BusyMinutes, plan.FreeMinutes,
            items, freeSlots, conflicts);
    }
}
