using FluentValidation;

namespace Application.Features.Products;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        Include(new ProductBaseDtoValidator());
    }
}

public class ProductsCreateDtoValidator : AbstractValidator<ProductsCreateDto>
{
    public ProductsCreateDtoValidator()
    {
        RuleFor(x => x.Products).NotNull();

        RuleFor(x => x.Products).NotEmpty();

        RuleForEach(x => x.Products).SetValidator(new ProductCreateDtoValidator());
    }
}
