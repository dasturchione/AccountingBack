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
            Code = x.Code,
            ParentId = x.ParentId,
            SortOrder = x.SortOrder,
            Name = x.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            Products = x.Products.Select(s => new ProductGroupTableDto
            {
                Id = s.Id,
                Code = s.Code,
                Sku = s.Sku,
                Article = s.Article,
                Barcode = s.Barcode,
                CreatedDate = s.CreatedDate,
                Description = s.Description,
                Name = s.Name,
                IsService = s.IsService,
                DefaultVatRateId = s.DefaultVatRateId,
                InventoryAccountId = s.InventoryAccountId,
                IncomeAccountId = s.IncomeAccountId,
                ExpenseAccountId = s.ExpenseAccountId,
                CogsAccountId = s.CogsAccountId,
                MinStock = s.MinStock,
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
