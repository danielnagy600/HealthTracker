using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Water.Models;
using MediatR;

namespace HealthTracker.Application.Features.Water.Queries.GetIntake;

public sealed class GetIntakeQueryHandler(IWaterRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetIntakeQuery, IntakeItem?>
{
    public async Task<IntakeItem?> Handle(GetIntakeQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        var intake = await repository.FindIntakeAsync(userId, request.Id, cancellationToken);
        return intake is null ? null : mapper.Map<IntakeItem>(intake);
    }
}
