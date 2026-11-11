using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using MediatR;

namespace HealthTracker.Application.Features.Water.Commands.DeleteIntake;

public sealed class DeleteIntakeCommandHandler(IWaterRepository repository, ICurrentUser currentUser)
    : IRequestHandler<DeleteIntakeCommand, bool>
{
    public async Task<bool> Handle(DeleteIntakeCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        var intake = await repository.FindIntakeAsync(userId, request.Id, cancellationToken);
        if (intake is null)
            return false;

        await repository.RemoveIntakeAsync(intake, cancellationToken);
        return true;
    }
}
