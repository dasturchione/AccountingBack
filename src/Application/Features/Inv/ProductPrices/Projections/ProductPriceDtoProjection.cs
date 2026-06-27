using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductPrices;

public class ProductPriceDtoProjection : IProjectionBuilder<ProductPrice, ProductPriceDto>
{
    public Expression<Func<ProductPrice, ProductPriceDto>> Build() =>
        x => new ProductPriceDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            ProductId = x.ProductId,
            ProductName = x.Product.Name,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            PriceTypeId = x.PriceTypeId,
            PriceTypeCode = x.PriceType.Code,
            PriceTypeName = x.PriceType.Name,
            UnitId = x.UnitId,
            UnitCode = x.Unit.Code,
            UnitName = x.Unit.Name,
            Price = x.Price,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
