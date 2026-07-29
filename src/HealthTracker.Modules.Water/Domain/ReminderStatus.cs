namespace HealthTracker.Modules.Water.Domain;

/// <summary>Az aktuális hidratáltsági állapot az adott napszakban.</summary>
public enum ReminderStatus
{
    /// <summary>A napi célt már elérted.</summary>
    GoalReached,

    /// <summary>Időarányosan jól állsz.</summary>
    OnTrack,

    /// <summary>Le vagy maradva, pótolni kell.</summary>
    Behind
}
