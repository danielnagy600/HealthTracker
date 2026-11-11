using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Commands.DeleteActivity;

public sealed class DeleteActivityCommandHandler(IActivityRepository repository, ICurrentUser currentUser)
    : IRequestHandler<DeleteActivityCommand, bool>
{
    public async Task<bool> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        var activity = await repository.FindAsync(userId, request.Id, cancellationToken);
        if (activity is null)
            return false;

        await repository.RemoveAsync(activity, cancellationToken);
        return true;
    }
}
