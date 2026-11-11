using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Water.Models;
using HealthTracker.Domain.Entities;
using MediatR;

namespace HealthTracker.Application.Features.Water.Commands.AddIntake;

public sealed class AddIntakeCommandHandler(
    IWaterRepository repository, IClock clock, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<AddIntakeCommand, IntakeItem>
{
    public async Task<IntakeItem> Handle(AddIntakeCommand request, CancellationToken cancellationToken)
    {
        var validation = await new AddIntakeCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation);

        var userId = currentUser.RequireUserId();
        var now = clock.Now;
        var today = DateOnly.FromDateTime(now.DateTime);

        var intake = new WaterIntake(Guid.NewGuid(), userId, today, now.ToUniversalTime(), request.AmountMl);
        await repository.AddIntakeAsync(intake, cancellationToken);

        return mapper.Map<IntakeItem>(intake);
    }
}
