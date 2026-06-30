using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashBoxes;

public class CashBoxService : ICashBoxService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CashBox> _query;
    private readonly ICommandRepository<CashBox> _command;

    public CashBoxService(IUserContext userContext,
                          IQueryBuilder queryBuilder, 
                          IQueryRepository<CashBox> query,
                          ICommandRepository<CashBox> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CashBoxCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        if (await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure<int>(CashBoxErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new CashBox
        {
            OrganizationId = orgId,
            BranchId = dto.BranchId,
            Code = dto.Code,
            Name = dto.Name,
            CurrencyId = dto.CurrencyId,
            IsMain = dto.IsMain,
            ResponsibleUserId = dto.ResponsibleUserId,
            OpeningBalance = dto.OpeningBalance,
            OpeningBalanceDate = dto.OpeningBalanceDate,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashBox>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(CashBoxErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CashBoxListDto>>> GetAllAsync(CashBoxListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<CashBox, CashBoxListDto, CashBoxListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CashBoxDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashBox>().Where(x => x.Id == id).As<CashBoxDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CashBoxDto>(CashBoxErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CashBoxUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashBox>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) return Result.Failure(CashBoxErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure(CashBoxErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        entity.BranchId = dto.BranchId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.CurrencyId = dto.CurrencyId;
        entity.IsMain = dto.IsMain;
        entity.ResponsibleUserId = dto.ResponsibleUserId;
        entity.OpeningBalance = dto.OpeningBalance;
        entity.OpeningBalanceDate = dto.OpeningBalanceDate;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
