using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductDtoProjection(IUserContext userContext) : IProjectionBuilder<Product, ProductDto>
{
    public Expression<Func<Product, ProductDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return product => new ProductDto
        {
            Id = product.Id,
            OrganizationId = product.OrganizationId,
            OrganizationName = product.Organization.ShortName,
            ProductGroupId = product.ProductGroupId,
            ProductGroupName = product.ProductGroup == null
                ? null
                : product.ProductGroup.ProductGroupTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Name)
                    .FirstOrDefault() ?? product.ProductGroup.Name,
            Code = product.Code,
            Sku = product.Sku,
            Article = product.Article,
            UnitId = product.UnitId,
            UnitName = product.Unit.Name,
            Barcode = product.Barcode,
            Name = product.Name,
            Description = product.Description,
            IsPieceTracked = product.IsPieceTracked,
            IsService = product.IsService,
            IsSold = product.IsSold,
            IsPurchased = product.IsPurchased,
            Mxik = product.Mxik,
            DefaultVatRateId = product.DefaultVatRateId,
            MinStock = product.MinStock,
            StateId = product.StateId,
            StateName = product.State.FullName,
            CreatedDate = product.CreatedDate
        };
    }
}
