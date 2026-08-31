namespace HealthTracker.Modules.Schedule.Application;

public interface IScheduleService
{
    Task<DayScheduleResponse> GetDayAsync(DateOnly? onDate = null, CancellationToken ct = default);

    Task<ActivityItem> AddAsync(SaveActivityRequest request, CancellationToken ct = default);

    Task<ActivityItem?> UpdateAsync(Guid id, SaveActivityRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
