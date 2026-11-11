using FluentValidation;
using HealthTracker.Domain.Entities;
using HealthTracker.Domain.Enums;

namespace HealthTracker.Application.Features.Schedule.Commands.AddActivity;

public sealed class AddActivityCommandValidator : AbstractValidator<AddActivityCommand>
{
    public AddActivityCommandValidator()
    {
        // Egy mezőn belül az első hibánál álljon meg (pl. üres címnél már ne nézze a hosszát).
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Color)
            .Must(ActivityColorParser.IsKnown)
            .WithMessage(x =>
                $"Unknown color: '{x.Color}'. Available: {string.Join(", ", Enum.GetNames<ActivityColor>())}.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .Must(title => title.Trim().Length <= Activity.MaxTitleLength)
            .WithMessage($"Title can be at most {Activity.MaxTitleLength} characters.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("The end must be after the start.");

        RuleFor(x => x.Note)
            .Must(note => note is null || note.Trim().Length <= Activity.MaxNoteLength)
            .WithMessage($"The note can be at most {Activity.MaxNoteLength} characters.");
    }
}
