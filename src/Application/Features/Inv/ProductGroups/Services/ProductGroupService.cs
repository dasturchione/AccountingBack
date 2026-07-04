using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.ProductGroups;

public class ProductGroupService : BaseService, IProductGroupService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductGroup> _query;
    private readonly ICommandRepository<ProductGroup> _command;
    public ProductGroupService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ProductGroup> query,
                               ICommandRepository<ProductGroup> command,
                               ILogger<ProductGroupService> logger, 
                               IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public Task<Result<int>> CreateAsync(ProductGroupCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = new ProductGroup
            {
                OrganizationId = _userContext.OrganizationId.Value,
                Code = dto.Code,
                ParentId = dto.ParentId,
                Name = dto.Name,
                SortOrder = dto.SortOrder,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            foreach (var p in dto.Products)
            {
                entity.Products.Add(new Product
                {
                    OrganizationId = _userContext.OrganizationId.Value,
                    Code = p.Code,
                    Sku = p.Sku,
                    Article = p.Article,
                    ProductTypeId = p.ProductTypeId,
                    UnitId = p.UnitId,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    Description = p.Description,
                    IsService = p.IsService,
                    IsPieceTracked = p.IsPieceTracked,
                    IsSold = p.IsSold,
                    IsPurchased = p.IsPurchased,
                    DefaultVatRateId = p.DefaultVatRateId,
                    MinStock = p.MinStock,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                });
            }

            await _command.CreateAsync(entity, ct);

            return entity.Id;
        });

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<ProductGroup>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            
            if (entity == null)
                return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<ProductGroupListDto>>> GetAllAsync(ProductGroupListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<ProductGroup, ProductGroupListDto, ProductGroupListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<ProductGroupDto>> GetByIdAsync(int id, bool? isService, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<ProductGroup>().Where(x => x.Id == id).As<ProductGroupDto>().Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<ProductGroupDto>(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

            if (isService.HasValue)
                entity.Products = entity.Products.Where(x => x.IsService == isService).ToList();

            return entity;
        });

    public Task<Result> UpdateAsync(int id, ProductGroupUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<ProductGroup>().Where(x => x.Id == id).Build();

            query.AddIncludes(e => e.Include(i => i.Products));

            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

            entity.OrganizationId = _userContext.OrganizationId.Value;
            entity.Code = dto.Code;
            entity.ParentId = dto.ParentId;
            entity.Name = dto.Name;
            entity.SortOrder = dto.SortOrder;
            entity.StateId = dto.StateId;

            foreach (var dtoProduct in dto.Products)
            {
                Product? product = null;

                if (dtoProduct.Id.HasValue)
                {
                    product = entity.Products.FirstOrDefault(x => x.Id == dtoProduct.Id.Value);

                    if (product is null)
                        continue; 
                }
                else
                {
                    product = new Product
                    {
                        OrganizationId = _userContext.OrganizationId.Value,
                        CreatedDate = DateTime.Now,
                        StateId = StateIdConst.ACTIVE,
                    };

                    entity.Products.Add(product);
                }

                product.Code = dtoProduct.Code;
                product.Sku = dtoProduct.Sku;
                product.Article = dtoProduct.Article;
                product.ProductTypeId = dtoProduct.ProductTypeId;
                product.Name = dtoProduct.Name;
                product.Barcode = dtoProduct.Barcode;
                product.Description = dtoProduct.Description;
                product.UnitId = dtoProduct.UnitId;
                product.IsPieceTracked = dtoProduct.IsPieceTracked;
                product.IsService = dtoProduct.IsService;
                product.IsSold = dtoProduct.IsSold;
                product.IsPurchased = dtoProduct.IsPurchased;
                product.DefaultVatRateId = dtoProduct.DefaultVatRateId;
                product.MinStock = dtoProduct.MinStock;
                product.StateId = dtoProduct.StateId ?? StateIdConst.ACTIVE;
            }

            var dtoIds = dto.Products.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();

            var productsToDelete = entity.Products.Where(x => !dtoIds.Contains(x.Id)).ToList();

            foreach (var product in productsToDelete)
            {
                product.StateId = StateIdConst.PASSIVE;
            }

            await _command.UpdateAsync(entity, ct);

            return Result.Success();
        });
}
