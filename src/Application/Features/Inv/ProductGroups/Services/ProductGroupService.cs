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

    public ProductGroupService(
        IUserContext userContext,
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
            var organizationId = _userContext.OrganizationId;
            if (dto.Products.Count > 0 && (!organizationId.HasValue || organizationId.Value <= 0))
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (await _query.AnyAsync(group => group.Code == dto.Code, ct))
                return Result.Failure<int>(ProductGroupErrors.CodeConflict(dto.Code, _userContext.LanguageId));

            var entity = new ProductGroup
            {
                Code = dto.Code,
                ParentId = dto.ParentId,
                IsAssignable = dto.IsAssignable,
                Name = dto.Name,
                SortOrder = dto.SortOrder,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            foreach (var productDto in dto.Products)
            {
                entity.Products.Add(new Product
                {
                    OrganizationId = organizationId!.Value,
                    Code = productDto.Code,
                    Sku = productDto.Sku,
                    Article = productDto.Article,
                    Mxik = productDto.Mxik,
                    UnitId = productDto.UnitId,
                    Barcode = productDto.Barcode,
                    Name = productDto.Name,
                    Description = productDto.Description,
                    IsService = productDto.IsService,
                    IsPieceTracked = productDto.IsPieceTracked,
                    IsSold = productDto.IsSold,
                    IsPurchased = productDto.IsPurchased,
                    DefaultVatRateId = productDto.DefaultVatRateId,
                    MinStock = productDto.MinStock,
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
            var query = _queryBuilder.For<ProductGroup>().Where(group => group.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity is null)
                return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<ProductGroupListDto>>> GetAllAsync(ProductGroupListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (filter.IsService.HasValue &&
                (_userContext.OrganizationId is not int organizationId || organizationId <= 0))
                return Result.Failure<PagedResponse<ProductGroupListDto>>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            filter.OrganizationId = _userContext.OrganizationId;
            var query = _queryBuilder.BuildPaged<ProductGroup, ProductGroupListDto, ProductGroupListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<ProductGroupDto>> GetByIdAsync(int id, bool? isService, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<ProductGroupDto>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<ProductGroup>()
                .Where(group => group.Id == id && group.IsAssignable)
                .As(new ProductGroupDtoProjection(_userContext).Build(isService))
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity is null)
                return Result.Failure<ProductGroupDto>(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

            return entity;
        });

    public Task<Result> UpdateAsync(int id, ProductGroupUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<ProductGroup>().Where(group => group.Id == id).Build();
            query.AddIncludes(builder => builder.Include(group =>
                group.Products.Where(product => product.OrganizationId == organizationId)));

            var entity = await _query.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

            if (entity.Code != dto.Code && await _query.AnyAsync(group => group.Code == dto.Code, ct))
                return Result.Failure(ProductGroupErrors.CodeConflict(dto.Code, _userContext.LanguageId));

            entity.Code = dto.Code;
            entity.ParentId = dto.ParentId;
            entity.IsAssignable = dto.IsAssignable;
            entity.Name = dto.Name;
            entity.SortOrder = dto.SortOrder;
            entity.StateId = dto.StateId;

            foreach (var productDto in dto.Products)
            {
                Product? product;
                if (productDto.Id.HasValue)
                {
                    product = entity.Products.FirstOrDefault(item => item.Id == productDto.Id.Value);
                    if (product is null)
                        continue;
                }
                else
                {
                    product = new Product
                    {
                        OrganizationId = organizationId,
                        CreatedDate = DateTime.Now,
                        StateId = StateIdConst.ACTIVE
                    };
                    entity.Products.Add(product);
                }

                product.Code = productDto.Code;
                product.Sku = productDto.Sku;
                product.Mxik = productDto.Mxik;
                product.Article = productDto.Article;
                product.Name = productDto.Name;
                product.Barcode = productDto.Barcode;
                product.Description = productDto.Description;
                product.UnitId = productDto.UnitId;
                product.IsPieceTracked = productDto.IsPieceTracked;
                product.IsService = productDto.IsService;
                product.IsSold = productDto.IsSold;
                product.IsPurchased = productDto.IsPurchased;
                product.DefaultVatRateId = productDto.DefaultVatRateId;
                product.MinStock = productDto.MinStock;
                product.StateId = productDto.StateId ?? StateIdConst.ACTIVE;
            }

            var dtoIds = dto.Products
                .Where(product => product.Id.HasValue)
                .Select(product => product.Id!.Value)
                .ToHashSet();

            foreach (var product in entity.Products.Where(product => !dtoIds.Contains(product.Id)))
                product.StateId = StateIdConst.PASSIVE;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });
}
