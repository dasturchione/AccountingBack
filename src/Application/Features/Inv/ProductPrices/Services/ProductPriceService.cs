using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
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
    private readonly IQueryRepository<Product> _productQuery;
    private readonly ICommandRepository<ProductPrice> _command;
    private readonly IProductPriceCalculateService _priceCalculateService;

    public ProductPriceService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ProductPrice> query,
                               IQueryRepository<Product> productQuery,
                               ICommandRepository<ProductPrice> command,
                               IProductPriceCalculateService priceCalculateService)
    {
        _query = query;
        _productQuery = productQuery;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
        _priceCalculateService = priceCalculateService;
    }

    public async Task<Result<long>> CreateAsync(ProductPriceCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!await ProductBelongsToOrganizationAsync(dto.ProductId, organizationId, ct))
            return Result.Failure<long>(ProductPriceErrors.ProductNotFound(dto.ProductId, _userContext.LanguageId));

        var entity = new ProductPrice
        {
            OrganizationId = organizationId,
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
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ProductPrice>()
            .Where(e => e.Id == id && e.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ProductPriceErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ProductPriceListDto>>> GetAllAsync(ProductPriceListFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<PagedResponse<ProductPriceListDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        filter.OrganizationId = organizationId;
        var query = _queryBuilder.BuildPaged<ProductPrice, ProductPriceListDto, ProductPriceListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ProductPriceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<ProductPriceDto>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ProductPrice>()
            .Where(e => e.Id == id && e.OrganizationId == organizationId)
            .As<ProductPriceDto>()
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<ProductPriceDto>(ProductPriceErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result<ProductPriceDetailsDto>> GetPriceDetailsByProductIdAsync(int productId, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<ProductPriceDetailsDto>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!await ProductBelongsToOrganizationAsync(productId, organizationId, ct))
            return Result.Failure<ProductPriceDetailsDto>(
                ProductPriceErrors.ProductNotFound(productId, _userContext.LanguageId));

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
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ProductPrice>()
            .Where(e => e.Id == id && e.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ProductPriceErrors.NotFound(id, _userContext.LanguageId));

        if (!await ProductBelongsToOrganizationAsync(dto.ProductId, organizationId, ct))
            return Result.Failure(ProductPriceErrors.ProductNotFound(dto.ProductId, _userContext.LanguageId));

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

    private Task<bool> ProductBelongsToOrganizationAsync(
        int productId,
        int organizationId,
        CancellationToken ct) =>
        _productQuery.AnyAsync(
            product => product.Id == productId && product.OrganizationId == organizationId,
            ct);
}
