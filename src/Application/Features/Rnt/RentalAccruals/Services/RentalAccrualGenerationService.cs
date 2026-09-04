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

    public Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(int year, int month, int? organizationId, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(GenerateDueAsync), async () =>
        {
            await _postingLock.AcquireAsync(DocumentTypeIdConst.RENTAL_ACCRUAL, 0, ct);
            var accrualMonth = RentalAccrualSchedule.GetMonth(year, month);
            var query = _queryBuilder.For<RentalContractObject>()
                .IgnoreQueryFilters()
                .Where(x => x.StateId == StateIdConst.ACTIVE &&
                             x.Contract.StateId == StateIdConst.ACTIVE &&
                             (x.Contract.StatusId == DocumentStatusIdConst.POSTED ||
                              (x.Contract.StatusId == DocumentStatusIdConst.CANCELLED &&
                               x.Contract.ConfirmationDate.HasValue &&
                               x.Contract.TerminationDate.HasValue)) &&
                             !x.Contract.IsFreeOfCharge &&
                             x.StartDate <= accrualMonth.EndDate &&
                             x.Contract.StartDate <= accrualMonth.EndDate &&
                            (!x.EndDate.HasValue || x.EndDate.Value >= accrualMonth.StartDate) &&
                            (!x.Contract.EndDate.HasValue || x.Contract.EndDate.Value >= accrualMonth.StartDate) &&
                            (!x.Contract.TerminationDate.HasValue || x.Contract.TerminationDate.Value >= accrualMonth.StartDate) &&
                             (!organizationId.HasValue || x.Contract.OrganizationId == organizationId.Value))
                .OrderBy(items => items.OrderBy(x => x.Contract.OrganizationId).ThenBy(x => x.ContractId).ThenBy(x => x.Id))
                .Build();
            query.AddIncludes(x => x.Include(o => o.Contract));
            var dueObjects = await _objectQuery.GetAllAsync(query, ct);
            var dueObjectIds = dueObjects.Select(x => x.Id).ToArray();
            var existingObjectIds = new HashSet<long>();
            if (dueObjectIds.Length > 0)
            {
                var periodQuery = _queryBuilder.For<RentalAccrualDocItem>()
                    .Where(x => dueObjectIds.Contains(x.ContractObjectId) &&
                                x.PeriodFrom <= accrualMonth.EndDate &&
                                x.PeriodTo >= accrualMonth.StartDate)
                    .As(x => x.ContractObjectId)
                    .Build();
                existingObjectIds = (await _itemQuery.GetAllAsync(periodQuery, ct)).ToHashSet();
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
                            if (existingObjectIds.Contains(contractObject.Id))
                                continue;

                            var accrualLimit = RentalAccrualSchedule.GetAccrualLimit(
                                contractObject.Contract.EndDate,
                                contractObject.EndDate,
                                contractObject.Contract.TerminationDate);
                            var accrualStart = contractObject.StartDate.Date >= contractObject.Contract.StartDate.Date
                                ? contractObject.StartDate.Date
                                : contractObject.Contract.StartDate.Date;
                            var period = RentalAccrualSchedule.GetPeriodForMonth(
                                accrualMonth,
                                contractObject.PeriodUnit,
                                accrualStart,
                                accrualLimit);
                            if (!period.HasValue)
                                continue;

                            sources.Add(new RentalAccrualDraftSource(contractObject, period.Value));
                            if (period.Value.NextAccrualDate > contractObject.NextAccrualDate)
                                contractObject.NextAccrualDate = period.Value.NextAccrualDate;
                            contractObject.UpdatedDate = DateTime.Now;
                        }

                        if (sources.Count > 0)
                        {
                            var contract = contractGroup.First().Contract;
                            var number = await _documentNumberService.GetNextAsync(
                                contract.OrganizationId,
                                DocumentTypeIdConst.RENTAL_ACCRUAL,
                                accrualMonth.EndDate,
                                ct);
                            if (!number.IsSuccess)
                                return Result.Failure<RentalAccrualGenerationResult>(number.Error);

                            var document = RentalAccrualDraftFactory.Create(
                                contract,
                                number.Value.DocumentNumber,
                                accrualMonth.EndDate,
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
