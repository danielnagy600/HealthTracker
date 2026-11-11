using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Commands.AddIntake;

public record AddIntakeCommand(int AmountMl) : IRequest<IntakeItem>;
