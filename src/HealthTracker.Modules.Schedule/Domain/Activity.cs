namespace HealthTracker.Modules.Schedule.Domain;

/// <summary>
/// Egy napi elfoglaltság: mettől meddig tart, mi a címe, milyen színnel jelenik
/// meg, és milyen megjegyzés tartozik hozzá.
///
/// Domain entitás: a saját invariánsait maga őrzi (a cím nem lehet üres, a vége
/// nem lehet a kezdete előtt), és nem függ semmilyen technológiától – nincs benne
/// EF Core, adatbázis vagy HTTP.
/// </summary>
public class Activity
{
    public const int MaxTitleLength = 120;
    public const int MaxNoteLength = 500;

    public Guid Id { get; private set; }

    /// <summary>A felhasználó, akihez az elfoglaltság tartozik.</summary>
    public Guid UserId { get; private set; }

    /// <summary>A nap, amelyre az elfoglaltság esik (helyi dátum). Erre indexelünk.</summary>
    public DateOnly Date { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public ActivityColor Color { get; private set; }

    /// <summary>Szabad szöveges megjegyzés, elhagyható.</summary>
    public string? Note { get; private set; }

    /// <summary>Az elfoglaltság hossza percben.</summary>
    public int DurationMinutes => (int)(EndTime.ToTimeSpan() - StartTime.ToTimeSpan()).TotalMinutes;

    // Az EF Core-nak kell egy paraméter nélküli konstruktor (private is lehet).
    private Activity() { }

    public Activity(
        Guid id,
        Guid userId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        string title,
        ActivityColor color,
        string? note = null)
    {
        Id = id;
        UserId = userId;
        Apply(date, startTime, endTime, title, color, note);
    }

    /// <summary>Meglévő elfoglaltság módosítása – ugyanazokkal az invariánsokkal.</summary>
    public void Update(
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        string title,
        ActivityColor color,
        string? note)
    {
        Apply(date, startTime, endTime, title, color, note);
    }

    private void Apply(
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        string title,
        ActivityColor color,
        string? note)
    {
        EnsureValid(startTime, endTime, title, note);

        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Title = title.Trim();
        Color = color;
        // Az üres megjegyzést null-ként tároljuk, hogy ne legyen kétféle "nincs megjegyzés".
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    /// <summary>
    /// Az invariánsok egy helyen. A HTTP-réteg is ezt hívja, hogy 400-as választ
    /// adhasson kivétel helyett – így a szabály nem duplikálódik.
    /// </summary>
    public static string? Validate(TimeOnly startTime, TimeOnly endTime, string title, string? note)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "A cím nem lehet üres.";

        if (title.Trim().Length > MaxTitleLength)
            return $"A cím legfeljebb {MaxTitleLength} karakter lehet.";

        if (endTime <= startTime)
            return "A befejezésnek a kezdés után kell lennie.";

        if (note is not null && note.Trim().Length > MaxNoteLength)
            return $"A megjegyzés legfeljebb {MaxNoteLength} karakter lehet.";

        return null;
    }

    /// <summary>Ugyanaz a szabálykészlet, de kivétellel – ezt hívja az entitás maga.</summary>
    private static void EnsureValid(TimeOnly startTime, TimeOnly endTime, string title, string? note)
    {
        var error = Validate(startTime, endTime, title, note);
        if (error is not null)
            throw new ArgumentException(error);
    }
}
