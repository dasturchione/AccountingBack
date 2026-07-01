using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using DocumentFormat.OpenXml.Vml.Office;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public class BankOperationService : IBankOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<BankOperation> _query;
    private readonly ICommandRepository<BankOperation> _command;

    public BankOperationService(IUserContext userContext,
                                IQueryBuilder queryBuilder,
                                IAuditLogService auditLogService,
                                IQueryRepository<BankOperation> query,
                                ICommandRepository<BankOperation> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
    }

    public async Task<Result<long>> CreateAsync(BankOperationCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        var entity = BuildCreateEntity(dto, orgId, _userContext.Id);

        await _command.CreateAsync(entity, ct);

        var docDto = await GetByIdInternalAsync(entity.Id, ct);
        if (docDto != null)
        {
            _auditLogService.SetNewValues(docDto);
            await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
        }

        return entity.Id;
    }

    public async Task<Result<List<long>>> CreateManyAsync(BankOperationsCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;
        var entities = dto.Operations
            .Select(operation => BuildCreateEntity(operation, orgId, _userContext.Id))
            .ToList();

        await _command.CreateAsync(entities, ct);

        return entities.Select(x => x.Id).ToList();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankOperation>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

        var oldDocDto = await GetByIdInternalAsync(id, ct);
        if (oldDocDto != null)
            _auditLogService.SetOldValues(oldDocDto);

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);

        var newDocDto = await GetByIdInternalAsync(id, ct);
        if (newDocDto != null)
        {
            _auditLogService.SetNewValues(newDocDto);
            await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, id.ToString(), AuditLogOperationTypeConst.Delete);
        }

        return Result.Success();
    }

    public async Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<BankOperation, BankOperationListDto, BankOperationListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankOperation>().Where(x => x.Id == id).As<BankOperationDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<BankOperationDto>(BankOperationErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, BankOperationUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankOperation>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

        var oldDocDto = await GetByIdInternalAsync(id, ct);
        if (oldDocDto != null)
            _auditLogService.SetOldValues(oldDocDto);

        entity.BankAccountId = dto.BankAccountId;
        entity.OperationTypeId = dto.OperationTypeId;
        entity.PaymentTypeId = 2;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.DocDate = dto.DocDate;
        entity.CounterpartyBankAccountId = dto.CounterpartyBankAccountId;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
        entity.PostedAt ??= DateTime.Now;
        entity.PostedByUserId ??= _userContext.Id;
        entity.Comment = dto.Comment;
        entity.ContractId = dto.ContractId;
        entity.StatusId = DocumentStatusIdConst.POSTED;
        entity.StateId = StateIdConst.ACTIVE;

        entity.BankOperationLines.Add(new BankOperationLine
        {
            Amount = dto.Amount,
            CounterpartyId = dto.CounterpartyId,
            PaymentPurposeId = dto.PaymentPurposeId,
            OrderNumber = (short)(entity.BankOperationLines.Count() + 1),
            Comment = dto.Comment,
        });

        await _command.UpdateAsync(entity, ct);

        var newDocDto = await GetByIdInternalAsync(id, ct);
        if (newDocDto != null)
        {
            _auditLogService.SetNewValues(newDocDto);
            await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
        }

        return Result.Success();
    }

    private async Task<BankOperationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<BankOperation>().Where(x => x.Id == id).As<BankOperationDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private static BankOperation BuildCreateEntity(BankOperationCreateDto dto, int orgId, int? userId) =>
        new()
        {
            OrganizationId = orgId,
            BankAccountId = dto.BankAccountId,
            OperationTypeId = dto.OperationTypeId,
            PaymentTypeId = 2,
            CounterpartyId = dto.CounterpartyId,
            DocNumber = string.Empty,
            DocDate = dto.DocDate,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            ContractId = dto.ContractId,
            ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
            PostedAt = DateTime.Now,
            PostedByUserId = userId,
            Comment = dto.Comment,
            CounterpartyBankAccountId = dto.CounterpartyBankAccountId,
            StatusId = DocumentStatusIdConst.POSTED,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now,
            BankOperationLines = new List<BankOperationLine>
            {
                new BankOperationLine
                {
                    Amount = dto.Amount,
                    CounterpartyId = dto.CounterpartyId,
                    PaymentPurposeId = dto.PaymentPurposeId,
                    OrderNumber = 1,
                    Comment = dto.Comment,
                }
            }
        };
}
