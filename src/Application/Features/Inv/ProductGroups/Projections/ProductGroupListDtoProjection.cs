using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupListDtoProjection : IProjectionBuilder<ProductGroup, ProductGroupListDto>
{
    public Expression<Func<ProductGroup, ProductGroupListDto>> Build() =>
        x => new ProductGroupListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            Code = x.Code,
            ParentId = x.ParentId,
            SortOrder = x.SortOrder,
            Name = x.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
