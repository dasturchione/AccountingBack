using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.FaReceipts;

public class FaReceiptBaseDtoValidator : AbstractValidator<FaReceiptBaseDto>
{
    public FaReceiptBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.ReceiptTypeId)
            .Must(id => id is FaReceiptTypeIdConst.PURCHASE or FaReceiptTypeIdConst.CONSTRUCTION or FaReceiptTypeIdConst.OTHER);
        RuleFor(x => x.SupplierAccountId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new FaReceiptLineWriteDtoValidator());
    }
}

public class FaReceiptLineWriteDtoValidator : AbstractValidator<FaReceiptLineWriteDto>
{
    public FaReceiptLineWriteDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CapitalInvestmentAccountId).GreaterThan(0);
        RuleFor(x => x.VatAccountId).GreaterThan(0).When(x => x.VatAccountId.HasValue);
        RuleFor(x => x.Assets).NotEmpty();
        RuleForEach(x => x.Assets).SetValidator(new FaReceiptAssetWriteDtoValidator());
    }
}

public class FaReceiptAssetWriteDtoValidator : AbstractValidator<FaReceiptAssetWriteDto>
{
    public FaReceiptAssetWriteDtoValidator()
    {
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.InitialCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FaGroupId).GreaterThan(0);
        RuleFor(x => x.AssetAccountId).GreaterThan(0);
    }
}
