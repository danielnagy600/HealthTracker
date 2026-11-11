using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Schedule.Models;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Commands.UpdateActivity;

public sealed class UpdateActivityCommandHandler(
    IActivityRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<UpdateActivityCommand, ActivityItem?>
{
    public async Task<ActivityItem?> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        var validation = await new UpdateActivityCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation);

        var userId = currentUser.RequireUserId();

        var activity = await repository.FindAsync(userId, request.Id, cancellationToken);
        if (activity is null)
            return null;

        activity.Update(
            request.Date, request.StartTime, request.EndTime,
            request.Title, ActivityColorParser.Parse(request.Color), request.Note);

        await repository.UpdateAsync(activity, cancellationToken);
        return mapper.Map<ActivityItem>(activity);
    }
}
