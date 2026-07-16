using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductDtoProjection : IProjectionBuilder<Product, ProductDto>
{
    public Expression<Func<Product, ProductDto>> Build() =>
        x => new ProductDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            ProductGroupId = x.ProductGroupId,
            ProductGroupName = x.ProductGroup != null ? x.ProductGroup.Name : null,
            Code = x.Code,
            Sku = x.Sku,
            Article = x.Article,
            UnitId = x.UnitId,
            UnitName = x.Unit.Name,
            Barcode = x.Barcode,
            Name = x.Name,
            Description = x.Description,
            IsPieceTracked = x.IsPieceTracked,
            IsService = x.IsService,
            IsSold = x.IsSold,
            IsPurchased = x.IsPurchased,
            Mxik = x.Mxik,
            DefaultVatRateId = x.DefaultVatRateId,
            MinStock = x.MinStock,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
