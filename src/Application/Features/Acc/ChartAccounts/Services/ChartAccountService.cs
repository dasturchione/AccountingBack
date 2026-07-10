using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.ChartAccounts;
using Domain.Entities;
using LinqKit;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public class ChartAccountService : IChartAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ChartAccount> _query;
    private readonly ICommandRepository<ChartAccount> _command;

    public ChartAccountService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ChartAccount> query, 
                               ICommandRepository<ChartAccount> command)
    {
        _query = query; 
        _command = command; 
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(ChartAccountCreateDto dto, CancellationToken ct = default)
    {
        if (!_userContext.OrganizationId.HasValue)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!string.IsNullOrEmpty(dto.Code) && await _query.AnyAsync(x => x.Code == dto.Code && x.OrganizationId == _userContext.OrganizationId.Value, ct))
            return Result.Failure<int>(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        if (await _query.AnyAsync(x => x.Number == dto.Number && x.OrganizationId == _userContext.OrganizationId.Value, ct))
            return Result.Failure<int>(ChartAccountErrors.NumberConflict(dto.Number, _userContext.LanguageId));

        var entity = new ChartAccount
        {
            ParentId = dto.ParentId,
            Code = dto.Code,
            Name = dto.Name,
            Number = dto.Number,
            IsGroup = dto.IsGroup,
            IsTaxAccounting = dto.IsTaxAccounting,
            IsQuantity = dto.IsQuantity,
            IsCurrency = dto.IsCurrency,
            IsDepartment = dto.IsDepartment,
            IsOffBalance = dto.IsOffBalance,
            AccountTypeId = dto.AccountTypeId,
            OrganizationId = _userContext.OrganizationId.Value,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ChartAccountListDto>>> GetAllAsync(ChartAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ChartAccount, ChartAccountListDto, ChartAccountListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PagedResponse<ChartAccountGroupedListDto>>> GetGroupedListAsync(ChartAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ChartAccount, ChartAccountGroupedListDto, ChartAccountListFilter>(filter);
        query.Criteria.And(x => x.ParentId == null);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ChartAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).As<ChartAccountDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure<ChartAccountDto>(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ChartAccountUpdateDto dto, CancellationToken ct = default)
    {
        if (!_userContext.OrganizationId.HasValue)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        if(entity.Code != dto.Code && !string.IsNullOrEmpty(dto.Code))
        {
            var codeConflict = await _query.AnyAsync(x => x.Code == dto.Code && x.OrganizationId == _userContext.OrganizationId.Value, ct);
            if (codeConflict)
                return Result.Failure(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        }

        if (entity.Number != dto.Number)
        {
            var numberConflict = await _query.AnyAsync(x => x.Number == dto.Number && x.OrganizationId == _userContext.OrganizationId.Value, ct);
            if (numberConflict)
                return Result.Failure(ChartAccountErrors.NumberConflict(dto.Number, _userContext.LanguageId));
        }

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Number = dto.Number;
        entity.ParentId = dto.ParentId;
        entity.IsGroup = dto.IsGroup;
        entity.IsCurrency = dto.IsCurrency;
        entity.IsQuantity = dto.IsQuantity;
        entity.IsDepartment = dto.IsDepartment;
        entity.IsOffBalance = dto.IsOffBalance;
        entity.AccountTypeId = dto.AccountTypeId;
        entity.IsTaxAccounting = dto.IsTaxAccounting;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
