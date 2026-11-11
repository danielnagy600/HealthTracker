using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Calories.Models;
using HealthTracker.Domain.Entities;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.AddFoodEntry;

public sealed class AddFoodEntryCommandHandler(
    IFoodEntryRepository repository, IClock clock, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<AddFoodEntryCommand, FoodEntryItem>
{
    public async Task<FoodEntryItem> Handle(AddFoodEntryCommand request, CancellationToken cancellationToken)
    {
        var validation = await new AddFoodEntryCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation);

        var userId = currentUser.RequireUserId();

        var entry = new FoodEntry(
            Guid.NewGuid(), userId, request.Date, MealTypeParser.Parse(request.Meal),
            request.Name, request.Calories, clock.Now.ToUniversalTime());

        await repository.AddAsync(entry, cancellationToken);
        return mapper.Map<FoodEntryItem>(entry);
    }
}
