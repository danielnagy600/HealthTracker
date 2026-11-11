using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Commands.UpdateWaterSettings;

public sealed class UpdateWaterSettingsCommandHandler(
    IWaterRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<UpdateWaterSettingsCommand, SettingsResponse>
{
    public async Task<SettingsResponse> Handle(UpdateWaterSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var settings = await repository.GetOrCreateSettingsAsync(userId, cancellationToken);

        settings.DailyTargetMl = request.DailyTargetMl;
        settings.WakeTime = request.WakeTime;
        settings.SleepTime = request.SleepTime;

        await repository.UpdateSettingsAsync(settings, cancellationToken);
        return mapper.Map<SettingsResponse>(settings);
    }
}
