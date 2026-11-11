using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Persistence.Repositories;

public sealed class WaterRepository : IWaterRepository
{
    private readonly HealthTrackerDbContext _db;

    public WaterRepository(HealthTrackerDbContext db) => _db = db;

    public async Task<WaterIntake?> FindIntakeAsync(Guid userId, Guid intakeId, CancellationToken ct = default)
    {
        return await _db.WaterIntakes
            .FirstOrDefaultAsync(i => i.Id == intakeId && i.UserId == userId, ct);
    }

    public async Task AddIntakeAsync(WaterIntake intake, CancellationToken ct = default)
    {
        _db.WaterIntakes.Add(intake);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveIntakeAsync(WaterIntake intake, CancellationToken ct = default)
    {
        _db.WaterIntakes.Remove(intake);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WaterIntake>> GetIntakesForDateAsync(
        Guid userId, DateOnly onDate, CancellationToken ct = default)
    {
        return await _db.WaterIntakes
            .Where(i => i.UserId == userId && i.Date == onDate)
            .ToListAsync(ct);
    }

    public async Task<WaterSettings> GetOrCreateSettingsAsync(Guid userId, CancellationToken ct = default)
    {
        var settings = await _db.WaterSettings.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (settings is null)
        {
            settings = WaterSettings.CreateDefault(userId);
            _db.WaterSettings.Add(settings);
            await _db.SaveChangesAsync(ct);
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(WaterSettings settings, CancellationToken ct = default)
    {
        _db.WaterSettings.Update(settings);
        await _db.SaveChangesAsync(ct);
    }
}
