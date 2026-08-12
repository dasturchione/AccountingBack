using FluentValidation;

namespace Application.Features.FaAssets;

public class FaAssetUpdateDtoValidator : AbstractValidator<FaAssetUpdateDto>
{
    public FaAssetUpdateDtoValidator()
    {
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FaGroupId).GreaterThan(0);
        RuleFor(x => x.DepreciationMethodId).GreaterThan((short)0);
        RuleFor(x => x.UsefulLifeMonths).GreaterThan(0);
        RuleFor(x => x.PlannedUnitsTotal).GreaterThan(0).When(x => x.PlannedUnitsTotal.HasValue);
    }
}