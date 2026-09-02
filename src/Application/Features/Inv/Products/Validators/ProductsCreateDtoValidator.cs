using FluentValidation;

namespace Application.Features.Products;

public sealed class ProductsCreateDtoValidator : AbstractValidator<ProductsCreateDto>
{
    public ProductsCreateDtoValidator()
    {
        RuleFor(dto => dto.Products).NotNull().NotEmpty();
        RuleForEach(dto => dto.Products).SetValidator(new ProductCreateDtoValidator());
    }
}
