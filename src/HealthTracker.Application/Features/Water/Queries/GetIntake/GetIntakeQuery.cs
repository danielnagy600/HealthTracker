using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetIntake;

/// <summary>Null, ha nincs ilyen bejegyzés (vagy nem a felhasználóé).</summary>
public record GetIntakeQuery(Guid Id) : IRequest<IntakeItem?>;
