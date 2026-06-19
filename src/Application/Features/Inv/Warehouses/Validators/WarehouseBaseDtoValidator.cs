using FluentValidation;

namespace Application.Features.Warehouses;

public class WarehouseBaseDtoValidator : AbstractValidator<WarehouseBaseDto>
{
    public WarehouseBaseDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
