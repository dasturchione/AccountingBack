using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductListDtoProjection : IProjectionBuilder<Product, ProductListDto>
{
    public Expression<Func<Product, ProductListDto>> Build() =>
        x => new ProductListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            ProductGroupId = x.ProductGroupId,
            ProductGroupName = x.ProductGroup != null ? x.ProductGroup.Name : null,
            UnitId = x.UnitId,
            UnitName = x.Unit.Name,
            Barcode = x.Barcode,
            Name = x.Name,
            IsService = x.IsService,
            Mxik = x.Mxik,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
