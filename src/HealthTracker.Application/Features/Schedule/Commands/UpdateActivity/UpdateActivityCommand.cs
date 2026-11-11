using HealthTracker.Application.Features.Schedule.Models;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Commands.UpdateActivity;

/// <summary>Null, ha nincs ilyen tevékenység (vagy nem a felhasználóé).</summary>
public record UpdateActivityCommand(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note) : IRequest<ActivityItem?>;
