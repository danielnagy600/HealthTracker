using AutoMapper;
using HealthTracker.Application.Features.Calories.Models;
using HealthTracker.Application.Features.Schedule.Models;
using HealthTracker.Application.Features.Water.Models;
using HealthTracker.Domain.Entities;

namespace HealthTracker.Application.Profiles;

// Entitás → DTO leképezések. Az AutoMapper név szerint párosítja a tulajdonságokat
// (rekordoknál a konstruktor-paramétereket), az enumokat pedig magától stringgé alakítja.
// Csak az egy-az-egyben megfeleltethető típusok vannak itt; az összesítő válaszokat
// (pl. DailySummaryResponse) a handlerek rakják össze, mert azokban számolás is van.
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WaterIntake, IntakeItem>();
        CreateMap<WaterSettings, SettingsResponse>();

        CreateMap<FoodEntry, FoodEntryItem>();
        CreateMap<CalorieGoal, GoalResponse>();

        CreateMap<Activity, ActivityItem>();
    }
}
