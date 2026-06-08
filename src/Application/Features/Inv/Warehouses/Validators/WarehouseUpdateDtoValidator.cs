using FluentValidation;

namespace Application.Features.Warehouses;

public class WarehouseUpdateDtoValidator : AbstractValidator<WarehouseUpdateDto>
{
    public WarehouseUpdateDtoValidator()
    {
        Include(new WarehouseBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
