using HealthTracker.Application.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace HealthTracker.Application;

public static class ApplicationServiceRegistration
{
    /// <param name="luckyPennyLicenseKey">
    /// Az AutoMapper 15+ és a MediatR 13+ licenckulcsa (a kettőhöz ugyanaz a kulcs tartozik,
    /// luckypennysoftware.com). Nélküle is működnek, csak induláskor figyelmeztetést logolnak.
    /// </param>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, string? luckyPennyLicenseKey = null)
    {
        services.AddAutoMapper(
            cfg => cfg.LicenseKey = luckyPennyLicenseKey,
            typeof(MappingProfile).Assembly);

        // Megkeresi az összes IRequestHandler implementációt ebben a projektben,
        // és regisztrálja őket - új handlerhez nem kell ide semmit felvenni.
        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = luckyPennyLicenseKey;
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly);
        });

        return services;
    }
}
