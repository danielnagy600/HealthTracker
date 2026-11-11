using FluentValidation.Results;

namespace HealthTracker.Application.Exceptions;

// A use case bemenete érvénytelen (egy XxxCommandValidator elutasította).
// Az Api ebből 400-as ProblemDetails választ csinál (ValidationExceptionHandler),
// így az Api-nak nem kell ismernie a FluentValidationt, csak ezt az osztályt.
public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(ValidationResult result)
        : base(string.Join(" ", result.Errors.Select(e => e.ErrorMessage)))
    {
        Errors = result.Errors.Select(e => e.ErrorMessage).ToList();
    }
}
