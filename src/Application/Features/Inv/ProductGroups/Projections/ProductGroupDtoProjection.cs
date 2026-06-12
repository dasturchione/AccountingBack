using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupDtoProjection : IProjectionBuilder<ProductGroup, ProductGroupDto>
{
    public Expression<Func<ProductGroup, ProductGroupDto>> Build() =>
        x => new ProductGroupDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            ParentId = x.ParentId,
            ParentName = x.Parent != null ? x.Parent.Name : null,
            Name = x.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            Products = x.Products.Select(s => new ProductGroupTableDto
            {
                Id = s.Id,
                Barcode = s.Barcode,
                CreatedDate = s.CreatedDate,
                Description = s.Description,
                Name = s.Name,
                IsService = s.IsService,
                OrganizationId = s.OrganizationId,
                OrganizationName = s.Organization.FullName,
                StateName = s.State.FullName,
                StateId = s.StateId,
                UnitCode = s.Unit.Code,
                UnitId = s.Unit.Id,
                UnitName = s.Unit.Name
            }).ToList(),
        };
}
