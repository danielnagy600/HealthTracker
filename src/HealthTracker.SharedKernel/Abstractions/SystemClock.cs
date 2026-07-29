namespace HealthTracker.SharedKernel.Abstractions;

/// <summary>
/// Az <see cref="IClock"/> éles implementációja: a valódi rendszeridőt adja vissza.
/// A DI-ban ezt regisztráljuk, a tesztekben viszont egy hamis órát.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
