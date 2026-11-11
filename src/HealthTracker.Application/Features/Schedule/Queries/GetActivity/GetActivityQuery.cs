using HealthTracker.Application.Features.Schedule.Models;
using MediatR;

namespace HealthTracker.Application.Features.Schedule.Queries.GetActivity;

/// <summary>Null, ha nincs ilyen tevékenység (vagy nem a felhasználóé).</summary>
public record GetActivityQuery(Guid Id) : IRequest<ActivityItem?>;
