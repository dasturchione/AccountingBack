using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public class CashOperationService : ICashOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<CashOperation> _query;
    private readonly ICommandRepository<CashOperation> _command;

    public CashOperationService(IUserContext userContext,
                                IQueryBuilder queryBuilder,
                                IAuditLogService auditLogService,
                                IQueryRepository<CashOperation> query,
                                ICommandRepository<CashOperation> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
    }

    public async Task<Result<long>> CreateAsync(CashOperationCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        var entity = new CashOperation
        {
            OrganizationId = orgId,
            CashBoxId = dto.CashBoxId,
            OperationTypeId = dto.OperationTypeId,
            PaymentTypeId = dto.PaymentTypeId,
            CounterpartyId = dto.CounterpartyId,
            DocNumber = dto.DocNumber,
            DocDate = dto.DocDate,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
            PostedAt = dto.StatusId == DocumentStatusIdConst.POSTED ? DateTime.Now : null,
            PostedByUserId = dto.StatusId == DocumentStatusIdConst.POSTED ? _userContext.Id : null,
            CancelledAt = dto.StatusId == DocumentStatusIdConst.CANCELLED ? DateTime.Now : null,
            CancelledByUserId = dto.StatusId == DocumentStatusIdConst.CANCELLED ? _userContext.Id : null,
            Comment = dto.Comment,
            StatusId = dto.StatusId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);

        var docDto = await GetByIdInternalAsync(entity.Id, ct);
        if (docDto != null)
        {
            _auditLogService.SetNewValues(docDto);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
        }

        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashOperation>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

        var oldDocDto = await GetByIdInternalAsync(id, ct);
        if (oldDocDto != null)
            _auditLogService.SetOldValues(oldDocDto);

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);

        var newDocDto = await GetByIdInternalAsync(id, ct);
        if (newDocDto != null)
        {
            _auditLogService.SetNewValues(newDocDto);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, id.ToString(), AuditLogOperationTypeConst.Delete);
        }

        return Result.Success();
    }

    public async Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<CashOperation, CashOperationListDto, CashOperationListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashOperation>().Where(x => x.Id == id).As<CashOperationDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CashOperationDto>(CashOperationErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, CashOperationUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashOperation>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

        var oldDocDto = await GetByIdInternalAsync(id, ct);
        if (oldDocDto != null)
            _auditLogService.SetOldValues(oldDocDto);

        entity.CashBoxId = dto.CashBoxId;
        entity.OperationTypeId = dto.OperationTypeId;
        entity.PaymentTypeId = dto.PaymentTypeId;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.DocNumber = dto.DocNumber;
        entity.DocDate = dto.DocDate;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
        if (dto.StatusId == DocumentStatusIdConst.POSTED)
        {
            entity.PostedAt ??= DateTime.Now;
            entity.PostedByUserId ??= _userContext.Id;
        }
        if (dto.StatusId == DocumentStatusIdConst.CANCELLED)
        {
            entity.CancelledAt ??= DateTime.Now;
            entity.CancelledByUserId ??= _userContext.Id;
        }
        entity.Comment = dto.Comment;
        entity.StatusId = dto.StatusId;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);

        var newDocDto = await GetByIdInternalAsync(id, ct);
        if (newDocDto != null)
        {
            _auditLogService.SetNewValues(newDocDto);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
        }

        return Result.Success();
    }

    private async Task<CashOperationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<CashOperation>().Where(x => x.Id == id).As<CashOperationDto>().Build();
        return await _query.GetAsync(query, ct);
    }
}
