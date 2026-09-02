using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupListDtoProjection(IUserContext userContext) : IProjectionBuilder<ProductGroup, ProductGroupListDto>
{
    public Expression<Func<ProductGroup, ProductGroupListDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return group => new ProductGroupListDto
        {
            Id = group.Id,
            Code = group.Code,
            ParentId = group.ParentId,
            IsAssignable = group.IsAssignable,
            SortOrder = group.SortOrder,
            Name = group.ProductGroupTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? group.Name,
            StateId = group.StateId,
            StateName = group.State.FullName,
            CreatedDate = group.CreatedDate
        };
    }
}
