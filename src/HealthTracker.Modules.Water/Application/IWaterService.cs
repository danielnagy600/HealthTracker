namespace HealthTracker.Modules.Water.Application;

public interface IWaterService
{
    Task<IntakeItem> AddIntakeAsync(AddIntakeRequest request, CancellationToken ct = default);
    Task<DailySummaryResponse> GetTodaySummaryAsync(CancellationToken ct = default);
    Task<ReminderResponse> GetReminderAsync(CancellationToken ct = default);
    Task<SettingsResponse> GetSettingsAsync(CancellationToken ct = default);
    Task<SettingsResponse> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken ct = default);
}
