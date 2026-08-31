namespace HealthTracker.Modules.Schedule.Domain;

public class Activity
{
    public const int MaxTitleLength = 120;
    public const int MaxNoteLength = 500;

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateOnly Date { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public ActivityColor Color { get; private set; }

    public string? Note { get; private set; }

    public int DurationMinutes => (int)(EndTime.ToTimeSpan() - StartTime.ToTimeSpan()).TotalMinutes;

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

        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public static string? Validate(TimeOnly startTime, TimeOnly endTime, string title, string? note)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title cannot be empty.";

        if (title.Trim().Length > MaxTitleLength)
            return $"Title can be at most {MaxTitleLength} characters.";

        if (endTime <= startTime)
            return "The end must be after the start.";

        if (note is not null && note.Trim().Length > MaxNoteLength)
            return $"The note can be at most {MaxNoteLength} characters.";

        return null;
    }

    private static void EnsureValid(TimeOnly startTime, TimeOnly endTime, string title, string? note)
    {
        var error = Validate(startTime, endTime, title, note);
        if (error is not null)
            throw new ArgumentException(error);
    }
}
