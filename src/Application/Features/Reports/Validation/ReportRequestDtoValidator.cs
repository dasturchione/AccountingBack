using Application.Features.Reports.DTOs;
using FluentValidation;

namespace Application.Features.Reports.Validation;

/// <summary>
/// Validates the generic report request.
/// </summary>
public class ReportRequestDtoValidator : AbstractValidator<ReportRequestDto>
{
    public ReportRequestDtoValidator()
    {
        RuleFor(x => x.Filter).SetValidator(new ReportFilterDtoValidator());
        RuleFor(x => x.Pagination).SetValidator(new PaginationDtoValidator());
        RuleFor(x => x.Sort!)
            .SetValidator(new SortDtoValidator())
            .When(x => x.Sort is not null);
    }
}
