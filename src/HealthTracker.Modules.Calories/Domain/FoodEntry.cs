namespace HealthTracker.Modules.Calories.Domain;

public class FoodEntry
{
    public const int MaxNameLength = 120;
    public const int MaxCalories = 10000;

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateOnly Date { get; private set; }

    public MealType Meal { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Calories { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

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
