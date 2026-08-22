using FluentValidation;

namespace Application.Features.RetailSaleDocs;

public class RetailSaleDocCreateDtoValidator : AbstractValidator<RetailSaleDocCreateDto>
{
    public RetailSaleDocCreateDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.CashRegisterId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new RetailSaleDocProductCreateDtoValidator());
        RuleForEach(x => x.Payments).SetValidator(new RetailSaleDocPaymentDtoValidator());
    }
}

public class RetailSaleDocUpdateDtoValidator : AbstractValidator<RetailSaleDocUpdateDto>
{
    public RetailSaleDocUpdateDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.CashRegisterId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.StateId).GreaterThan((short)0);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new RetailSaleDocProductCreateDtoValidator());
        RuleForEach(x => x.Payments).SetValidator(new RetailSaleDocPaymentDtoValidator());
    }
}

public class RetailSaleDocProductCreateDtoValidator : AbstractValidator<RetailSaleDocProductCreateDto>
{
    public RetailSaleDocProductCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VatAmount).GreaterThanOrEqualTo(0).When(x => x.VatAmount.HasValue);
    }
}

public class RetailSaleDocPaymentDtoValidator : AbstractValidator<RetailSaleDocPaymentDto>
{
    public RetailSaleDocPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentMethodId).GreaterThan((short)0);
        RuleFor(x => x.DebitAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.TransactionNumber).MaximumLength(100);
    }
}
