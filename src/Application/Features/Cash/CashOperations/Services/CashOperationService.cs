using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public class CashOperationService : ICashOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<CashOperation> _query;
    private readonly ICommandRepository<CashOperation> _command;
    private readonly IQueryBuilder<CashOperation> _queryBuilder;

    public CashOperationService(IUserContext userContext, IQueryRepository<CashOperation> query,
        ICommandRepository<CashOperation> command, IQueryBuilder<CashOperation> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(CashOperationCreateDto dto, CancellationToken ct = default)
    {
        var entity = new CashOperation
        {
            OrganizationId = dto.OrganizationId,
            CashBoxId = dto.CashBoxId,
            OperationTypeId = dto.OperationTypeId,
            PaymentTypeId = dto.PaymentTypeId,
            CounterpartyId = dto.CounterpartyId,
            DocNumber = dto.DocNumber,
            DocDate = dto.DocDate,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            Comment = dto.Comment,
            StatusId = dto.StatusId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<CashOperationListDto, CashOperationListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<CashOperation, CashOperationDto>(id), ct);
        if (entity == null) return Result.Failure<CashOperationDto>(CashOperationErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, CashOperationUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.CashBoxId = dto.CashBoxId;
        entity.OperationTypeId = dto.OperationTypeId;
        entity.PaymentTypeId = dto.PaymentTypeId;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.DocNumber = dto.DocNumber;
        entity.DocDate = dto.DocDate;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.Comment = dto.Comment;
        entity.StatusId = dto.StatusId;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
