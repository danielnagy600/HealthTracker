using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.UpdateCalorieGoal;

public sealed class UpdateCalorieGoalCommandHandler(
    IFoodEntryRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<UpdateCalorieGoalCommand, GoalResponse>
{
    public async Task<GoalResponse> Handle(UpdateCalorieGoalCommand request, CancellationToken cancellationToken)
    {
        var validation = await new UpdateCalorieGoalCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation);

        var userId = currentUser.RequireUserId();
        var goal = await repository.GetOrCreateGoalAsync(userId, cancellationToken);
        goal.DailyTargetKcal = request.DailyTargetKcal;

        await repository.UpdateGoalAsync(goal, cancellationToken);
        return mapper.Map<GoalResponse>(goal);
    }
}
