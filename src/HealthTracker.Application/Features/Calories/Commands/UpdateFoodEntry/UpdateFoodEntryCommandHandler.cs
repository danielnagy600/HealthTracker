using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.UpdateFoodEntry;

public sealed class UpdateFoodEntryCommandHandler(
    IFoodEntryRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<UpdateFoodEntryCommand, FoodEntryItem?>
{
    public async Task<FoodEntryItem?> Handle(UpdateFoodEntryCommand request, CancellationToken cancellationToken)
    {
        var validation = await new UpdateFoodEntryCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation);

        var userId = currentUser.RequireUserId();

        var entry = await repository.FindAsync(userId, request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Date, MealTypeParser.Parse(request.Meal), request.Name, request.Calories);

        await repository.UpdateAsync(entry, cancellationToken);
        return mapper.Map<FoodEntryItem>(entry);
    }
}
