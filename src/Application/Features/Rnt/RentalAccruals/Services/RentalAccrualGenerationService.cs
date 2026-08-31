using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.DocumentNumbers;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualGenerationService : BaseService, IRentalAccrualGenerationService
{
    private readonly IUserContext _userContext;
    private readonly IBackgroundOrganizationScope _backgroundOrganizationScope;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<RentalContractObject> _objectQuery;
    private readonly IQueryRepository<RentalAccrualDocItem> _itemQuery;
    private readonly ICommandRepository<RentalAccrualDoc> _documentCommand;
    private readonly ICommandRepository<RentalContractObject> _objectCommand;

    public RentalAccrualGenerationService(
        IUserContext userContext,
        IBackgroundOrganizationScope backgroundOrganizationScope,
        IQueryBuilder queryBuilder,
        IDocumentNumberService documentNumberService,
        IDocumentPostingLock postingLock,
        IAuditLogService auditLogService,
        IQueryRepository<RentalContractObject> objectQuery,
        IQueryRepository<RentalAccrualDocItem> itemQuery,
        ICommandRepository<RentalAccrualDoc> documentCommand,
        ICommandRepository<RentalContractObject> objectCommand,
        ILogger<RentalAccrualGenerationService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _backgroundOrganizationScope = backgroundOrganizationScope;
        _queryBuilder = queryBuilder;
        _documentNumberService = documentNumberService;
        _postingLock = postingLock;
        _auditLogService = auditLogService;
        _objectQuery = objectQuery;
        _itemQuery = itemQuery;
        _documentCommand = documentCommand;
        _objectCommand = objectCommand;
    }

    public Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(DateTime asOfDate, int? organizationId, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(GenerateDueAsync), async () =>
        {
            await _postingLock.AcquireAsync(DocumentTypeIdConst.RENTAL_ACCRUAL, 0, ct);
            var date = asOfDate.Date;
            var query = _queryBuilder.For<RentalContractObject>()
                .IgnoreQueryFilters()
                .Where(x => x.StateId == StateIdConst.ACTIVE &&
                            x.Contract.StateId == StateIdConst.ACTIVE &&
                            x.Contract.StatusId == DocumentStatusIdConst.POSTED &&
                            x.NextAccrualDate <= date &&
                            x.NextAccrualDate <= x.EndDate &&
                            (!organizationId.HasValue || x.Contract.OrganizationId == organizationId.Value))
                .OrderBy(items => items.OrderBy(x => x.Contract.OrganizationId).ThenBy(x => x.ContractId).ThenBy(x => x.Id))
                .Build();
            query.AddIncludes(x => x.Include(o => o.Contract));
            var dueObjects = await _objectQuery.GetAllAsync(query, ct);
            var dueObjectIds = dueObjects.Select(x => x.Id).ToArray();
            var existingPeriods = new HashSet<(long ObjectId, DateTime PeriodFrom, DateTime PeriodTo)>();
            if (dueObjectIds.Length > 0)
            {
                var periodQuery = _queryBuilder.For<RentalAccrualDocItem>()
                    .Where(x => dueObjectIds.Contains(x.ContractObjectId))
                    .As(x => new ExistingRentalAccrualPeriod(x.ContractObjectId, x.PeriodFrom, x.PeriodTo))
                    .Build();
                existingPeriods = (await _itemQuery.GetAllAsync(periodQuery, ct))
                    .Select(x => (x.ContractObjectId, x.PeriodFrom.Date, x.PeriodTo.Date))
                    .ToHashSet();
            }

            var documentIds = new List<long>();
            var itemCount = 0;
            foreach (var organizationGroup in dueObjects.GroupBy(x => x.Contract.OrganizationId))
            {
                IDisposable? lease = null;
                if (!_backgroundOrganizationScope.IsActive)
                    lease = _backgroundOrganizationScope.Enter(organizationGroup.Key, "rental-accrual-generation");
                else if (_backgroundOrganizationScope.OrganizationId != organizationGroup.Key)
                    return Result.Failure<RentalAccrualGenerationResult>(RentalAccrualErrors.OrganizationScopeMismatch(_userContext.LanguageId));

                using (lease)
                {
                    foreach (var contractGroup in organizationGroup.GroupBy(x => x.ContractId))
                    {
                        var sources = new List<RentalAccrualDraftSource>();
                        foreach (var contractObject in contractGroup)
                        {
                            var cursor = contractObject.NextAccrualDate.Date;
                            while (cursor <= date && cursor <= contractObject.EndDate.Date)
                            {
                                var period = RentalAccrualSchedule.GetPeriod(
                                    cursor,
                                    contractObject.PeriodUnit,
                                    contractObject.PeriodValue,
                                    contractObject.EndDate.Date);
                                var periodKey = (contractObject.Id, period.PeriodFrom.Date, period.PeriodTo.Date);
                                if (existingPeriods.Add(periodKey))
                                    sources.Add(new RentalAccrualDraftSource(contractObject, period));
                                cursor = period.NextAccrualDate;
                            }
                            contractObject.NextAccrualDate = cursor;
                            contractObject.UpdatedDate = DateTime.Now;
                        }

                        if (sources.Count > 0)
                        {
                            var contract = contractGroup.First().Contract;
                            var number = await _documentNumberService.GetNextAsync(
                                contract.OrganizationId,
                                DocumentTypeIdConst.RENTAL_ACCRUAL,
                                date,
                                ct);
                            if (!number.IsSuccess)
                                return Result.Failure<RentalAccrualGenerationResult>(number.Error);

                            var document = RentalAccrualDraftFactory.Create(
                                contract,
                                number.Value.DocumentNumber,
                                date,
                                sources,
                                organizationId.HasValue ? _userContext.Id : null);
                            await _documentCommand.CreateAsync(document, ct);
                            _auditLogService.SetNewValues(new
                            {
                                document.Id,
                                document.OrganizationId,
                                document.ContractId,
                                document.DocNumber,
                                document.DocDate,
                                document.ContractAmount,
                                document.TaxBaseAmount,
                                document.TaxAmount,
                                document.PayableAmount,
                                document.Amount,
                                document.StatusId,
                                Items = document.Items.Select(item => new
                                {
                                    item.Id,
                                    item.ContractObjectId,
                                    item.PeriodFrom,
                                    item.PeriodTo,
                                    item.ContractAmount,
                                    item.TaxBaseAmount,
                                    item.TaxRate,
                                    item.TaxAmount,
                                    item.PayableAmount,
                                    item.Amount
                                })
                            });
                            await _auditLogService.CreateAsync(
                                AuditLogTableConst.RentalAccrual,
                                document.Id.ToString(),
                                AuditLogOperationTypeConst.Create,
                                "Generated automatically from rental contract",
                                document.OrganizationId);
                            documentIds.Add(document.Id);
                            itemCount += document.Items.Count;
                        }

                        await _objectCommand.UpdateAsync(contractGroup, ct);
                    }
                }
            }

            return Result.Success(new RentalAccrualGenerationResult(documentIds.Count, itemCount, documentIds));
        }, ct);
}

internal readonly record struct ExistingRentalAccrualPeriod(
    long ContractObjectId,
    DateTime PeriodFrom,
    DateTime PeriodTo);
