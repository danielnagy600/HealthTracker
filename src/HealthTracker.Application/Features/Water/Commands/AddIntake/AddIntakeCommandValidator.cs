using FluentValidation;

namespace HealthTracker.Application.Features.Water.Commands.AddIntake;

public sealed class AddIntakeCommandValidator : AbstractValidator<AddIntakeCommand>
{
    public AddIntakeCommandValidator()
    {
        RuleFor(x => x.AmountMl)
            .GreaterThan(0).WithMessage("AmountMl must be positive.");
    }
}
