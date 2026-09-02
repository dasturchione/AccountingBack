using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupDtoProjection(IUserContext userContext) : IProjectionBuilder<ProductGroup, ProductGroupDto>
{
    public Expression<Func<ProductGroup, ProductGroupDto>> Build() => Build(null);

    public Expression<Func<ProductGroup, ProductGroupDto>> Build(bool? isService)
    {
        var languageId = userContext.LanguageId;
        var organizationId = userContext.OrganizationId;

        return group => new ProductGroupDto
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
            CreatedDate = group.CreatedDate,
            Products = group.Products
                .Where(product => organizationId.HasValue &&
                                  product.OrganizationId == organizationId.Value &&
                                  (!isService.HasValue || product.IsService == isService.Value))
                .Select(product => new ProductGroupTableDto
                {
                    Id = product.Id,
                    Code = product.Code,
                    Sku = product.Sku,
                    Article = product.Article,
                    Barcode = product.Barcode,
                    Mxik = product.Mxik,
                    CreatedDate = product.CreatedDate,
                    Description = product.Description,
                    Name = product.Name,
                    IsPieceTracked = product.IsPieceTracked,
                    IsService = product.IsService,
                    IsSold = product.IsSold,
                    IsPurchased = product.IsPurchased,
                    DefaultVatRateId = product.DefaultVatRateId,
                    MinStock = product.MinStock,
                    OrganizationId = product.OrganizationId,
                    OrganizationName = product.Organization.FullName,
                    StateName = product.State.FullName,
                    StateId = product.StateId,
                    UnitCode = product.Unit.Code,
                    UnitId = product.Unit.Id,
                    UnitName = product.Unit.Name
                }).ToList()
        };
    }
}
