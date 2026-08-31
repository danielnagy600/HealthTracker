namespace HealthTracker.SharedKernel.Abstractions;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
