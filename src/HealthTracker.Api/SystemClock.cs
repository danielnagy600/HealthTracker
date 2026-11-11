using HealthTracker.Application.Contracts;

namespace HealthTracker.Api;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
