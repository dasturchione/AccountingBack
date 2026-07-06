using FluentValidation;

namespace Application.Features.FaAssets;

public class FaAssetBaseDtoValidator : AbstractValidator<FaAssetBaseDto>
{
    public FaAssetBaseDtoValidator()
    {
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FaGroupId).GreaterThan(0);
        RuleFor(x => x.DepreciationMethodId).GreaterThan((short)0);
        RuleFor(x => x.UsefulLifeMonths).GreaterThan(0);
        RuleFor(x => x.InitialCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalvageValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalvageValue).LessThanOrEqualTo(x => x.InitialCost);
        RuleFor(x => x.PlannedUnitsTotal).GreaterThan(0).When(x => x.PlannedUnitsTotal.HasValue);
        RuleFor(x => x.DeprStartDate)
            .GreaterThanOrEqualTo(x => x.CommissioningDate!.Value)
            .When(x => x.CommissioningDate.HasValue && x.DeprStartDate.HasValue);
    }
}
