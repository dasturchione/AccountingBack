using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public class BankOperationService : IBankOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<BankOperation> _query;
    private readonly ICommandRepository<BankOperation> _command;
    private readonly IQueryBuilder<BankOperation> _queryBuilder;

    public BankOperationService(IUserContext userContext, IQueryRepository<BankOperation> query,
        ICommandRepository<BankOperation> command, IQueryBuilder<BankOperation> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(BankOperationCreateDto dto, CancellationToken ct = default)
    {
        var entity = new BankOperation
        {
            OrganizationId = dto.OrganizationId,
            BankAccountId = dto.BankAccountId,
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
        if (entity == null) return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<BankOperationListDto, BankOperationListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<BankOperation, BankOperationDto>(id), ct);
        if (entity == null) return Result.Failure<BankOperationDto>(BankOperationErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, BankOperationUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.BankAccountId = dto.BankAccountId;
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
