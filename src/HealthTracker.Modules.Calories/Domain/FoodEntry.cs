namespace HealthTracker.Modules.Calories.Domain;

/// <summary>
/// Egy elfogyasztott étel vagy ital bejegyzése: mi volt, hány kalória, és melyik
/// étkezéshez tartozik.
///
/// Domain entitás: a saját invariánsait maga őrzi (a név nem lehet üres, a kalória
/// pozitív és értelmes tartományban van), és nem függ semmilyen technológiától.
/// </summary>
public class FoodEntry
{
    public const int MaxNameLength = 120;
    public const int MaxCalories = 10000;

    public Guid Id { get; private set; }

    /// <summary>A felhasználó, akihez a bejegyzés tartozik.</summary>
    public Guid UserId { get; private set; }

    /// <summary>A nap, amelyhez a bejegyzés tartozik (helyi dátum). Erre indexelünk.</summary>
    public DateOnly Date { get; private set; }

    /// <summary>Melyik étkezéshez tartozik.</summary>
    public MealType Meal { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Az energiatartalom kilokalóriában.</summary>
    public int Calories { get; private set; }

    /// <summary>A rögzítés pontos időpontja – ez adja a napon belüli sorrendet.</summary>
    public DateTimeOffset RecordedAt { get; private set; }

    // Az EF Core-nak kell egy paraméter nélküli konstruktor (private is lehet).
    private FoodEntry() { }

    public FoodEntry(
        Guid id,
        Guid userId,
        DateOnly date,
        MealType meal,
        string name,
        int calories,
        DateTimeOffset recordedAt)
    {
        Id = id;
        UserId = userId;
        RecordedAt = recordedAt;
        Apply(date, meal, name, calories);
    }

    /// <summary>Meglévő bejegyzés módosítása – ugyanazokkal az invariánsokkal.</summary>
    public void Update(DateOnly date, MealType meal, string name, int calories)
    {
        Apply(date, meal, name, calories);
    }

    private void Apply(DateOnly date, MealType meal, string name, int calories)
    {
        var error = Validate(name, calories);
        if (error is not null)
            throw new ArgumentException(error);

        Date = date;
        Meal = meal;
        Name = name.Trim();
        Calories = calories;
    }

    /// <summary>
    /// Az invariánsok egy helyen. A HTTP-réteg is ezt hívja, hogy 400-as választ
    /// adhasson kivétel helyett – így a szabály nem duplikálódik.
    /// </summary>
    public static string? Validate(string name, int calories)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "The food name cannot be empty.";

        if (name.Trim().Length > MaxNameLength)
            return $"The food name can be at most {MaxNameLength} characters.";

        if (calories <= 0)
            return "Calories must be positive.";

        if (calories > MaxCalories)
            return $"A single entry can be at most {MaxCalories} kcal.";

        return null;
    }
}
