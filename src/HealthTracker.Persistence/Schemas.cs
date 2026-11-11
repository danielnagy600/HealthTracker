namespace HealthTracker.Persistence;

// PostgreSQL sémák funkciónként - így az adatbázisban is látszik, melyik tábla mihez tartozik.
public static class Schemas
{
    public const string Identity = "identity";
    public const string Water = "water";
    public const string Calories = "calories";
    public const string Schedule = "schedule";
}
