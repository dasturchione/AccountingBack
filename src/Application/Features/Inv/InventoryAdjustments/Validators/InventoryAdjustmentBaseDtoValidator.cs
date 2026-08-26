using FluentValidation;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentBaseDtoValidator : AbstractValidator<InventoryAdjustmentBaseDto>
{
    public InventoryAdjustmentBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.AdjustmentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InventoryAdjustmentLineRequestDtoValidator());
    }
}

public class InventoryAdjustmentCreateDtoValidator : AbstractValidator<InventoryAdjustmentCreateDto>
{
    public InventoryAdjustmentCreateDtoValidator()
    {
        Include(new InventoryAdjustmentBaseDtoValidator());
    }
}

public class InventoryAdjustmentUpdateDtoValidator : AbstractValidator<InventoryAdjustmentUpdateDto>
{
    public InventoryAdjustmentUpdateDtoValidator()
    {
        Include(new InventoryAdjustmentBaseDtoValidator());
    }
}

public class InventoryAdjustmentLineRequestDtoValidator : AbstractValidator<InventoryAdjustmentLineRequestDto>
{
    public InventoryAdjustmentLineRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleForEach(x => x.Items).SetValidator(new InventoryAdjustmentTableRequestDtoValidator());
    }
}

public class InventoryAdjustmentTableRequestDtoValidator : AbstractValidator<InventoryAdjustmentTableRequestDto>
{
    public InventoryAdjustmentTableRequestDtoValidator()
    {
        RuleFor(x => x.ProductTableId).GreaterThan(0).When(x => x.ProductTableId.HasValue);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
    }
}
