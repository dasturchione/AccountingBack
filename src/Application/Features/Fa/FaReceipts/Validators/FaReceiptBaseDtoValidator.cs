using FluentValidation;

namespace Application.Features.FaReceipts;

public class FaReceiptBaseDtoValidator : AbstractValidator<FaReceiptBaseDto>
{
    public FaReceiptBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.ReceiptTypeId).GreaterThan((short)0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new FaReceiptLineWriteDtoValidator());
    }
}

public class FaReceiptLineWriteDtoValidator : AbstractValidator<FaReceiptLineWriteDto>
{
    public FaReceiptLineWriteDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Assets).SetValidator(new FaReceiptAssetWriteDtoValidator());
    }
}

public class FaReceiptAssetWriteDtoValidator : AbstractValidator<FaReceiptAssetWriteDto>
{
    public FaReceiptAssetWriteDtoValidator()
    {
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InitialCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalvageValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalvageValue).LessThanOrEqualTo(x => x.InitialCost);
        RuleFor(x => x.UsefulLifeMonths).GreaterThan(0);
        RuleFor(x => x.DepreciationMethodId).GreaterThan((short)0);
        RuleFor(x => x.FaGroupId).GreaterThan(0);
        RuleFor(x => x.PlannedUnitsTotal).GreaterThan(0).When(x => x.PlannedUnitsTotal.HasValue);
        RuleFor(x => x.DeprStartDate)
            .GreaterThanOrEqualTo(x => x.CommissioningDate!.Value)
            .When(x => x.CommissioningDate.HasValue && x.DeprStartDate.HasValue);
    }
}
