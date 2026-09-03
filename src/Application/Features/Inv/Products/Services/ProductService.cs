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
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (dto.Code is not null &&
                await _query.AnyAsync(
                    product => product.OrganizationId == organizationId && product.Code == dto.Code,
                    ct))
                return Result.Failure<int>(ProductErrors.CodeConflict(dto.Code, _userContext.LanguageId));

            var entity = new Product
            {
                OrganizationId = organizationId,
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
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var requestedCodes = dto.Products
                .Where(product => product.Code is not null)
                .Select(product => product.Code!)
                .ToArray();
            var duplicateCode = requestedCodes
                .GroupBy(code => code)
                .FirstOrDefault(group => group.Count() > 1)
                ?.Key;
            if (duplicateCode is not null)
                return Result.Failure(ProductErrors.CodeConflict(duplicateCode, _userContext.LanguageId));

            if (requestedCodes.Length > 0)
            {
                var conflictQuery = _queryBuilder.For<Product>()
                    .Where(product => product.OrganizationId == organizationId &&
                                      product.Code != null &&
                                      requestedCodes.Contains(product.Code))
                    .Build();
                var conflict = await _query.GetAsync(conflictQuery, ct);
                if (conflict?.Code is not null)
                    return Result.Failure(ProductErrors.CodeConflict(conflict.Code, _userContext.LanguageId));
            }

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
                OrganizationId = organizationId,
                Name = s.Name,
            }).ToList();

            await _command.CreateAsync(entities, ct);

            return Result.Success();
        });

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Product>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build();
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
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<PagedResponse<ProductListDto>>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.BuildPaged<Product, ProductListDto, ProductListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<ProductDto>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Product>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .As<ProductDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<ProductDto>(ProductErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        });

    public Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Product>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build();

            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(ProductErrors.NotFound(id, _userContext.LanguageId));

            if (dto.Code is not null &&
                entity.Code != dto.Code &&
                await _query.AnyAsync(
                    product => product.OrganizationId == organizationId && product.Code == dto.Code,
                    ct))
                return Result.Failure(ProductErrors.CodeConflict(dto.Code, _userContext.LanguageId));

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
