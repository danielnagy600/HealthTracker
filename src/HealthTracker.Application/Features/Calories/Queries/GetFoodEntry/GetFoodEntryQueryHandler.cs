using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Queries.GetFoodEntry;

public sealed class GetFoodEntryQueryHandler(
    IFoodEntryRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetFoodEntryQuery, FoodEntryItem?>
{
    public async Task<FoodEntryItem?> Handle(GetFoodEntryQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        var entry = await repository.FindAsync(userId, request.Id, cancellationToken);
        return entry is null ? null : mapper.Map<FoodEntryItem>(entry);
    }
}
