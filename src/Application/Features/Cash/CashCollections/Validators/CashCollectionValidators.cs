using FluentValidation;

namespace Application.Features.CashCollections;

public sealed class CashCollectionBaseDtoValidator : AbstractValidator<CashCollectionBaseDto>
{
    public CashCollectionBaseDtoValidator()
    {
        RuleFor(x => x.CashBoxId).GreaterThan(0);
        RuleFor(x => x.BankAccountId).GreaterThan(0);
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.ExchangeRate).GreaterThan(0m);
        RuleFor(x => x.CashChartAccountId).GreaterThan(0).When(x => x.CashChartAccountId.HasValue);
        RuleFor(x => x.CashInTransitAccountId).GreaterThan(0).When(x => x.CashInTransitAccountId.HasValue);
        RuleFor(x => x.BankChartAccountId).GreaterThan(0).When(x => x.BankChartAccountId.HasValue);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public sealed class CashCollectionCreateDtoValidator : AbstractValidator<CashCollectionCreateDto>
{
    public CashCollectionCreateDtoValidator() => Include(new CashCollectionBaseDtoValidator());
}

public sealed class CashCollectionUpdateDtoValidator : AbstractValidator<CashCollectionUpdateDto>
{
    public CashCollectionUpdateDtoValidator() => Include(new CashCollectionBaseDtoValidator());
}
