namespace HealthTracker.SharedKernel.Abstractions;

/// <summary>
/// A bejelentkezett felhasználó absztrakciója. A funkciómodulok (pl. Water) ezen
/// keresztül tudják meg, kihez tartoznak az adatok – anélkül, hogy ismernék az
/// Identity modult vagy közvetlenül az ASP.NET HttpContextet.
///
/// Az implementáció (a claim-ekből olvas) az Api hostban van. Ez a "Dependency
/// Inversion": a modulok az absztrakciótól függenek, nem a konkrét megvalósítástól.
/// </summary>
public interface ICurrentUser
{
    /// <summary>A bejelentkezett felhasználó azonosítója, vagy null, ha nincs bejelentkezve.</summary>
    Guid? UserId { get; }

    bool IsAuthenticated => UserId is not null;

    /// <summary>A felhasználó azonosítója, vagy kivétel, ha nincs bejelentkezve.</summary>
    Guid RequireUserId() =>
        UserId ?? throw new InvalidOperationException("Nincs bejelentkezett felhasználó.");
}
