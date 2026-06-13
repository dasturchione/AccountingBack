using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.ProductGroups;

public class ProductGroupService : IProductGroupService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductGroup> _query;
    private readonly ICommandRepository<ProductGroup> _command;

    public ProductGroupService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ProductGroup> query,
                               ICommandRepository<ProductGroup> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(ProductGroupCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var entity = new ProductGroup
        {
            OrganizationId = _userContext.OrganizationId.Value,
            ParentId       = dto.ParentId,
            Name           = dto.Name,
            StateId        = StateIdConst.ACTIVE,
            CreatedDate    = DateTime.Now
        };

        foreach (var p in dto.Products)
        {
            entity.Products.Add(new Product
            {
                OrganizationId = _userContext.OrganizationId.Value,
                UnitId         = p.UnitId,
                Barcode        = p.Barcode,
                Name           = p.Name,
                Description    = p.Description,
                IsService      = p.IsService,
                StateId        = StateIdConst.ACTIVE,
                CreatedDate    = DateTime.Now
            });
        }

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ProductGroup>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ProductGroupListDto>>> GetAllAsync(ProductGroupListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ProductGroup, ProductGroupListDto, ProductGroupListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ProductGroupDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ProductGroup>().Where(x => x.Id == id).As<ProductGroupDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<ProductGroupDto>(ProductGroupErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ProductGroupUpdateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ProductGroup>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = _userContext.OrganizationId.Value;
        entity.ParentId = dto.ParentId;
        entity.Name = dto.Name;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
