using FluentValidation;

namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryCreateDtoValidator : AbstractValidator<OpeningInventoryCreateDto>
{
    public OpeningInventoryCreateDtoValidator()
    {
        Include(new OpeningInventoryBaseDtoValidator());
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new OpeningInventoryProductCreateDtoValidator());
    }
}

public sealed class OpeningInventoryUpdateDtoValidator : AbstractValidator<OpeningInventoryUpdateDto>
{
    public OpeningInventoryUpdateDtoValidator()
    {
        Include(new OpeningInventoryBaseDtoValidator());
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new OpeningInventoryProductCreateDtoValidator());
    }
}

internal sealed class OpeningInventoryBaseDtoValidator : AbstractValidator<OpeningInventoryBaseDto>
{
    public OpeningInventoryBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.CounterpartyId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.ContractId).GreaterThan(0).When(x => x.ContractId.HasValue);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
    }
}

internal sealed class OpeningInventoryProductCreateDtoValidator
    : AbstractValidator<OpeningInventoryProductCreateDto>
{
    public OpeningInventoryProductCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DebitAccountId).GreaterThan(0);
        RuleForEach(x => x.Items).SetValidator(new OpeningInventoryItemCreateDtoValidator());
    }
}

internal sealed class OpeningInventoryItemCreateDtoValidator
    : AbstractValidator<OpeningInventoryTableCreateDto>
{
    public OpeningInventoryItemCreateDtoValidator()
    {
        RuleFor(x => x.MarkingNumber).NotEmpty().MaximumLength(250);
        RuleFor(x => x.SerialNumber).MaximumLength(250).When(x => x.SerialNumber != null);
    }
}
