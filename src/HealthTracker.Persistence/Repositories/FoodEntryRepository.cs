using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Persistence.Repositories;

public sealed class FoodEntryRepository : IFoodEntryRepository
{
    private readonly HealthTrackerDbContext _db;

    public FoodEntryRepository(HealthTrackerDbContext db) => _db = db;

    public async Task<IReadOnlyList<FoodEntry>> GetForDateAsync(
        Guid userId, DateOnly onDate, CancellationToken ct = default)
    {
        return await _db.FoodEntries
            .Where(e => e.UserId == userId && e.Date == onDate)
            .OrderBy(e => e.RecordedAt)
            .ToListAsync(ct);
    }

    public async Task<FoodEntry?> FindAsync(Guid userId, Guid entryId, CancellationToken ct = default)
    {
        return await _db.FoodEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId, ct);
    }

    public async Task AddAsync(FoodEntry entry, CancellationToken ct = default)
    {
        _db.FoodEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(FoodEntry entry, CancellationToken ct = default)
    {
        _db.FoodEntries.Update(entry);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(FoodEntry entry, CancellationToken ct = default)
    {
        _db.FoodEntries.Remove(entry);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<CalorieGoal> GetOrCreateGoalAsync(Guid userId, CancellationToken ct = default)
    {
        var goal = await _db.CalorieGoals.FirstOrDefaultAsync(g => g.UserId == userId, ct);
        if (goal is null)
        {
            goal = CalorieGoal.CreateDefault(userId);
            _db.CalorieGoals.Add(goal);
            await _db.SaveChangesAsync(ct);
        }
        return goal;
    }

    public async Task UpdateGoalAsync(CalorieGoal goal, CancellationToken ct = default)
    {
        _db.CalorieGoals.Update(goal);
        await _db.SaveChangesAsync(ct);
    }
}
