using FluentValidation;

namespace Application.Features.Warehouses;

public class WarehouseCreateDtoValidator : AbstractValidator<WarehouseCreateDto>
{
    public WarehouseCreateDtoValidator()
    {
        Include(new WarehouseBaseDtoValidator());
    }
}
