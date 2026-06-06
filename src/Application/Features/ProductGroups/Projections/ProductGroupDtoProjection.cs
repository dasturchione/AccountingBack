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
            Code = x.Code,
            Name = x.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
