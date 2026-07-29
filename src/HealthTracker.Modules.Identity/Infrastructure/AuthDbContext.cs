using HealthTracker.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Identity.Infrastructure;

/// <summary>
/// Az Identity modul adatbázis-kontextusa. Az Identity kész tábláit (felhasználók,
/// szerepkörök, tokenek) hozza létre a saját "identity" PostgreSQL sémában.
/// </summary>
public sealed class AuthDbContext : IdentityDbContext<AppUser>
{
    public const string Schema = "identity";

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);
    }
}
