using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Calories.Infrastructure;

/// <summary>
/// Az IFoodEntryRepository EF Core + PostgreSQL implementációja. Ez az egyetlen hely,
/// ahol az adatbázis-technológia megjelenik – a modul többi része nem tud róla.
/// </summary>
public sealed class FoodEntryRepository : IFoodEntryRepository
{
    private readonly CalorieDbContext _db;

    public FoodEntryRepository(CalorieDbContext db) => _db = db;

    public async Task<IReadOnlyList<FoodEntry>> GetForDateAsync(
        Guid userId, DateOnly date, CancellationToken ct = default)
    {
        return await _db.Entries
            .Where(e => e.UserId == userId && e.Date == date)
            .OrderBy(e => e.RecordedAt)
            .ToListAsync(ct);
    }

    public async Task<FoodEntry?> FindAsync(Guid userId, Guid entryId, CancellationToken ct = default)
    {
        // A userId-ra is szűrünk: így más felhasználó bejegyzését akkor sem lehet
        // elérni, ha valaki kitalálja az azonosítóját.
        return await _db.Entries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId, ct);
    }

    public async Task AddAsync(FoodEntry entry, CancellationToken ct = default)
    {
        _db.Entries.Add(entry);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(FoodEntry entry, CancellationToken ct = default)
    {
        _db.Entries.Update(entry);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(FoodEntry entry, CancellationToken ct = default)
    {
        _db.Entries.Remove(entry);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<CalorieGoal> GetOrCreateGoalAsync(Guid userId, CancellationToken ct = default)
    {
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.UserId == userId, ct);
        if (goal is null)
        {
            goal = CalorieGoal.CreateDefault(userId);
            _db.Goals.Add(goal);
            await _db.SaveChangesAsync(ct);
        }
        return goal;
    }

    public async Task UpdateGoalAsync(CalorieGoal goal, CancellationToken ct = default)
    {
        _db.Goals.Update(goal);
        await _db.SaveChangesAsync(ct);
    }
}
