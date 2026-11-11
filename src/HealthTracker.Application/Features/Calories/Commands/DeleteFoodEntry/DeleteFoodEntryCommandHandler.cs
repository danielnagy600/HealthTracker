using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.DeleteFoodEntry;

public sealed class DeleteFoodEntryCommandHandler(IFoodEntryRepository repository, ICurrentUser currentUser)
    : IRequestHandler<DeleteFoodEntryCommand, bool>
{
    public async Task<bool> Handle(DeleteFoodEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        var entry = await repository.FindAsync(userId, request.Id, cancellationToken);
        if (entry is null)
            return false;

        await repository.RemoveAsync(entry, cancellationToken);
        return true;
    }
}
