using Application.Features.Reports.DTOs;
using FluentValidation;

namespace Application.Features.Reports.Validation;

/// <summary>
/// Validates pagination settings for report requests.
/// </summary>
public class PaginationDtoValidator : AbstractValidator<PaginationDto>
{
    public PaginationDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).When(x => x.PageSize.HasValue);
    }
}
