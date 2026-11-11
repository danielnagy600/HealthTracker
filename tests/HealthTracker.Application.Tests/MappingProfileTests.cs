using AutoMapper;
using HealthTracker.Application.Profiles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthTracker.Application.Tests;

public class MappingProfileTests
{
    [Fact]
    public void Every_mapping_is_complete()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);

        // Elbukik, ha egy DTO-tulajdonságnak nincs párja az entitásban
        // (pl. átneveztek egy mezőt, és a leképezés csendben üres maradna).
        config.AssertConfigurationIsValid();
    }
}
