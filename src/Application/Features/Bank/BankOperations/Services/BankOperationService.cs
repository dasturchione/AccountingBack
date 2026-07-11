using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public class BankOperationService : BaseService, IBankOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IBankLifecycleService _bankLifecycleService;
    private readonly IQueryRepository<BankOperation> _query;
    private readonly ICommandRepository<BankOperation> _command;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public BankOperationService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IBankLifecycleService bankLifecycleService,
        IDocNumberGenerator docNumberGenerator,
        IQueryRepository<BankOperation> query,
        ICommandRepository<BankOperation> command,
        ILogger<BankOperationService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _bankLifecycleService = bankLifecycleService;
        _docNumberGenerator = docNumberGenerator;
        _query = query;
        _command = command;
    }

    public Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<BankOperation, BankOperationListDto, BankOperationListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<BankOperationDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<BankOperation>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<BankOperationDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);
            return entity == null
                ? Result.Failure<BankOperationDto>(BankOperationErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(BankOperationCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = await BuildCreateEntityAsync(dto, _userContext.OrganizationId.Value, ct);
            await _command.CreateAsync(entity, ct);

            var docDto = await GetByIdInternalAsync(entity.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(entity.Id);
        }, ct);

    public Task<Result<List<long>>> CreateManyAsync(BankOperationsCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateManyAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<List<long>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entities = new List<BankOperation>(dto.Operations.Count);
            foreach (var operation in dto.Operations)
                entities.Add(await BuildCreateEntityAsync(operation, _userContext.OrganizationId.Value, ct));

            await _command.CreateAsync(entities, ct);

            foreach (var entity in entities)
            {
                var docDto = await GetByIdInternalAsync(entity.Id, ct);
                if (docDto == null)
                    continue;

                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(entities.Select(x => x.Id).ToList());
        }, ct);

    public Task<Result> UpdateAsync(long id, BankOperationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<BankOperation>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

            if (entity.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(BankOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(BankOperationErrors.CannotUpdateInCurrentStatus(id, entity.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            entity.BankAccountId = dto.BankAccountId;
            entity.OperationTypeId = dto.OperationTypeId;
            entity.PaymentTypeId = PaymentTypeIdConst.BANK;
            entity.CounterpartyId = dto.CounterpartyId;
            entity.CounterpartyBankAccountId = dto.CounterpartyBankAccountId;
            entity.BankChartAccountId = dto.BankChartAccountId;
            entity.OffsetAccountId = dto.OffsetAccountId;
            entity.ContractId = dto.ContractId;
            entity.DocDate = dto.DocDate;
            entity.CurrencyId = dto.CurrencyId;
            entity.Amount = dto.Amount;
            entity.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
            entity.Comment = dto.Comment;
            entity.PaymentPurposeId = dto.PaymentPurposeId;
            entity.StateId = StateIdConst.ACTIVE;

            await _command.UpdateAsync(entity, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _bankLifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _bankLifecycleService.CancelAsync(id, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<BankOperation>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

            if (entity.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(BankOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(BankOperationErrors.CannotDeleteInCurrentStatus(id, entity.StatusId, _userContext.LanguageId));

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
        }, ct);

    private async Task<BankOperationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<BankOperation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<BankOperationDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<BankOperation> BuildCreateEntityAsync(BankOperationCreateDto dto, int organizationId, CancellationToken ct)
    {
        var docNumber = await _docNumberGenerator.GenerateAsync(organizationId, "BNK", dto.DocDate, ct);

        return new BankOperation
        {
            OrganizationId = organizationId,
            BankAccountId = dto.BankAccountId,
            OperationTypeId = dto.OperationTypeId,
            PaymentTypeId = dto.PaymentTypeId,
            PaymentPurposeId = dto.PaymentPurposeId,
            CounterpartyId = dto.CounterpartyId,
            CounterpartyBankAccountId = dto.CounterpartyBankAccountId,
            BankChartAccountId = dto.BankChartAccountId,
            OffsetAccountId = dto.OffsetAccountId,
            ContractId = dto.ContractId,
            DocNumber = docNumber,
            DocDate = dto.DocDate,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
            Comment = dto.Comment,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
    }
}
