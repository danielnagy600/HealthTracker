using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Commands.UpdateWaterSettings;

public record UpdateWaterSettingsCommand(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime)
    : IRequest<SettingsResponse>;
