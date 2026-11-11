using Microsoft.AspNetCore.Identity;

namespace HealthTracker.Persistence.Identity;

// Szándékosan nem a Domainben van: az IdentityUser az ASP.NET Identity keretrendszer
// típusa (jelszó-hash, security stamp ...), nem üzleti fogalom. A Domain entitásai
// csak a felhasználó azonosítóját (Guid UserId) ismerik, magát a felhasználót nem.
public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
