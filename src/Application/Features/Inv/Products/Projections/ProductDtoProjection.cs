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
            UnitId = x.UnitId,
            UnitName = x.Unit.Name,
            Code = x.Code,
            Barcode = x.Barcode,
            Name = x.Name,
            Description = x.Description,
            IsService = x.IsService,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
