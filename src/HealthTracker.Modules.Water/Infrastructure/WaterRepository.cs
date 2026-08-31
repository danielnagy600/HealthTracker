using HealthTracker.Modules.Water.Application;
using HealthTracker.Modules.Water.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Water.Infrastructure;

public sealed class WaterRepository : IWaterRepository
{
    private readonly WaterDbContext _db;

    public WaterRepository(WaterDbContext db) => _db = db;

    public async Task AddIntakeAsync(WaterIntake intake, CancellationToken ct = default)
    {
        _db.Intakes.Add(intake);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WaterIntake>> GetIntakesForDateAsync(
        Guid userId, DateOnly onDate, CancellationToken ct = default)
    {
        return await _db.Intakes
            .Where(i => i.UserId == userId && i.Date == onDate)
            .ToListAsync(ct);
    }

    public async Task<WaterSettings> GetOrCreateSettingsAsync(Guid userId, CancellationToken ct = default)
    {
        var settings = await _db.Settings.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (settings is null)
        {
            settings = WaterSettings.CreateDefault(userId);
            _db.Settings.Add(settings);
            await _db.SaveChangesAsync(ct);
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(WaterSettings settings, CancellationToken ct = default)
    {
        _db.Settings.Update(settings);
        await _db.SaveChangesAsync(ct);
    }
}
