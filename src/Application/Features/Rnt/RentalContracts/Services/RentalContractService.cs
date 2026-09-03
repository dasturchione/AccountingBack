using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalContractService : BaseService, IRentalContractService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<RentalContract> _query;
    private readonly ICommandRepository<RentalContract> _command;
    private readonly IQueryRepository<RentalAccrualDoc> _accrualQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<RentalObjectType> _typeQuery;
    private readonly IQueryRepository<ChartAccount> _accountQuery;

    public RentalContractService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<RentalContract> query,
        ICommandRepository<RentalContract> command,
        IQueryRepository<RentalAccrualDoc> accrualQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<RentalObjectType> typeQuery,
        IQueryRepository<ChartAccount> accountQuery,
        ILogger<RentalContractService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _accrualQuery = accrualQuery;
        _currencyQuery = currencyQuery;
        _typeQuery = typeQuery;
        _accountQuery = accountQuery;
    }

    public Task<Result<PagedResponse<RentalContractListDto>>> GetAllAsync(RentalContractListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PagedResponse<RentalContractListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.BuildPaged<RentalContract, RentalContractListDto, RentalContractListFilter>(filter);
            var page = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
        });

    public Task<Result<RentalContractDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoAsync(id, ct);
            return dto is null
                ? Result.Failure<RentalContractDto>(RentalContractErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(RentalContractCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var referenceValidation = await ValidateReferencesAsync(dto, requireAccounts: false, ct);
            if (!referenceValidation.IsSuccess)
                return Result.Failure<long>(referenceValidation.Error);
            var normalizedNumber = dto.ContractNumber.Trim();
            var contractYear = dto.ContractDate.Year;
            var organizationId = _userContext.OrganizationId.Value;
            if (await _query.AnyAsync(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                                         x.ContractNumber == normalizedNumber && x.ContractDate.Year == contractYear, ct))
                return Result.Failure<long>(RentalContractErrors.DuplicateNumber(normalizedNumber, contractYear, _userContext.LanguageId));

            var entity = new RentalContract
            {
                OrganizationId = organizationId,
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                CreatedByUserId = _userContext.Id
            };
            ApplyHeader(dto, entity);
            foreach (var item in dto.Objects)
                entity.Objects.Add(CreateObject(item));

            await _command.CreateAsync(entity, ct);
            var created = await GetDtoAsync(entity.Id, ct);
            if (created is not null)
            {
                _auditLogService.SetNewValues(created);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalContract, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            }
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, RentalContractUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeObjects: true, ct);
            if (entity is null)
                return Result.Failure(RentalContractErrors.NotFound(id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalContractErrors.InvalidStatus(id, entity.StatusId, _userContext.LanguageId));

            var referenceValidation = await ValidateReferencesAsync(dto, requireAccounts: false, ct);
            if (!referenceValidation.IsSuccess)
                return referenceValidation;
            var normalizedNumber = dto.ContractNumber.Trim();
            var contractYear = dto.ContractDate.Year;
            if (await _query.AnyAsync(x => x.Id != id && x.OrganizationId == entity.OrganizationId && x.StateId == StateIdConst.ACTIVE &&
                                         x.ContractNumber == normalizedNumber && x.ContractDate.Year == contractYear, ct))
                return Result.Failure(RentalContractErrors.DuplicateNumber(normalizedNumber, contractYear, _userContext.LanguageId));

            var requestedIds = dto.Objects.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
            if (requestedIds.Any(idValue => entity.Objects.All(x => x.Id != idValue)))
                return Result.Failure(RentalContractErrors.InvalidReference(_userContext.LanguageId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);

            ApplyHeader(dto, entity);
            foreach (var existing in entity.Objects.Where(x => x.StateId == StateIdConst.ACTIVE && !requestedIds.Contains(x.Id)))
            {
                existing.StateId = StateIdConst.PASSIVE;
                existing.UpdatedDate = DateTime.Now;
            }

            foreach (var item in dto.Objects)
            {
                if (item.Id.HasValue)
                {
                    var existing = entity.Objects.Single(x => x.Id == item.Id.Value);
                    ApplyObject(item, existing, preserveNextAccrualDate: false);
                }
                else
                {
                    entity.Objects.Add(CreateObject(item));
                }
            }

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);

            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalContract, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeObjects: false, ct);
            if (entity is null)
                return Result.Failure(RentalContractErrors.NotFound(id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalContractErrors.InvalidStatus(id, entity.StatusId, _userContext.LanguageId));
            if (await _accrualQuery.AnyAsync(x => x.ContractId == id && x.StateId == StateIdConst.ACTIVE, ct))
                return Result.Failure(RentalContractErrors.ExistingAccruals(id, _userContext.LanguageId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);
            entity.StateId = StateIdConst.PASSIVE;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.RentalContract, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result> ActivateAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ActivateAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeObjects: true, ct);
            if (entity is null)
                return Result.Failure(RentalContractErrors.NotFound(id, _userContext.LanguageId));
            if (entity.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalContractErrors.InvalidStatus(id, entity.StatusId, _userContext.LanguageId));
            if (!entity.LessorPayableAccountId.HasValue || !entity.TaxPayableAccountId.HasValue ||
                entity.Objects.Where(x => x.StateId == StateIdConst.ACTIVE).Any(x => !x.ExpenseAccountId.HasValue))
                return Result.Failure(RentalContractErrors.MissingAccounts(_userContext.LanguageId));

            var activationDto = ToValidationDto(entity);
            var references = await ValidateReferencesAsync(activationDto, requireAccounts: true, ct);
            if (!references.IsSuccess)
                return references;

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);
            entity.StatusId = DocumentStatusIdConst.POSTED;
            entity.PostedAt = DateTime.Now;
            entity.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalContract, id.ToString(), AuditLogOperationTypeConst.Update, "Activated");
            }
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeObjects: false, ct);
            if (entity is null)
                return Result.Failure(RentalContractErrors.NotFound(id, _userContext.LanguageId));
            if (entity.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (entity.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.POSTED))
                return Result.Failure(RentalContractErrors.InvalidStatus(id, entity.StatusId, _userContext.LanguageId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);
            entity.StatusId = DocumentStatusIdConst.CANCELLED;
            entity.CancelledAt = DateTime.Now;
            entity.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalContract, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }
            return Result.Success();
        }, ct);

    private async Task<Result> ValidateReferencesAsync(RentalContractBaseDto dto, bool requireAccounts, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!await _currencyQuery.AnyAsync(x => x.Id == dto.CurrencyId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(RentalContractErrors.InvalidReference(_userContext.LanguageId));

        var typeIds = dto.Objects.Select(x => x.RentalObjectTypeId).Distinct().ToArray();
        var typeQuery = _queryBuilder.For<RentalObjectType>()
            .Where(x => typeIds.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
            .As(x => x.Id)
            .Build();
        if ((await _typeQuery.GetAllAsync(typeQuery, ct)).Count != typeIds.Length)
            return Result.Failure(RentalContractErrors.InvalidReference(_userContext.LanguageId));

        var accountIds = dto.Objects.Select(x => x.ExpenseAccountId)
            .Append(dto.LessorPayableAccountId)
            .Append(dto.TaxPayableAccountId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        if (requireAccounts && (!dto.LessorPayableAccountId.HasValue || !dto.TaxPayableAccountId.HasValue ||
                                dto.Objects.Any(x => !x.ExpenseAccountId.HasValue)))
            return Result.Failure(RentalContractErrors.MissingAccounts(_userContext.LanguageId));
        if (requireAccounts && (dto.LessorPayableAccountId == dto.TaxPayableAccountId ||
                                dto.Objects.Any(x => x.ExpenseAccountId == dto.LessorPayableAccountId ||
                                                     x.ExpenseAccountId == dto.TaxPayableAccountId)))
            return Result.Failure(RentalContractErrors.InvalidReference(_userContext.LanguageId));

        if (accountIds.Length > 0)
        {
            var organizationId = _userContext.OrganizationId.Value;
            var accountQuery = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id)
                .Build();
            if ((await _accountQuery.GetAllAsync(accountQuery, ct)).Count != accountIds.Length)
                return Result.Failure(RentalContractErrors.InvalidReference(_userContext.LanguageId));
        }
        return Result.Success();
    }

    private async Task<RentalContract?> GetEntityAsync(long id, bool includeObjects, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;
        var organizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<RentalContract>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        if (includeObjects)
            query.AddIncludes(x => x.Include(c => c.Objects));
        return await _query.GetAsync(query, ct);
    }

    private async Task<RentalContractDto?> GetDtoAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;
        var organizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<RentalContract>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .As<RentalContractDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private static void ApplyHeader(RentalContractBaseDto dto, RentalContract entity)
    {
        entity.LessorFullName = dto.LessorFullName.Trim();
        entity.LessorInn = NullIfWhiteSpace(dto.LessorInn);
        entity.LessorPinfl = NullIfWhiteSpace(dto.LessorPinfl);
        entity.ContractNumber = dto.ContractNumber.Trim();
        entity.ContractDate = dto.ContractDate.Date;
        entity.StartDate = dto.StartDate.Date;
        entity.EndDate = dto.EndDate.Date;
        entity.CurrencyId = dto.CurrencyId;
        entity.LessorPayableAccountId = dto.LessorPayableAccountId;
        entity.TaxPayableAccountId = dto.TaxPayableAccountId;
        entity.Comment = NullIfWhiteSpace(dto.Comment);
    }

    private static RentalContractObject CreateObject(RentalContractObjectInputDto dto)
    {
        var entity = new RentalContractObject
        {
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        ApplyObject(dto, entity, preserveNextAccrualDate: false);
        return entity;
    }

    private static void ApplyObject(RentalContractObjectInputDto dto, RentalContractObject entity, bool preserveNextAccrualDate)
    {
        entity.RentalObjectTypeId = dto.RentalObjectTypeId;
        entity.ObjectName = dto.ObjectName.Trim();
        entity.ObjectIdentifier = NullIfWhiteSpace(dto.ObjectIdentifier);
        entity.ObjectAddress = NullIfWhiteSpace(dto.ObjectAddress);
        entity.StartDate = dto.StartDate.Date;
        entity.EndDate = dto.EndDate.Date;
        entity.PeriodUnit = dto.PeriodUnit.ToUpperInvariant();
        entity.PeriodValue = dto.PeriodValue;
        if (!preserveNextAccrualDate)
            entity.NextAccrualDate = dto.StartDate.Date;
        entity.ContractAmount = dto.ContractAmount;
        entity.TaxBaseAmount = dto.TaxBaseAmount;
        entity.TaxRate = dto.TaxRate;
        entity.ExpenseAccountId = dto.ExpenseAccountId;
        entity.StateId = StateIdConst.ACTIVE;
        entity.UpdatedDate = entity.Id == 0 ? null : DateTime.Now;
    }

    private static RentalContractUpdateDto ToValidationDto(RentalContract entity) => new()
    {
        LessorFullName = entity.LessorFullName,
        LessorInn = entity.LessorInn,
        LessorPinfl = entity.LessorPinfl,
        ContractNumber = entity.ContractNumber,
        ContractDate = entity.ContractDate,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        CurrencyId = entity.CurrencyId,
        LessorPayableAccountId = entity.LessorPayableAccountId,
        TaxPayableAccountId = entity.TaxPayableAccountId,
        Objects = entity.Objects.Where(x => x.StateId == StateIdConst.ACTIVE).Select(x => new RentalContractObjectInputDto
        {
            Id = x.Id,
            RentalObjectTypeId = x.RentalObjectTypeId,
            ObjectName = x.ObjectName,
            ObjectIdentifier = x.ObjectIdentifier,
            ObjectAddress = x.ObjectAddress,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            PeriodUnit = x.PeriodUnit,
            PeriodValue = x.PeriodValue,
            ContractAmount = x.ContractAmount,
            TaxBaseAmount = x.TaxBaseAmount,
            TaxRate = x.TaxRate,
            ExpenseAccountId = x.ExpenseAccountId
        }).ToList()
    };

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
