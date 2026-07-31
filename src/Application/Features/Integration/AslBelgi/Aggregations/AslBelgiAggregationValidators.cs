using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Aggregations;

public sealed class MarkingAggregationCreateRequestDtoValidator : AbstractValidator<MarkingAggregationCreateRequestDto>
{
    public MarkingAggregationCreateRequestDtoValidator()
    {
        RuleFor(x => x.ParentCode).NotEmpty();
        RuleFor(x => x.InnerCodes).NotEmpty();
        RuleForEach(x => x.InnerCodes).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.BusinessPlaceId.HasValue || x.WarehouseId.HasValue)
            .WithMessage("Either businessPlaceId or warehouseId must be provided.");

        When(x => x.BusinessPlaceId.HasValue, () =>
            RuleFor(x => x.BusinessPlaceId!.Value).GreaterThan(0));

        When(x => x.WarehouseId.HasValue, () =>
            RuleFor(x => x.WarehouseId!.Value).GreaterThan(0));

        RuleFor(x => x.PackingDate).NotEqual(default(DateOnly));

        RuleFor(x => x.PlannedCapacity).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.InnerCodes.Count <= x.PlannedCapacity)
            .WithMessage("The number of inner codes must not exceed plannedCapacity.")
            .When(x => x.PlannedCapacity > 0);

        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
    }
}
