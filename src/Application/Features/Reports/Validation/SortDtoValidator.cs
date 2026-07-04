using Application.Features.Reports.DTOs;
using FluentValidation;

namespace Application.Features.Reports.Validation;

/// <summary>
/// Validates report sorting settings.
/// </summary>
public class SortDtoValidator : AbstractValidator<SortDto>
{
    public SortDtoValidator()
    {
        RuleFor(x => x.SortBy).MaximumLength(128).When(x => x.SortBy is not null);
    }
}
