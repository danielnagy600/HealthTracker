using AutoMapper;
using HealthTracker.Application.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace HealthTracker.Application.Tests;

// A handlerek valódi (nem mockolt) AutoMappert kapnak a tesztekben: így a
// tesztek a MappingProfile helyességét is ellenőrzik.
internal static class TestMapper
{
    public static IMapper Create() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance)
            .CreateMapper();
}
