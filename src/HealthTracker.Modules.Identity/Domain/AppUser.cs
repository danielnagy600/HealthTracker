using Microsoft.AspNetCore.Identity;

namespace HealthTracker.Modules.Identity.Domain;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
