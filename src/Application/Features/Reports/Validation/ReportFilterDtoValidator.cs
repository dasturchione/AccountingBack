using Application.Features.Reports.DTOs;
using FluentValidation;

namespace Application.Features.Reports.Validation;

/// <summary>
/// Validates common report filters.
/// </summary>
public class ReportFilterDtoValidator : AbstractValidator<ReportFilterDto>
{
    public ReportFilterDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0).When(x => x.OrganizationId.HasValue);
        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0).When(x => x.CurrencyId.HasValue);
        RuleFor(x => x.DateRange!)
            .SetValidator(new DateRangeDtoValidator())
            .When(x => x.DateRange is not null);
    }
}
