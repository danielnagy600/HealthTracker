namespace HealthTracker.Modules.Schedule.Application;

// Ezek a DTO-k a modul "külső szerződése": ezt kapja/küldi a HTTP-réteg.
// Szándékosan elválik a Domain entitásoktól, hogy a belső modell szabadon
// változhasson anélkül, hogy az API elromlana.

/// <summary>Egy elfoglaltság a napi listában. A szín stringként megy (pl. "Blue").</summary>
public record ActivityItem(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note,
    int DurationMinutes);

/// <summary>Elfoglaltság létrehozása vagy módosítása – ugyanaz a mezőkészlet.</summary>
public record SaveActivityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note);

/// <summary>Egy szabad idősáv a napban.</summary>
public record TimeSlot(TimeOnly Start, TimeOnly End, int DurationMinutes);

/// <summary>Két elfoglaltság ütközése – a felület ezt figyelmeztetésként mutatja.</summary>
public record ConflictItem(
    Guid FirstId,
    Guid SecondId,
    string FirstTitle,
    string SecondTitle,
    TimeOnly OverlapStart,
    TimeOnly OverlapEnd,
    int OverlapMinutes);

/// <summary>Egy nap teljes képe: az elfoglaltságok és a belőlük számolt összesítés.</summary>
public record DayScheduleResponse(
    DateOnly Date,
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<ActivityItem> Activities,
    IReadOnlyList<TimeSlot> FreeSlots,
    IReadOnlyList<ConflictItem> Conflicts);
