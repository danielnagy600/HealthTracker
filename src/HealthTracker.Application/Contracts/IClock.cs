namespace HealthTracker.Application.Contracts;

public interface IClock
{
    DateTimeOffset Now { get; }
}
