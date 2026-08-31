namespace HealthTracker.Modules.Water.Domain;

public class WaterIntake
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateOnly Date { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

    public int AmountMl { get; private set; }

    private WaterIntake() { }

    public WaterIntake(Guid id, Guid userId, DateOnly date, DateTimeOffset recordedAt, int amountMl)
    {
        if (amountMl <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountMl), "A mennyiségnek pozitívnak kell lennie.");

        Id = id;
        UserId = userId;
        Date = date;
        RecordedAt = recordedAt;
        AmountMl = amountMl;
    }
}
