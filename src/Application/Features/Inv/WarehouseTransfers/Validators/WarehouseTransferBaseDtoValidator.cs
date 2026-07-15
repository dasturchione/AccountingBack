using FluentValidation;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferBaseDtoValidator : AbstractValidator<WarehouseTransferBaseDto>
{
    public WarehouseTransferBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.SourceWarehouseId).GreaterThan(0);
        RuleFor(x => x.DestinationWarehouseId).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleFor(x => x.Lines).NotEmpty();
        RuleFor(x => x.SourceWarehouseId)
            .NotEqual(x => x.DestinationWarehouseId)
            .WithMessage("Source and destination warehouses must be different.");
        RuleForEach(x => x.Lines).SetValidator(new WarehouseTransferLineRequestDtoValidator());
    }
}

public class WarehouseTransferCreateDtoValidator : AbstractValidator<WarehouseTransferCreateDto>
{
    public WarehouseTransferCreateDtoValidator()
    {
        Include(new WarehouseTransferBaseDtoValidator());
    }
}

public class WarehouseTransferUpdateDtoValidator : AbstractValidator<WarehouseTransferUpdateDto>
{
    public WarehouseTransferUpdateDtoValidator()
    {
        Include(new WarehouseTransferBaseDtoValidator());
    }
}

public class WarehouseTransferLineRequestDtoValidator : AbstractValidator<WarehouseTransferLineRequestDto>
{
    public WarehouseTransferLineRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleForEach(x => x.Items).SetValidator(new WarehouseTransferTableRequestDtoValidator());
    }
}

public class WarehouseTransferTableRequestDtoValidator : AbstractValidator<WarehouseTransferTableRequestDto>
{
    public WarehouseTransferTableRequestDtoValidator()
    {
        RuleFor(x => x.ProductTableId).GreaterThan(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
    }
}
