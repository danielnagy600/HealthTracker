using HealthTracker.Domain.Entities;

namespace HealthTracker.Application.Contracts.Persistence;

public interface IFoodEntryRepository
{
    Task<IReadOnlyList<FoodEntry>> GetForDateAsync(Guid userId, DateOnly onDate, CancellationToken ct = default);

    Task<FoodEntry?> FindAsync(Guid userId, Guid entryId, CancellationToken ct = default);

    Task AddAsync(FoodEntry entry, CancellationToken ct = default);

    Task UpdateAsync(FoodEntry entry, CancellationToken ct = default);

    Task RemoveAsync(FoodEntry entry, CancellationToken ct = default);

    Task<CalorieGoal> GetOrCreateGoalAsync(Guid userId, CancellationToken ct = default);

    Task UpdateGoalAsync(CalorieGoal goal, CancellationToken ct = default);
}
