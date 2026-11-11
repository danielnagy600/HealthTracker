using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetWaterSettings;

public sealed class GetWaterSettingsQueryHandler(
    IWaterRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetWaterSettingsQuery, SettingsResponse>
{
    public async Task<SettingsResponse> Handle(GetWaterSettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var settings = await repository.GetOrCreateSettingsAsync(userId, cancellationToken);
        return mapper.Map<SettingsResponse>(settings);
    }
}
