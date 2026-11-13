using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Water.Models;
using HealthTracker.Domain.DomainServices;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetWaterReminder;

public sealed class GetWaterReminderQueryHandler(
    IWaterRepository repository, IClock clock, ICurrentUser currentUser)
    : IRequestHandler<GetWaterReminderQuery, ReminderResponse>
{
    public async Task<ReminderResponse> Handle(GetWaterReminderQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var now = clock.Now;
        var today = DateOnly.FromDateTime(now.DateTime);

        var settings = await repository.GetOrCreateSettingsAsync(userId, cancellationToken);
        var intakes = await repository.GetIntakesForDateAsync(userId, today, cancellationToken);
        var consumed = intakes.Sum(i => i.AmountMl);

        var r = WaterReminderCalculator.Calculate(settings, consumed, now);

        return new ReminderResponse(
            r.ConsumedMl, r.TargetMl, r.RemainingMl, r.ExpectedByNowMl, r.DeficitMl,
            r.Status.ToString(), r.NextDoseMl, r.NextReminderAt, r.Message);
    }
}
