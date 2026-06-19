using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Branches;

public class BranchService : IBranchService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Branch> _query;
    private readonly ICommandRepository<Branch> _command;

    public BranchService(IUserContext userContext,
                         IQueryBuilder queryBuilder, 
                         IQueryRepository<Branch> query,
                         ICommandRepository<Branch> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(BranchCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        if (await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure<int>(BranchErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        var entity = new Branch
        {
            OrganizationId = orgId,
            Code = dto.Code,
            Name = dto.Name,
            RegionId = dto.RegionId,
            DistrictId = dto.DistrictId,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Branch>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(BranchErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<BranchListDto>>> GetAllAsync(BranchListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Branch, BranchListDto, BranchListFilter>(filter);
        var paged = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(paged, filter.Page, filter.PageSize);
    }

    public async Task<Result<BranchDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Branch>().Where(x => x.Id == id).As<BranchDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<BranchDto>(BranchErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, BranchUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Branch>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(BranchErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code &&
            await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure(BranchErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.RegionId = dto.RegionId;
        entity.DistrictId = dto.DistrictId;
        entity.Address = dto.Address;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
