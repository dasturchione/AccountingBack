using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Products;

public class ProductService : IProductService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Product> _query;
    private readonly ICommandRepository<Product> _command;

    public ProductService(IUserContext userContext,
                          IQueryBuilder queryBuilder, 
                          IQueryRepository<Product> query,
                          ICommandRepository<Product> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Product
        {
            OrganizationId = dto.OrganizationId,
            ProductGroupId = dto.ProductGroupId,
            UnitId = dto.UnitId,
            Barcode = dto.Barcode,
            Name = dto.Name,
            Description = dto.Description,
            IsService = dto.IsService,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Product>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ProductErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ProductListDto>>> GetAllAsync(ProductListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Product, ProductListDto, ProductListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Product>().Where(x => x.Id == id).As<ProductDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<ProductDto>(ProductErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Product>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ProductErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.ProductGroupId = dto.ProductGroupId;
        entity.UnitId = dto.UnitId;
        entity.Barcode = dto.Barcode;
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsService = dto.IsService;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
