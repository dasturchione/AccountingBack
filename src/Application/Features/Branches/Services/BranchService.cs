using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.Branches;

public class BranchService : IBranchService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Branch> _query;
    private readonly ICommandRepository<Branch> _command;
    private readonly IQueryBuilder<Branch> _queryBuilder;

    public BranchService(
        IUserContext userContext,
        IQueryRepository<Branch> query,
        ICommandRepository<Branch> command,
        IQueryBuilder<Branch> queryBuilder)
    {
        _userContext   = userContext;
        _query         = query;
        _command       = command;
        _queryBuilder  = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(BranchCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _query.AnyAsync(b => b.OrganizationId == dto.OrganizationId && b.Code == dto.Code, ct);
        if (exists)
            return Result.Failure<int>(BranchErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Branch
        {
            OrganizationId = dto.OrganizationId,
            Code           = dto.Code,
            Name           = dto.Name,
            RegionId       = dto.RegionId,
            DistrictId     = dto.DistrictId,
            Address        = dto.Address,
            PhoneNumber    = dto.PhoneNumber,
            StateId        = StateIdConst.ACTIVE,
            CreatedDate    = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var spec   = _queryBuilder.ById(id);
        var entity = await _query.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(BranchErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<BranchListDto>>> GetAllAsync(BranchListFilter filter, CancellationToken ct = default)
    {
        var spec      = _queryBuilder.BuildPaged<BranchListDto, BranchListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(spec, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<BranchDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var spec   = _queryBuilder.ById<Branch, BranchDto>(id);
        var entity = await _query.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure<BranchDto>(BranchErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, BranchUpdateDto dto, CancellationToken ct = default)
    {
        var spec   = _queryBuilder.ById(id);
        var entity = await _query.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(BranchErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code)
        {
            var exists = await _query.AnyAsync(b => b.OrganizationId == dto.OrganizationId && b.Code == dto.Code, ct);
            if (exists)
                return Result.Failure(BranchErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        }

        entity.OrganizationId = dto.OrganizationId;
        entity.Code           = dto.Code;
        entity.Name           = dto.Name;
        entity.RegionId       = dto.RegionId;
        entity.DistrictId     = dto.DistrictId;
        entity.Address        = dto.Address;
        entity.PhoneNumber    = dto.PhoneNumber;
        entity.StateId        = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
