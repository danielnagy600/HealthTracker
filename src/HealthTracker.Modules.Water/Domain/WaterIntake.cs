namespace HealthTracker.Modules.Water.Domain;

/// <summary>
/// Egy elfogyasztott adag víz. Ez egy Domain entitás: tartalmazza a saját
/// invariánsait (pl. a mennyiség csak pozitív lehet), és nem függ semmilyen
/// technológiától (nincs benne EF Core, adatbázis, HTTP – tiszta üzleti fogalom).
/// </summary>
public class WaterIntake
{
    public Guid Id { get; private set; }

    /// <summary>A felhasználó, akihez a bejegyzés tartozik (profilhoz kötött adat).</summary>
    public Guid UserId { get; private set; }

    /// <summary>A nap, amelyhez a bejegyzés tartozik (helyi dátum). Erre indexelünk.</summary>
    public DateOnly Date { get; private set; }

    /// <summary>A rögzítés pontos időpontja.</summary>
    public DateTimeOffset RecordedAt { get; private set; }

    /// <summary>Az elfogyasztott mennyiség milliliterben.</summary>
    public int AmountMl { get; private set; }

    // Az EF Core-nak kell egy paraméter nélküli konstruktor (private is lehet).
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
