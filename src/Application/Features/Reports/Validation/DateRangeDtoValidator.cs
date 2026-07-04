using Application.Features.Reports.DTOs;
using FluentValidation;

namespace Application.Features.Reports.Validation;

/// <summary>
/// Validates date ranges used by reports.
/// </summary>
public class DateRangeDtoValidator : AbstractValidator<DateRangeDto>
{
    public DateRangeDtoValidator()
    {
        RuleFor(x => x.From).LessThanOrEqualTo(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue);
    }
}
