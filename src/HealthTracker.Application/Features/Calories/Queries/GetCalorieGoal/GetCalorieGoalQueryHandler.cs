using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Queries.GetCalorieGoal;

public sealed class GetCalorieGoalQueryHandler(
    IFoodEntryRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetCalorieGoalQuery, GoalResponse>
{
    public async Task<GoalResponse> Handle(GetCalorieGoalQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var goal = await repository.GetOrCreateGoalAsync(userId, cancellationToken);
        return mapper.Map<GoalResponse>(goal);
    }
}
