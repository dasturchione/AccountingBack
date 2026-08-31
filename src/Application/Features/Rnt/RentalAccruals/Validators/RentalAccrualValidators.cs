using FluentValidation;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualItemAccountDtoValidator : AbstractValidator<RentalAccrualItemAccountDto>
{
    public RentalAccrualItemAccountDtoValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ExpenseAccountId).GreaterThan(0);
    }
}

public sealed class RentalAccrualUpdateDtoValidator : AbstractValidator<RentalAccrualUpdateDto>
{
    public RentalAccrualUpdateDtoValidator()
    {
        RuleFor(x => x.ExchangeRate).GreaterThan(0m);
        RuleFor(x => x.LessorPayableAccountId).GreaterThan(0);
        RuleFor(x => x.TaxPayableAccountId).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new RentalAccrualItemAccountDtoValidator());
        RuleFor(x => x.Items.Select(i => i.ItemId)).Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Accrual item ids must be unique.");
    }
}
