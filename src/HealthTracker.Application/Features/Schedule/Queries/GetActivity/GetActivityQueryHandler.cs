using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Schedule.Models;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Queries.GetActivity;

public sealed class GetActivityQueryHandler(
    IActivityRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetActivityQuery, ActivityItem?>
{
    public async Task<ActivityItem?> Handle(GetActivityQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        var activity = await repository.FindAsync(userId, request.Id, cancellationToken);
        return activity is null ? null : mapper.Map<ActivityItem>(activity);
    }
}
