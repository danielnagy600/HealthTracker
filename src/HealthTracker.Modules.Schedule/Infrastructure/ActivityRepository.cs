using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Schedule.Infrastructure;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly ScheduleDbContext _db;

    public ActivityRepository(ScheduleDbContext db) => _db = db;

    public async Task<IReadOnlyList<Activity>> GetForDateAsync(
        Guid userId, DateOnly onDate, CancellationToken ct = default)
    {
        return await _db.Activities
            .Where(a => a.UserId == userId && a.Date == onDate)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);
    }

    public async Task<Activity?> FindAsync(Guid userId, Guid activityId, CancellationToken ct = default)
    {
        return await _db.Activities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.UserId == userId, ct);
    }

    public async Task AddAsync(Activity activity, CancellationToken ct = default)
    {
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Activity activity, CancellationToken ct = default)
    {
        _db.Activities.Update(activity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Activity activity, CancellationToken ct = default)
    {
        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync(ct);
    }
}
