using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Orders;

public sealed class MarkingOrderCreateRequestDtoValidator : AbstractValidator<MarkingOrderCreateRequestDto>
{
    public MarkingOrderCreateRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);

        RuleFor(x => x)
            .Must(x => x.BusinessPlaceId.HasValue || x.WarehouseId.HasValue)
            .WithMessage("Either businessPlaceId or warehouseId must be provided.");

        When(x => x.BusinessPlaceId.HasValue, () =>
            RuleFor(x => x.BusinessPlaceId!.Value).GreaterThan(0));

        When(x => x.WarehouseId.HasValue, () =>
            RuleFor(x => x.WarehouseId!.Value).GreaterThan(0));
    }
}
