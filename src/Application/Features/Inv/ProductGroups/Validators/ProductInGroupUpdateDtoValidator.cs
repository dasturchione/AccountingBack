using FluentValidation;

namespace Application.Features.ProductGroups;

public sealed class ProductInGroupUpdateDtoValidator : AbstractValidator<ProductInGroupUpdateDto>
{
    public ProductInGroupUpdateDtoValidator()
    {
        Include(new ProductInGroupBaseDtoValidator());
        RuleFor(product => product.Id).GreaterThan(0).When(product => product.Id.HasValue);
        RuleFor(product => product.StateId).GreaterThan((short)0).When(product => product.StateId.HasValue);
    }
}
