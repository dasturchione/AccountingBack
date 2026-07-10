using FluentValidation;

namespace Application.Features.ChartAccounts;

public class ChartAccountBaseDtoValidator : AbstractValidator<ChartAccountBaseDto>
{
    public ChartAccountBaseDtoValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Subkontos)
            .Must(subkontos => subkontos == null ||
                               subkontos.Select(x => x.SubkontoTypeId).Distinct().Count() == subkontos.Count)
            .WithMessage("SubkontoTypeId must be unique.");

        RuleForEach(x => x.Subkontos)
            .ChildRules(subkonto =>
            {
                subkonto.RuleFor(x => x.SubkontoTypeId).GreaterThan((short)0);
                subkonto.RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
            });
    }
}
