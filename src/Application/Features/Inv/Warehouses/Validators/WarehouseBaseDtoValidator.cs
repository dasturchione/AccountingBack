using FluentValidation;

namespace Application.Features.Warehouses;

public class WarehouseBaseDtoValidator : AbstractValidator<WarehouseBaseDto>
{
    public WarehouseBaseDtoValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);
        RuleFor(x => x.Code)
            .MaximumLength(100)
            .When(x => x.Code != null);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Address)
            .MaximumLength(1000)
            .When(x => x.Address != null);
        RuleFor(x => x.ResponsibleUserId)
            .GreaterThan(0)
            .When(x => x.ResponsibleUserId.HasValue);
    }
}
