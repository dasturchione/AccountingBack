using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductPrices;

public class ProductPriceService : IProductPriceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductPrice> _query;
    private readonly ICommandRepository<ProductPrice> _command;
    private readonly IProductPriceCalculateService _priceCalculateService;

    public ProductPriceService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ProductPrice> query,
                               ICommandRepository<ProductPrice> command,
                               IProductPriceCalculateService priceCalculateService)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
        _priceCalculateService = priceCalculateService;
    }

    public async Task<Result<long>> CreateAsync(ProductPriceCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        var entity = new ProductPrice
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            CurrencyId = dto.CurrencyId,
            PriceTypeId = dto.PriceTypeId,
            UnitId = dto.UnitId,
            Price = dto.Price,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ProductPrice>().Where(e => e.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ProductPriceErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ProductPriceListDto>>> GetAllAsync(ProductPriceListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ProductPrice, ProductPriceListDto, ProductPriceListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ProductPriceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ProductPrice>().Where(e => e.Id == id).As<ProductPriceDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<ProductPriceDto>(ProductPriceErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result<ProductPriceDetailsDto>> GetPriceDetailsByProductIdAsync(int productId, CancellationToken ct = default)
    {
        var productIds = new[] { productId };
        var salePriceMap = await _priceCalculateService.GetSalePriceMapAsync(productIds, ct);
        var costPriceMap = await _priceCalculateService.GetCostPriceMapAsync(productIds, ct);

        salePriceMap.TryGetValue(productId, out var salePrice);
        costPriceMap.TryGetValue(productId, out var costPrice);

        return new ProductPriceDetailsDto
        {
            Sale = salePrice ?? new ProductSalePriceDto(),
            Cost = costPrice ?? new ProductCostPriceDto()
        };
    }

    public async Task<Result> UpdateAsync(long id, ProductPriceUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ProductPrice>().Where(e => e.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ProductPriceErrors.NotFound(id, _userContext.LanguageId));

        entity.ProductId = dto.ProductId;
        entity.CurrencyId = dto.CurrencyId;
        entity.PriceTypeId = dto.PriceTypeId;
        entity.UnitId = dto.UnitId;
        entity.Price = dto.Price;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
