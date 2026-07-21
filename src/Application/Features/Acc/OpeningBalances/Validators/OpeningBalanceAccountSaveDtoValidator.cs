using FluentValidation;

namespace Application.Features.Acc.OpeningBalances;

public class OpeningBalanceAccountSaveDtoValidator : AbstractValidator<OpeningBalanceAccountSaveDto>
{
    public OpeningBalanceAccountSaveDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
        RuleFor(x => x.ChartAccountId).GreaterThan(0);
        RuleFor(x => x.Details).NotEmpty();
        RuleFor(x => x.Details)
            .Must(details => details is not null &&
                             details.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).Distinct().Count() ==
                             details.Count(x => x.Id.HasValue))
            .WithMessage("Detail ids must be unique.");
        RuleForEach(x => x.Details).SetValidator(new OpeningBalanceAccountDetailSaveDtoValidator());
    }
}

public class OpeningBalanceAccountDetailSaveDtoValidator : AbstractValidator<OpeningBalanceAccountDetailSaveDto>
{
    public OpeningBalanceAccountDetailSaveDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
        RuleFor(x => x.DebitAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CreditAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).When(x => x.Quantity.HasValue);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.CurrencyAmount).GreaterThanOrEqualTo(0).When(x => x.CurrencyAmount.HasValue);
        RuleFor(x => x.ExchangeRate).GreaterThanOrEqualTo(0).When(x => x.ExchangeRate.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Subkontos).NotNull();
        RuleFor(x => x.Subkontos)
            .Must(subkontos => subkontos is not null &&
                                subkontos.Select(x => x.SubkontoTypeId).Distinct().Count() == subkontos.Count)
            .WithMessage("SubkontoTypeId must be unique.");
        RuleForEach(x => x.Subkontos).SetValidator(new OpeningBalanceAccountDetailSubkontoSaveDtoValidator());
    }
}

public class OpeningBalanceAccountDetailSubkontoSaveDtoValidator : AbstractValidator<OpeningBalanceAccountDetailSubkontoSaveDto>
{
    public OpeningBalanceAccountDetailSubkontoSaveDtoValidator()
    {
        RuleFor(x => x.SubkontoTypeId).GreaterThan((short)0);
        RuleFor(x => x.SubkontoId).GreaterThan(0);
    }
}
