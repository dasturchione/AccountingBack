using FluentValidation;

namespace Application.Features.InventoryCounts;

public class InventoryCountBaseDtoValidator : AbstractValidator<InventoryCountBaseDto>
{
    public InventoryCountBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InventoryCountLineRequestDtoValidator());
    }
}

public class InventoryCountCreateDtoValidator : AbstractValidator<InventoryCountCreateDto>
{
    public InventoryCountCreateDtoValidator()
    {
        Include(new InventoryCountBaseDtoValidator());
    }
}

public class InventoryCountUpdateDtoValidator : AbstractValidator<InventoryCountUpdateDto>
{
    public InventoryCountUpdateDtoValidator()
    {
        Include(new InventoryCountBaseDtoValidator());
    }
}

public class InventoryCountLineRequestDtoValidator : AbstractValidator<InventoryCountLineRequestDto>
{
    public InventoryCountLineRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.CountedQuantity).GreaterThan(0);
        RuleFor(x => x.DefaultCostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleForEach(x => x.Items).SetValidator(new InventoryCountTableRequestDtoValidator());
    }
}

public class InventoryCountTableRequestDtoValidator : AbstractValidator<InventoryCountTableRequestDto>
{
    public InventoryCountTableRequestDtoValidator()
    {
        RuleFor(x => x.ProductTableId).GreaterThan(0).When(x => x.ProductTableId.HasValue);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode != null);
        RuleFor(x => x.SerialNumber).MaximumLength(250).When(x => x.SerialNumber != null);
        RuleFor(x => x.MarkingNumber).MaximumLength(250).When(x => x.MarkingNumber != null);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
    }
}
