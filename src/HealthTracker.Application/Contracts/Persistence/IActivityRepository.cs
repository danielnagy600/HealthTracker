using HealthTracker.Domain.Entities;

namespace HealthTracker.Application.Contracts.Persistence;

public interface IActivityRepository
{
    Task<IReadOnlyList<Activity>> GetForDateAsync(Guid userId, DateOnly onDate, CancellationToken ct = default);
    Task<Activity?> FindAsync(Guid userId, Guid activityId, CancellationToken ct = default);
    Task AddAsync(Activity activity, CancellationToken ct = default);
    Task UpdateAsync(Activity activity, CancellationToken ct = default);
    Task RemoveAsync(Activity activity, CancellationToken ct = default);
}
