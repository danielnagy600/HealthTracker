using Microsoft.AspNetCore.Identity;

namespace HealthTracker.Modules.Identity.Domain;

/// <summary>
/// Az alkalmazás felhasználója. Az ASP.NET Core Identity <see cref="IdentityUser"/>
/// osztályát bővítjük egy megjelenítendő névvel (profil-adat).
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>A felhasználó megjelenítendő neve a profilon.</summary>
    public string? DisplayName { get; set; }
}
