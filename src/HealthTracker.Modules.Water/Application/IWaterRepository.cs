using HealthTracker.Modules.Water.Domain.Entities;

namespace HealthTracker.Modules.Water.Application;

public interface IWaterRepository
{
    Task<WaterIntake?> FindIntakeAsync(Guid userId, Guid intakeId, CancellationToken ct = default);
    Task AddIntakeAsync(WaterIntake intake, CancellationToken ct = default);
    Task RemoveIntakeAsync(WaterIntake intake, CancellationToken ct = default);
    Task<IReadOnlyList<WaterIntake>> GetIntakesForDateAsync(Guid userId, DateOnly onDate, CancellationToken ct = default);
    Task<WaterSettings> GetOrCreateSettingsAsync(Guid userId, CancellationToken ct = default);
    Task UpdateSettingsAsync(WaterSettings settings, CancellationToken ct = default);
}
