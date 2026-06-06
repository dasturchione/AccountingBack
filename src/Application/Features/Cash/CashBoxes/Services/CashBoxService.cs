using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.CashBoxes;

public class CashBoxService : ICashBoxService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<CashBox> _query;
    private readonly ICommandRepository<CashBox> _command;
    private readonly IQueryBuilder<CashBox> _queryBuilder;

    public CashBoxService(IUserContext userContext, IQueryRepository<CashBox> query,
        ICommandRepository<CashBox> command, IQueryBuilder<CashBox> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CashBoxCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure<int>(CashBoxErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new CashBox
        {
            OrganizationId = dto.OrganizationId,
            BranchId = dto.BranchId,
            Code = dto.Code,
            Name = dto.Name,
            CurrencyId = dto.CurrencyId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CashBoxErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CashBoxListDto>>> GetAllAsync(CashBoxListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<CashBoxListDto, CashBoxListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CashBoxDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<CashBox, CashBoxDto>(id), ct);
        if (entity == null) return Result.Failure<CashBoxDto>(CashBoxErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CashBoxUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CashBoxErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure(CashBoxErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.BranchId = dto.BranchId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.CurrencyId = dto.CurrencyId;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
