using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetWaterDay;

public sealed class GetWaterDayQueryHandler(
    IWaterRepository repository, IClock clock, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetWaterDayQuery, DailySummaryResponse>
{
    public async Task<DailySummaryResponse> Handle(GetWaterDayQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var day = request.Date ?? DateOnly.FromDateTime(clock.Now.DateTime);

        var settings = await repository.GetOrCreateSettingsAsync(userId, cancellationToken);
        var intakes = await repository.GetIntakesForDateAsync(userId, day, cancellationToken);

        var consumed = intakes.Sum(i => i.AmountMl);
        var remaining = Math.Max(0, settings.DailyTargetMl - consumed);
        var percent = settings.DailyTargetMl == 0
            ? 0
            : Math.Round(100.0 * consumed / settings.DailyTargetMl, 1);

        var items = intakes
            .OrderBy(i => i.RecordedAt)
            .Select(i => mapper.Map<IntakeItem>(i))
            .ToList();

        return new DailySummaryResponse(day, settings.DailyTargetMl, consumed, remaining, percent, items);
    }
}
