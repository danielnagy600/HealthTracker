using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Schedule.Models;
using HealthTracker.Domain.Entities;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Commands.AddActivity;

public sealed class AddActivityCommandHandler(
    IActivityRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<AddActivityCommand, ActivityItem>
{
    public async Task<ActivityItem> Handle(AddActivityCommand request, CancellationToken cancellationToken)
    {
        var validation = await new AddActivityCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation);

        var userId = currentUser.RequireUserId();

        var activity = new Activity(
            Guid.NewGuid(), userId, request.Date, request.StartTime, request.EndTime,
            request.Title, ActivityColorParser.Parse(request.Color), request.Note);

        await repository.AddAsync(activity, cancellationToken);
        return mapper.Map<ActivityItem>(activity);
    }
}
