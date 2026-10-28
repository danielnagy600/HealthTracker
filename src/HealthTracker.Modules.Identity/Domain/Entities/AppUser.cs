using Microsoft.AspNetCore.Identity;

namespace HealthTracker.Modules.Identity.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
