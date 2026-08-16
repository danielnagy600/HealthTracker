namespace HealthTracker.Modules.Schedule.Domain;

/// <summary>
/// A választható színek rögzített palettája. Szándékosan nem szabad hex-kód:
/// így a felület garantáltan olvasható marad (kontraszt), és a paletta egy
/// helyen bővíthető. A tárolásban is ez az enum megy, nem a konkrét szín.
/// </summary>
public enum ActivityColor
{
    Blue,
    Green,
    Amber,
    Red,
    Purple,
    Teal
}
