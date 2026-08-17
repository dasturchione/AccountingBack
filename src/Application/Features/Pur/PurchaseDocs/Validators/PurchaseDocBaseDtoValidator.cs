using Application.Features.PurchaseDocs;
using FluentValidation;

namespace Application.Features.Pur.PurchaseDocs
{
    public class PurchaseDocBaseDtoValidator : AbstractValidator<PurchaseDocBaseDto>
    {
        public PurchaseDocBaseDtoValidator()
        {
            RuleFor(x => x.ExternalId).MaximumLength(150).When(x => x.ExternalId != null);
            RuleFor(x => x.ExternalDocNumber).MaximumLength(100).When(x => x.ExternalDocNumber != null);
            RuleFor(x => x.DocDate).NotEmpty();
            RuleFor(x => x.CounterpartyId).GreaterThan(0);
            RuleFor(x => x.WarehouseId).GreaterThan(0);
            RuleFor(x => x.CurrencyId).GreaterThan((short)0);
            RuleFor(x => x.ContractId).GreaterThan(0).When(x => x.ContractId.HasValue);
            RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        }
    }

    public class PurchaseDocCreateDtoValidator : AbstractValidator<PurchaseDocCreateDto>
    {
        public PurchaseDocCreateDtoValidator()
        {
            Include(new PurchaseDocBaseDtoValidator());
            RuleFor(x => x.Lines).NotEmpty();
            RuleForEach(x => x.Lines).SetValidator(new PurchaseDocLineDtoValidator());
        }
    }

    public class PurchaseDocUpdateDtoValidator : AbstractValidator<PurchaseDocUpdateDto>
    {
        public PurchaseDocUpdateDtoValidator()
        {
            Include(new PurchaseDocBaseDtoValidator());
            RuleFor(x => x.Lines).NotEmpty();
            RuleForEach(x => x.Lines).SetValidator(new PurchaseDocLineDtoValidator());
        }
    }

    public class PurchaseDocLineDtoValidator : AbstractValidator<PurchaseDocLineDto>
    {
        public PurchaseDocLineDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.UnitId).GreaterThan((short)0);
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            RuleForEach(x => x.Items).SetValidator(new PurchaseDocLineItemDtoValidator());
        }
    }

    public class PurchaseDocLineItemDtoValidator : AbstractValidator<PurchaseDocLineItemDto>
    {
        public PurchaseDocLineItemDtoValidator()
        {
            RuleFor(x => x.MarkingNumber).NotEmpty().MaximumLength(250);
            RuleFor(x => x.SerialNumber).MaximumLength(250).When(x => x.SerialNumber != null);
        }
    }
}
