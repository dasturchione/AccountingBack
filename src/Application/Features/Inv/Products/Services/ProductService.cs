using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Products;

public class ProductService : BaseService, IProductService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Product> _query;
    private readonly ICommandRepository<Product> _command;
    public ProductService(IUserContext userContext,
                          IQueryBuilder queryBuilder, 
                          IQueryRepository<Product> query,
                          ICommandRepository<Product> command, 
                          ILogger<ProductService> logger, 
                          IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public Task<Result<int>> CreateAsync(ProductCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync<int>(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = new Product
            {
                OrganizationId = _userContext.OrganizationId.Value,
                Code = dto.Code,
                Sku = dto.Sku,
                Article = dto.Article,
                ProductGroupId = dto.ProductGroupId,
                UnitId = dto.UnitId,
                Barcode = dto.Barcode,
                Name = dto.Name,
                IsPieceTracked = dto.IsPieceTracked,
                Description = dto.Description,
                IsService = dto.IsService,
                IsSold = dto.IsSold,
                IsPurchased = dto.IsPurchased,
                Mxik = dto.Mxik,
                DefaultVatRateId = dto.DefaultVatRateId,
                MinStock = dto.MinStock,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _command.CreateAsync(entity, ct);
            return entity.Id;
        });

    public Task<Result> CreateManyAsync(ProductsCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateManyAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entities = dto.Products.Select(s => new Product
            {
                Code = s.Code,
                Sku = s.Sku,
                Article = s.Article,
                ProductGroupId = s.ProductGroupId,
                CreatedDate = DateTime.Now,
                Barcode = s.Barcode,
                Description = s.Description,
                IsPieceTracked = s.IsPieceTracked,
                IsService = s.IsService,
                IsSold = s.IsSold,
                IsPurchased = s.IsPurchased,
                Mxik = s.Mxik,
                DefaultVatRateId = s.DefaultVatRateId,
                MinStock = s.MinStock,
                StateId = StateIdConst.ACTIVE,
                UnitId = s.UnitId,
                OrganizationId = _userContext.OrganizationId.Value,
                Name = s.Name,
            });

            await _command.CreateAsync(entities, ct);

            return Result.Success();
        });

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<Product>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure(ProductErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<ProductListDto>>> GetAllAsync(ProductListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<Product, ProductListDto, ProductListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<Product>().Where(x => x.Id == id).As<ProductDto>().Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<ProductDto>(ProductErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        });

    public Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Product>().Where(x => x.Id == id).Build();

            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(ProductErrors.NotFound(id, _userContext.LanguageId));

            entity.OrganizationId = _userContext.OrganizationId.Value;
            entity.Code = dto.Code;
            entity.Sku = dto.Sku;
            entity.Article = dto.Article;
            entity.ProductGroupId = dto.ProductGroupId;
            entity.UnitId = dto.UnitId;
            entity.Barcode = dto.Barcode;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.IsService = dto.IsService;
            entity.IsSold = dto.IsSold;
            entity.IsPurchased = dto.IsPurchased;
            entity.Mxik = dto.Mxik;
            entity.StateId = dto.StateId;
            entity.IsPieceTracked = dto.IsPieceTracked;
            entity.DefaultVatRateId = dto.DefaultVatRateId;
            entity.MinStock = dto.MinStock;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });
}
