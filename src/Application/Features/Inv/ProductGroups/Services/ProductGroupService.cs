using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.ProductGroups;

public class ProductGroupService : IProductGroupService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<ProductGroup> _query;
    private readonly ICommandRepository<ProductGroup> _command;
    private readonly IQueryBuilder<ProductGroup> _queryBuilder;

    public ProductGroupService(IUserContext userContext, IQueryRepository<ProductGroup> query,
        ICommandRepository<ProductGroup> command, IQueryBuilder<ProductGroup> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(ProductGroupCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure<int>(ProductGroupErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new ProductGroup
        {
            OrganizationId = dto.OrganizationId,
            ParentId = dto.ParentId,
            Code = dto.Code,
            Name = dto.Name,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ProductGroupListDto>>> GetAllAsync(ProductGroupListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<ProductGroupListDto, ProductGroupListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ProductGroupDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<ProductGroup, ProductGroupDto>(id), ct);
        if (entity == null) return Result.Failure<ProductGroupDto>(ProductGroupErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ProductGroupUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(ProductGroupErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure(ProductGroupErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.ParentId = dto.ParentId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
