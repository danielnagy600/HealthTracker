using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetWaterReminder;

public record GetWaterReminderQuery : IRequest<ReminderResponse>;
