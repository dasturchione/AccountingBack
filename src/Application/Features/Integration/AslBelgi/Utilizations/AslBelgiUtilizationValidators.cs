using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Utilizations;

public sealed class MarkingUtilizationCreateRequestDtoValidator : AbstractValidator<MarkingUtilizationCreateRequestDto>
{
    private static readonly HashSet<string> AllowedReleaseTypes = new(StringComparer.Ordinal) { "PRODUCTION", "IMPORT", "CIRCULATION" };

    public MarkingUtilizationCreateRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.MarkingOrderId.HasValue || (x.Codes is { Count: > 0 }))
            .WithMessage("Either markingOrderId or a non-empty codes list must be provided.");

        RuleFor(x => x)
            .Must(x => x.BusinessPlaceId.HasValue || x.WarehouseId.HasValue)
            .WithMessage("Either businessPlaceId or warehouseId must be provided.");

        When(x => x.BusinessPlaceId.HasValue, () =>
            RuleFor(x => x.BusinessPlaceId!.Value).GreaterThan(0));

        When(x => x.WarehouseId.HasValue, () =>
            RuleFor(x => x.WarehouseId!.Value).GreaterThan(0));

        When(x => x.MarkingOrderId.HasValue, () =>
            RuleFor(x => x.MarkingOrderId!.Value).GreaterThan(0));

        RuleFor(x => x.ProductionDate).NotEqual(default(DateOnly));

        RuleFor(x => x.ReleaseType)
            .NotEmpty()
            .Must(AllowedReleaseTypes.Contains)
            .WithMessage("releaseType must be one of: PRODUCTION, IMPORT, CIRCULATION.");

        RuleFor(x => x.ManufacturerCountry)
            .NotEmpty()
            .Matches("^[A-Z]{2}$")
            .WithMessage("manufacturerCountry must be a 2-letter uppercase ISO country code.");

        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
    }
}
