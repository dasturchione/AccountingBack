using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.Rnt.RentalAccruals;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using SharedKernel.Time;

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
    private readonly IQueryRepository<UtilityService> _utilityServiceQuery;
    private readonly IQueryRepository<ChartAccount> _accountQuery;
    private readonly IQueryRepository<RentalLessor> _lessorQuery;
    private readonly ICommandRepository<RentalLessor> _lessorCommand;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly ICommandRepository<CounterpartyCard> _counterpartyCommand;

    public RentalContractService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<RentalContract> query,
        ICommandRepository<RentalContract> command,
        IQueryRepository<RentalAccrualDoc> accrualQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<RentalObjectType> typeQuery,
        IQueryRepository<UtilityService> utilityServiceQuery,
        IQueryRepository<ChartAccount> accountQuery,
        IQueryRepository<RentalLessor> lessorQuery,
        ICommandRepository<RentalLessor> lessorCommand,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        ICommandRepository<CounterpartyCard> counterpartyCommand,
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
        _utilityServiceQuery = utilityServiceQuery;
        _accountQuery = accountQuery;
        _lessorQuery = lessorQuery;
        _lessorCommand = lessorCommand;
        _counterpartyQuery = counterpartyQuery;
        _counterpartyCommand = counterpartyCommand;
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

            var lessorResult = await ResolveLessorsAsync(dto.Lessors, organizationId, ct);
            if (!lessorResult.IsSuccess)
                return Result.Failure<long>(lessorResult.Error);

            var entity = new RentalContract
            {
                OrganizationId = organizationId,
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                CreatedByUserId = _userContext.Id
            };
            ApplyHeader(dto, entity);
            SyncLessors(entity, lessorResult.Value);
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

            var lessorResult = await ResolveLessorsAsync(dto.Lessors, entity.OrganizationId, ct);
            if (!lessorResult.IsSuccess)
                return Result.Failure(lessorResult.Error);

            ApplyHeader(dto, entity);
            SyncLessors(entity, lessorResult.Value);
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

    public Task<Result> ActivateAsync(long id, DateTime? confirmationDate = null, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ActivateAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeObjects: true, ct);
            if (entity is null)
                return Result.Failure(RentalContractErrors.NotFound(id, _userContext.LanguageId));
            if (entity.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalContractErrors.InvalidStatus(id, entity.StatusId, _userContext.LanguageId));
            var effectiveConfirmationDate = (confirmationDate ?? TashkentTime.Today).Date;
            if (effectiveConfirmationDate < entity.ContractDate.Date || effectiveConfirmationDate > TashkentTime.Today)
                return Result.Failure(RentalContractErrors.InvalidConfirmationDate(effectiveConfirmationDate, _userContext.LanguageId));
            if (!entity.IsFreeOfCharge &&
                (!entity.LessorPayableAccountId.HasValue || !entity.TaxPayableAccountId.HasValue ||
                 entity.Objects.Where(x => x.StateId == StateIdConst.ACTIVE).Any(x => !x.ExpenseAccountId.HasValue)))
                return Result.Failure(RentalContractErrors.MissingAccounts(_userContext.LanguageId));

            var activationDto = ToValidationDto(entity);
            var references = await ValidateReferencesAsync(activationDto, requireAccounts: !entity.IsFreeOfCharge, ct);
            if (!references.IsSuccess)
                return references;

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);
            entity.StatusId = DocumentStatusIdConst.POSTED;
            entity.ConfirmationDate = effectiveConfirmationDate;
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

    public Task<Result> CancelAsync(long id, DateTime? terminationDate = null, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeObjects: false, ct);
            if (entity is null)
                return Result.Failure(RentalContractErrors.NotFound(id, _userContext.LanguageId));
            if (entity.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (entity.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.POSTED))
                return Result.Failure(RentalContractErrors.InvalidStatus(id, entity.StatusId, _userContext.LanguageId));

            DateTime? effectiveTerminationDate = null;
            if (entity.StatusId == DocumentStatusIdConst.POSTED)
            {
                effectiveTerminationDate = (terminationDate ?? TashkentTime.Today).Date;
                if (effectiveTerminationDate < entity.StartDate.Date || effectiveTerminationDate > TashkentTime.Today)
                    return Result.Failure(RentalContractErrors.InvalidTerminationDate(effectiveTerminationDate.Value, entity.StartDate, _userContext.LanguageId));
            }

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);
            if (entity.StatusId == DocumentStatusIdConst.POSTED && !entity.ConfirmationDate.HasValue)
                entity.ConfirmationDate = entity.PostedAt?.Date ?? entity.ContractDate.Date;
            entity.StatusId = DocumentStatusIdConst.CANCELLED;
            if (entity.ConfirmationDate.HasValue && effectiveTerminationDate.HasValue)
                entity.TerminationDate = effectiveTerminationDate;
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

        var utilityServiceIds = dto.Objects
            .SelectMany(x => x.Utilities)
            .Select(x => x.UtilityServiceId)
            .Distinct()
            .ToArray();
        if (utilityServiceIds.Length > 0)
        {
            var utilityQuery = _queryBuilder.For<UtilityService>()
                .Where(x => utilityServiceIds.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id)
                .Build();
            if ((await _utilityServiceQuery.GetAllAsync(utilityQuery, ct)).Count != utilityServiceIds.Length)
                return Result.Failure(RentalContractErrors.InvalidReference(_userContext.LanguageId));
        }

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
        {
            query.AddIncludes(x => x.Include(c => c.Objects).ThenInclude(o => o.Utilities));
            query.AddIncludes(x => x.Include(c => c.Lessors).ThenInclude(link => link.Lessor));
        }
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
        var dto = await _query.GetAsync(query, ct);
        if (dto is null)
            return null;

        foreach (var contractObject in dto.Objects)
        {
            var totals = RentalAccrualSchedule.CalculateContractTotals(
                contractObject.PeriodAmount,
                contractObject.TaxBaseAmount,
                contractObject.TaxRate,
                contractObject.PeriodUnit,
                contractObject.StartDate,
                dto.EndDate,
                contractObject.EndDate,
                dto.TerminationDate);
            contractObject.ContractAmount = totals.ContractAmount;
            contractObject.ContractTaxBaseAmount = totals.ContractTaxBaseAmount;
            contractObject.ContractTaxAmount = totals.ContractTaxAmount;
        }

        return dto;
    }

    private static void ApplyHeader(RentalContractBaseDto dto, RentalContract entity)
    {
        entity.IsFreeOfCharge = dto.IsFreeOfCharge;
        entity.ContractNumber = dto.ContractNumber.Trim();
        entity.ContractDate = dto.ContractDate.Date;
        entity.StartDate = dto.StartDate.Date;
        entity.EndDate = dto.EndDate?.Date;
        entity.CurrencyId = dto.CurrencyId;
        entity.LessorPayableAccountId = dto.LessorPayableAccountId;
        entity.TaxPayableAccountId = dto.TaxPayableAccountId;
        entity.Comment = NullIfWhiteSpace(dto.Comment);
    }

    private async Task<Result<List<RentalLessor>>> ResolveLessorsAsync(
        IReadOnlyCollection<RentalLessorInputDto> items,
        int organizationId,
        CancellationToken ct)
    {
        var result = new List<RentalLessor>(items.Count);
        foreach (var item in items)
        {
            var inn = NullIfWhiteSpace(item.Inn);
            var pinfl = NullIfWhiteSpace(item.Pinfl);
            var kindCode = item.LessorKindCode.Trim().ToUpperInvariant();
            if (inn is null && pinfl is null)
                return Result.Failure<List<RentalLessor>>(RentalContractErrors.InvalidLessor(_userContext.LanguageId));

            RentalLessor? lessor = null;
            if (pinfl is not null)
            {
                var pinflQuery = _queryBuilder.For<RentalLessor>()
                    .Where(x => x.OrganizationId == organizationId &&
                                x.StateId == StateIdConst.ACTIVE &&
                                x.Pinfl != null && x.Pinfl.Trim() == pinfl)
                    .OrderBy(x => x.Id)
                    .Build();
                lessor = await _lessorQuery.GetAsync(pinflQuery, ct);
            }

            if (lessor is null && inn is not null)
            {
                var innQuery = _queryBuilder.For<RentalLessor>()
                    .Where(x => x.OrganizationId == organizationId &&
                                x.StateId == StateIdConst.ACTIVE &&
                                x.Inn != null && x.Inn.Trim() == inn)
                    .OrderBy(x => x.Id)
                    .Build();
                lessor = await _lessorQuery.GetAsync(innQuery, ct);
            }

            int? counterpartyId = null;
            if (kindCode == RentalLessorKindConst.LegalEntity)
            {
                if (inn is null)
                    return Result.Failure<List<RentalLessor>>(RentalContractErrors.InvalidLessor(_userContext.LanguageId));
                counterpartyId = (await ResolveLegalCounterpartyAsync(item, inn, organizationId, ct)).Id;
            }

            if (lessor is null)
            {
                lessor = new RentalLessor
                {
                    OrganizationId = organizationId,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                };
                ApplyLessor(item, lessor, kindCode, inn, pinfl, counterpartyId);
                await _lessorCommand.CreateAsync(lessor, ct);
            }
            else
            {
                ApplyLessor(item, lessor, kindCode, inn, pinfl, counterpartyId);
                lessor.UpdatedDate = DateTime.Now;
                await _lessorCommand.UpdateAsync(lessor, ct);
            }

            result.Add(lessor);
        }

        return Result.Success(result);
    }

    private async Task<CounterpartyCard> ResolveLegalCounterpartyAsync(
        RentalLessorInputDto item,
        string inn,
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.Inn != null && x.Inn.Trim() == inn)
            .OrderBy(x => x.Id)
            .Build();
        var counterparty = await _counterpartyQuery.GetAsync(query, ct);
        if (counterparty is not null)
            return counterparty;

        var fullName = item.FullName.Trim();
        counterparty = new CounterpartyCard
        {
            OrganizationId = organizationId,
            ShortName = fullName.Length <= 250 ? fullName : fullName[..250],
            FullName = fullName,
            Inn = inn,
            PhoneNumber = NullIfWhiteSpace(item.PhoneNumber),
            Address = NullIfWhiteSpace(item.RegisteredAddress),
            IsVatPayer = false,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _counterpartyCommand.CreateAsync(counterparty, ct);
        return counterparty;
    }

    private static void ApplyLessor(
        RentalLessorInputDto dto,
        RentalLessor entity,
        string kindCode,
        string? inn,
        string? pinfl,
        int? counterpartyId)
    {
        entity.LessorKindCode = kindCode;
        entity.CounterpartyId = counterpartyId;
        entity.FullName = dto.FullName.Trim();
        entity.Inn = inn;
        entity.Pinfl = pinfl;
        entity.PhoneNumber = NullIfWhiteSpace(dto.PhoneNumber);
        entity.RegisteredAddress = NullIfWhiteSpace(dto.RegisteredAddress);
        entity.ResidentialAddress = NullIfWhiteSpace(dto.ResidentialAddress);
    }

    private static void SyncLessors(RentalContract entity, IReadOnlyCollection<RentalLessor> lessors)
    {
        var requestedIds = lessors.Select(x => x.Id).ToHashSet();
        foreach (var existing in entity.Lessors.Where(x => !requestedIds.Contains(x.LessorId)).ToList())
            entity.Lessors.Remove(existing);

        var existingIds = entity.Lessors.Select(x => x.LessorId).ToHashSet();
        foreach (var lessor in lessors.Where(x => !existingIds.Contains(x.Id)))
            entity.Lessors.Add(new RentalContractLessor { LessorId = lessor.Id });
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
        entity.TotalArea = dto.TotalArea;
        entity.RentedArea = dto.RentedArea;
        entity.StartDate = dto.StartDate.Date;
        entity.EndDate = dto.EndDate?.Date;
        entity.PeriodUnit = dto.PeriodUnit.ToUpperInvariant();
        if (!preserveNextAccrualDate)
            entity.NextAccrualDate = dto.StartDate.Date;
        entity.PeriodAmount = dto.PeriodAmount;
        entity.TaxBaseAmount = dto.TaxBaseAmount;
        entity.TaxRate = dto.TaxRate;
        entity.ExpenseAccountId = dto.ExpenseAccountId;
        entity.StateId = StateIdConst.ACTIVE;
        entity.UpdatedDate = entity.Id == 0 ? null : DateTime.Now;
        SyncUtilities(entity, dto.Utilities);
    }

    private static void SyncUtilities(
        RentalContractObject entity,
        IReadOnlyCollection<RentalContractObjectUtilityInputDto> utilities)
    {
        var requestedIds = utilities.Select(x => x.UtilityServiceId).ToHashSet();
        foreach (var existing in entity.Utilities.Where(x => !requestedIds.Contains(x.UtilityServiceId)).ToList())
            entity.Utilities.Remove(existing);

        foreach (var item in utilities)
        {
            var existing = entity.Utilities.SingleOrDefault(x => x.UtilityServiceId == item.UtilityServiceId);
            if (existing is null)
            {
                entity.Utilities.Add(new RentalContractObjectUtility
                {
                    UtilityServiceId = item.UtilityServiceId,
                    PayerCode = item.PayerCode.Trim().ToUpperInvariant()
                });
            }
            else
            {
                existing.PayerCode = item.PayerCode.Trim().ToUpperInvariant();
            }
        }
    }

    private static RentalContractUpdateDto ToValidationDto(RentalContract entity) => new()
    {
        IsFreeOfCharge = entity.IsFreeOfCharge,
        ContractNumber = entity.ContractNumber,
        ContractDate = entity.ContractDate,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        CurrencyId = entity.CurrencyId,
        LessorPayableAccountId = entity.LessorPayableAccountId,
        TaxPayableAccountId = entity.TaxPayableAccountId,
        Lessors = entity.Lessors
            .Where(x => x.Lessor.StateId == StateIdConst.ACTIVE)
            .Select(x => new RentalLessorInputDto
            {
                LessorKindCode = x.Lessor.LessorKindCode,
                FullName = x.Lessor.FullName,
                Inn = x.Lessor.Inn,
                Pinfl = x.Lessor.Pinfl,
                PhoneNumber = x.Lessor.PhoneNumber,
                RegisteredAddress = x.Lessor.RegisteredAddress,
                ResidentialAddress = x.Lessor.ResidentialAddress
            }).ToList(),
        Objects = entity.Objects.Where(x => x.StateId == StateIdConst.ACTIVE).Select(x => new RentalContractObjectInputDto
        {
            Id = x.Id,
            RentalObjectTypeId = x.RentalObjectTypeId,
            ObjectName = x.ObjectName,
            ObjectIdentifier = x.ObjectIdentifier,
            ObjectAddress = x.ObjectAddress,
            TotalArea = x.TotalArea,
            RentedArea = x.RentedArea,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            PeriodUnit = x.PeriodUnit,
            PeriodAmount = x.PeriodAmount,
            TaxBaseAmount = x.TaxBaseAmount,
            TaxRate = x.TaxRate,
            ExpenseAccountId = x.ExpenseAccountId,
            Utilities = x.Utilities.Select(utility => new RentalContractObjectUtilityInputDto
            {
                UtilityServiceId = utility.UtilityServiceId,
                PayerCode = utility.PayerCode
            }).ToList()
        }).ToList()
    };

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
