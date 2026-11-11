using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetWaterSettings;

public record GetWaterSettingsQuery : IRequest<SettingsResponse>;
