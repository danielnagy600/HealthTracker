using HealthTracker.Modules.Water.Application.Dtos;

namespace HealthTracker.Modules.Water.Application;

public interface IWaterService
{
    Task<IntakeItem?> GetIntakeAsync(Guid id, CancellationToken ct = default);
    Task<IntakeItem> AddIntakeAsync(AddIntakeRequest request, CancellationToken ct = default);
    Task<bool> DeleteIntakeAsync(Guid id, CancellationToken ct = default);
    Task<DailySummaryResponse> GetDayAsync(DateOnly? onDate = null, CancellationToken ct = default);
    Task<ReminderResponse> GetReminderAsync(CancellationToken ct = default);
    Task<SettingsResponse> GetSettingsAsync(CancellationToken ct = default);
    Task<SettingsResponse> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken ct = default);
}
