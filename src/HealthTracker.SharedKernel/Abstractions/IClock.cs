namespace HealthTracker.SharedKernel.Abstractions;

public interface IClock
{
    DateTimeOffset Now { get; }
}
