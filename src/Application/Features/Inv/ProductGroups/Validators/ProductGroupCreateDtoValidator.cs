using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupCreateDtoValidator : AbstractValidator<ProductGroupCreateDto>
{
    public ProductGroupCreateDtoValidator()
    {
        Include(new ProductGroupBaseDtoValidator());
        RuleForEach(x => x.Products)
            .ChildRules(product => product.RuleFor(x => x.Mxik).Length(17).When(x => x.Mxik != null));
    }
}
