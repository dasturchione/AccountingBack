using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.DocumentNumbers;
using Application.Features.InventoryCounts;
using Application.Features.InventoryMovements;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public class RetailSaleDocService : BaseService, IRetailSaleDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IActiveInventoryCountGuardService _inventoryCountGuard;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IAccountingDispatcher _accountingDispatcher;
    private readonly IQueryRepository<RetailSaleDoc> _documentQuery;
    private readonly ICommandRepository<RetailSaleDoc> _documentCommand;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<FiscalCashRegister> _cashRegisterQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly IQueryRepository<PaymentMethod> _paymentMethodQuery;
    private readonly IQueryRepository<PaymentAcceptancePoint> _paymentAcceptancePointQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly ICommandRepository<RetailSaleDocProduct> _lineCommand;
    private readonly ICommandRepository<RetailSaleDocTable> _tableCommand;
    private readonly ICommandRepository<RetailSaleDocPayment> _paymentCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _entryQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _entryCommand;
    private readonly IQueryRepository<WarehouseProductMovement> _movementQuery;

    public RetailSaleDocService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentNumberService documentNumberService,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IActiveInventoryCountGuardService inventoryCountGuard,
        IInventoryDispatcher inventoryDispatcher,
        IAccountingDispatcher accountingDispatcher,
        IQueryRepository<RetailSaleDoc> documentQuery,
        ICommandRepository<RetailSaleDoc> documentCommand,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<FiscalCashRegister> cashRegisterQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<VatRate> vatRateQuery,
        IQueryRepository<PaymentMethod> paymentMethodQuery,
        IQueryRepository<PaymentAcceptancePoint> paymentAcceptancePointQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        ICommandRepository<RetailSaleDocProduct> lineCommand,
        ICommandRepository<RetailSaleDocTable> tableCommand,
        ICommandRepository<RetailSaleDocPayment> paymentCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> entryQuery,
        ICommandRepository<AccountingRegisterEntry> entryCommand,
        IQueryRepository<WarehouseProductMovement> movementQuery,
        ILogger<RetailSaleDocService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _documentNumberService = documentNumberService;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _inventoryCountGuard = inventoryCountGuard;
        _inventoryDispatcher = inventoryDispatcher;
        _accountingDispatcher = accountingDispatcher;
        _documentQuery = documentQuery;
        _documentCommand = documentCommand;
        _warehouseQuery = warehouseQuery;
        _cashRegisterQuery = cashRegisterQuery;
        _counterpartyQuery = counterpartyQuery;
        _productQuery = productQuery;
        _productTableQuery = productTableQuery;
        _vatRateQuery = vatRateQuery;
        _paymentMethodQuery = paymentMethodQuery;
        _paymentAcceptancePointQuery = paymentAcceptancePointQuery;
        _chartAccountQuery = chartAccountQuery;
        _lineCommand = lineCommand;
        _tableCommand = tableCommand;
        _paymentCommand = paymentCommand;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _entryQuery = entryQuery;
        _entryCommand = entryCommand;
        _movementQuery = movementQuery;
    }

    public Task<Result<PagedResponse<RetailSaleDocListDto>>> GetAllAsync(RetailSaleDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var specification = _queryBuilder.BuildPaged<RetailSaleDoc, RetailSaleDocListDto, RetailSaleDocListFilter>(filter);
            var page = await _documentQuery.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
        });

    public Task<Result<RetailSaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var result = await GetByIdInternalAsync(id, ct);
            return result is null ? Result.Failure<RetailSaleDocDto>(RetailSaleDocErrors.NotFound(id)) : Result.Success(result);
        });

    public Task<Result<long>> CreateAsync(RetailSaleDocCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            var organizationResult = GetOrganizationId();
            if (!organizationResult.IsSuccess)
                return Result.Failure<long>(organizationResult.Error);

            var organizationId = organizationResult.Value;
            var header = await ValidateHeaderAsync(organizationId, dto.CounterpartyId, dto.WarehouseId, dto.CashRegisterId, dto.ReceivableAccountId, dto.VatAccountId, ct);
            if (!header.IsSuccess)
                return Result.Failure<long>(header.Error);

            var detailsResult = await BuildDetailsAsync(organizationId, dto.Lines, dto.Payments, ct);
            if (!detailsResult.IsSuccess)
                return Result.Failure<long>(detailsResult.Error);

            var date = dto.DocDate.HasValue ? DateTime.SpecifyKind(dto.DocDate.Value, DateTimeKind.Unspecified) : DateTime.Now;
            var number = await _documentNumberService.GetNextAsync(organizationId, DocumentTypeIdConst.RETAIL_SALE, date, ct);
            if (!number.IsSuccess)
                return Result.Failure<long>(number.Error);

            var details = detailsResult.Value;
            var document = new RetailSaleDoc
            {
                OrganizationId = organizationId,
                DocNumber = number.Value.DocumentNumber,
                DocDate = date,
                CounterpartyId = dto.CounterpartyId,
                WarehouseId = dto.WarehouseId,
                CashRegisterId = dto.CashRegisterId,
                CurrencyId = dto.CurrencyId,
                ExchangeRate = dto.ExchangeRate,
                ReceivableAccountId = dto.ReceivableAccountId,
                VatAccountId = dto.VatAccountId,
                TotalAmount = details.TotalAmount,
                VatAmount = details.VatAmount,
                FinalAmount = details.FinalAmount,
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                Comment = dto.Comment,
                CreatedDate = DateTime.Now
            };
            await _documentCommand.CreateAsync(document, ct);
            await PersistDetailsAsync(document.Id, details, ct);

            if (dto.ProcessingMode == RetailSaleProcessingMode.Immediate)
            {
                var confirmation = await ConfirmInternalAsync(document.Id, new RetailSaleDocConfirmDto(), ct);
                if (!confirmation.IsSuccess)
                    return Result.Failure<long>(confirmation.Error);
            }

            return Result.Success(document.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, RetailSaleDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var organizationResult = GetOrganizationId();
            if (!organizationResult.IsSuccess)
                return Result.Failure(organizationResult.Error);

            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(RetailSaleDocErrors.NotFound(id));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RetailSaleDocErrors.CannotUpdate(id, document.StatusId));

            var header = await ValidateHeaderAsync(organizationResult.Value, dto.CounterpartyId, dto.WarehouseId, dto.CashRegisterId, dto.ReceivableAccountId, dto.VatAccountId, ct);
            if (!header.IsSuccess)
                return header;

            var detailsResult = await BuildDetailsAsync(organizationResult.Value, dto.Lines, dto.Payments, ct);
            if (!detailsResult.IsSuccess)
                return Result.Failure(detailsResult.Error);

            var oldTables = document.RetailSaleDocProducts.SelectMany(x => x.RetailSaleDocTables).ToList();
            if (oldTables.Count > 0)
                await _tableCommand.DeleteAsync(oldTables, ct);
            if (document.RetailSaleDocProducts.Count > 0)
                await _lineCommand.DeleteAsync(document.RetailSaleDocProducts, ct);
            if (document.RetailSaleDocPayments.Count > 0)
                await _paymentCommand.DeleteAsync(document.RetailSaleDocPayments, ct);

            var details = detailsResult.Value;
            document.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            document.CounterpartyId = dto.CounterpartyId;
            document.WarehouseId = dto.WarehouseId;
            document.CashRegisterId = dto.CashRegisterId;
            document.CurrencyId = dto.CurrencyId;
            document.ExchangeRate = dto.ExchangeRate;
            document.ReceivableAccountId = dto.ReceivableAccountId;
            document.VatAccountId = dto.VatAccountId;
            document.TotalAmount = details.TotalAmount;
            document.VatAmount = details.VatAmount;
            document.FinalAmount = details.FinalAmount;
            document.StateId = dto.StateId;
            document.Comment = dto.Comment;
            await _documentCommand.UpdateAsync(document, ct);
            await PersistDetailsAsync(document.Id, details, ct);
            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, RetailSaleDocConfirmDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), () => ConfirmInternalAsync(id, dto, ct), ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (!GetOrganizationId().IsSuccess)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.RETAIL_SALE, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(RetailSaleDocErrors.NotFound(id));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.POSTED))
                return Result.Failure(RetailSaleDocErrors.CannotCancel(id, document.StatusId));

            await _postingLock.AcquireInventoryAsync(
                document.OrganizationId,
                document.WarehouseId,
                document.RetailSaleDocProducts.Where(line => !line.Product.IsService).Select(line => line.ProductId).ToArray(),
                GetDistinctPhysicalMarkingIds(document),
                ct);

            var period = await _periodValidator.EnsureOpenAsync(document.OrganizationId, DateTime.Now, ct);
            if (!period.IsSuccess)
                return period;

            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activeBatch = await GetActivePostingBatchAsync(id, ct);
                if (activeBatch is null)
                    return Result.Failure(RetailSaleDocErrors.MissingPostingBatch(id));

                var reverseBatch = await CreatePostingBatchAsync(document, PostingBatchStatusConst.REVERSAL, "Retail sale cancelled", ct);
                var accounting = await ReverseAccountingEntriesAsync(id, reverseBatch.Id, ct);
                if (!accounting.IsSuccess)
                    return accounting;
                var inventory = await _inventoryDispatcher.ReverseAsync(document, ct);
                if (!inventory.IsSuccess)
                    return inventory;

                activeBatch.Status = PostingBatchStatusConst.REVERSED;
                activeBatch.ReversedAt = DateTime.Now;
                activeBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activeBatch, ct);
            }

            document.StatusId = DocumentStatusIdConst.CANCELLED;
            document.CancelledAt = DateTime.Now;
            document.CancelledByUserId = _userContext.Id;
            await _documentCommand.UpdateAsync(document, ct);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(RetailSaleDocErrors.NotFound(id));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RetailSaleDocErrors.CannotUpdate(id, document.StatusId));
            await _documentCommand.DeleteAsync(document, ct);
            return Result.Success();
        }, ct);

    private async Task<Result> ConfirmInternalAsync(long id, RetailSaleDocConfirmDto dto, CancellationToken ct)
    {
        var organization = GetOrganizationId();
        if (!organization.IsSuccess)
            return Result.Failure(organization.Error);

        await _postingLock.AcquireAsync(DocumentTypeIdConst.RETAIL_SALE, id, ct);
        var document = await GetDocumentAsync(id, ct);
        if (document is null)
            return Result.Failure(RetailSaleDocErrors.NotFound(id));
        if (document.StatusId == DocumentStatusIdConst.POSTED)
            return await GetActivePostingBatchAsync(id, ct) is null
                ? Result.Failure(RetailSaleDocErrors.MissingPostingBatch(id))
                : Result.Success();
        if (document.StatusId != DocumentStatusIdConst.DRAFT)
            return Result.Failure(RetailSaleDocErrors.CannotConfirm(id, document.StatusId));

        await _postingLock.AcquireInventoryAsync(
            document.OrganizationId,
            document.WarehouseId,
            document.RetailSaleDocProducts.Where(line => !line.Product.IsService).Select(line => line.ProductId).ToArray(),
            GetDistinctPhysicalMarkingIds(document),
            ct);

        var period = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
        if (!period.IsSuccess)
            return period;
        var guard = await _inventoryCountGuard.EnsureWarehouseIsNotBlockedAsync(document.OrganizationId, document.WarehouseId, "RetailSaleConfirm", ct: ct);
        if (!guard.IsSuccess)
            return guard;
        if (await GetActivePostingBatchAsync(id, ct) is not null || await HasBusinessEffectsAsync(id, ct))
            return Result.Failure(RetailSaleDocErrors.BusinessEffectsExist(id));

        var amounts = await ApplyConfirmChangesAsync(document, dto, ct);
        if (!amounts.IsSuccess)
            return amounts;
        var valid = ValidateForConfirm(document);
        if (!valid.IsSuccess)
            return valid;

        var batch = await CreatePostingBatchAsync(document, PostingBatchStatusConst.POSTED, "Retail sale confirmed", ct);
        var inventory = await _inventoryDispatcher.ProcessAsync(document, ct, batch.Id);
        if (!inventory.IsSuccess)
            return Result.Failure(inventory.Error);
        var accounting = await _accountingDispatcher.ProcessAsync(document, ct, batch.Id);
        if (!accounting.IsSuccess)
            return Result.Failure(accounting.Error);

        document.StatusId = DocumentStatusIdConst.POSTED;
        document.PostedAt = DateTime.Now;
        document.PostedByUserId = _userContext.Id;
        await _documentCommand.UpdateAsync(document, ct);
        return Result.Success();
    }

    private async Task<Result<RetailSaleDetails>> BuildDetailsAsync(int organizationId, IReadOnlyCollection<RetailSaleDocProductCreateDto> dtoLines, IReadOnlyCollection<RetailSaleDocPaymentDto> dtoPayments, CancellationToken ct)
    {
        if (dtoLines.Count == 0)
            return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.EmptyLines());

        var productIds = dtoLines.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productQuery.GetAllAsync(_queryBuilder.For<Product>()
            .Where(x => productIds.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build(), ct);
        var productMap = products.ToDictionary(x => x.Id);
        if (productMap.Count != productIds.Count)
            return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.ProductNotFound(productIds.First(x => !productMap.ContainsKey(x))));

        var vatIds = dtoLines.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();
        var vatMap = vatIds.Count == 0
            ? new Dictionary<short, VatRate>()
            : (await _vatRateQuery.GetAllAsync(_queryBuilder.For<VatRate>().Where(x => vatIds.Contains(x.Id)).Build(), ct)).ToDictionary(x => x.Id);
        if (vatMap.Count != vatIds.Count)
            return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.InvalidLine(0));

        var tableIds = dtoLines.SelectMany(x => x.Items).Select(x => x.ProductTableId).ToList();
        var distinctTableIds = tableIds.Distinct().ToList();
        var tables = distinctTableIds.Count == 0 ? new List<ProductTable>() : await _productTableQuery.GetAllAsync(
            _queryBuilder.For<ProductTable>().Where(x => distinctTableIds.Contains(x.Id) && x.Product.OrganizationId == organizationId).Build(), ct);
        var tableMap = tables.ToDictionary(x => x.Id);
        if (tableMap.Count != distinctTableIds.Count)
            return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.InvalidProductTable(distinctTableIds.First(x => !tableMap.ContainsKey(x))));

        var lines = new List<RetailSaleLineDraft>();
        foreach (var dto in dtoLines)
        {
            var product = productMap[dto.ProductId];
            var isPieceTracked = !product.IsService && product.IsPieceTracked;
            if (dto.Quantity <= 0m || dto.UnitPrice < 0m || dto.CostPrice < 0m || dto.VatAmount is < 0m || dto.UnitId != product.UnitId ||
                (isPieceTracked && (dto.Quantity != decimal.Truncate(dto.Quantity) ||
                                    !SaleMarkingPolicy.IsOccurrenceCountAllowed(dto.Quantity, dto.Items.Count))) ||
                (!isPieceTracked && dto.Items.Count > 0))
            {
                return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.InvalidLine(dto.ProductId));
            }

            if (dto.Items.Any(x => !tableMap.ContainsKey(x.ProductTableId)))
                return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.InvalidProductTable(dto.Items.First().ProductTableId));

            var vatRate = dto.VatRateId.HasValue ? vatMap[dto.VatRateId.Value].Rate : (decimal?)null;
            var vatAmount = RetailSaleVatCalculator.ResolveTotal(dto.UnitPrice, dto.Quantity, dto.VatAmount, vatRate);
            var vatPerUnit = RetailSaleVatCalculator.ResolvePerUnit(vatAmount, dto.Quantity);
            var amount = dto.Quantity * dto.UnitPrice;
            var line = new RetailSaleDocProduct
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitId = dto.UnitId,
                UnitPrice = dto.UnitPrice,
                CostPrice = dto.CostPrice,
                Amount = amount,
                VatRateId = dto.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                InventoryAccountId = dto.InventoryAccountId,
                IncomeAccountId = dto.IncomeAccountId,
                CostAccountId = dto.CostAccountId
            };
            lines.Add(new RetailSaleLineDraft(line, dto.Items.Select(x => new RetailSaleDocTable
            {
                ProductTableId = x.ProductTableId,
                CostPrice = dto.CostPrice,
                Amount = dto.UnitPrice,
                VatRateId = dto.VatRateId,
                VatAmount = vatPerUnit,
                TotalAmount = dto.UnitPrice + vatPerUnit
            }).ToList()));
        }

        var paymentsResult = await BuildPaymentsAsync(organizationId, dtoPayments, ct);
        if (!paymentsResult.IsSuccess)
            return Result.Failure<RetailSaleDetails>(paymentsResult.Error);

        var details = new RetailSaleDetails(lines, paymentsResult.Value);
        if (details.FinalAmount != details.Payments.Sum(x => x.Amount))
            return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.PaymentTotalMismatch(details.FinalAmount, details.Payments.Sum(x => x.Amount)));

        var accountIds = lines.SelectMany(x => new[] { x.Line.InventoryAccountId, x.Line.IncomeAccountId, x.Line.CostAccountId })
            .Concat(details.Payments.Select(x => (int?)x.DebitAccountId))
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (accountIds.Count > 0)
        {
            var accounts = await _chartAccountQuery.GetAllAsync(_queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id).Build(), ct);
            if (accounts.Count != accountIds.Count)
                return Result.Failure<RetailSaleDetails>(RetailSaleDocErrors.AccountRequired());
        }

        return Result.Success(details);
    }

    private async Task<Result<List<RetailSaleDocPayment>>> BuildPaymentsAsync(int organizationId, IReadOnlyCollection<RetailSaleDocPaymentDto> dtoPayments, CancellationToken ct)
    {
        if (dtoPayments.Count == 0 || dtoPayments.Any(x => x.Amount <= 0m))
            return Result.Failure<List<RetailSaleDocPayment>>(RetailSaleDocErrors.InvalidPayment());

        var methodIds = dtoPayments.Select(x => x.PaymentMethodId).Distinct().ToList();
        var methods = await _paymentMethodQuery.GetAllAsync(_queryBuilder.For<PaymentMethod>()
            .Where(x => methodIds.Contains(x.Id)).As(x => x.Id).Build(), ct);
        if (methods.Count != methodIds.Count)
            return Result.Failure<List<RetailSaleDocPayment>>(RetailSaleDocErrors.InvalidPayment());

        var pointIds = dtoPayments.Where(x => x.PaymentAcceptancePointId.HasValue).Select(x => x.PaymentAcceptancePointId!.Value).Distinct().ToList();
        if (pointIds.Count > 0)
        {
            var points = await _paymentAcceptancePointQuery.GetAllAsync(_queryBuilder.For<PaymentAcceptancePoint>()
                .Where(x => pointIds.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id).Build(), ct);
            if (points.Count != pointIds.Count)
                return Result.Failure<List<RetailSaleDocPayment>>(RetailSaleDocErrors.InvalidPayment());
        }

        return Result.Success(dtoPayments.Select(x => new RetailSaleDocPayment
        {
            PaymentMethodId = x.PaymentMethodId,
            PaymentAcceptancePointId = x.PaymentAcceptancePointId,
            DebitAccountId = x.DebitAccountId,
            Amount = x.Amount,
            TransactionNumber = x.TransactionNumber
        }).ToList());
    }

    private async Task<Result> ValidateHeaderAsync(int organizationId, int? counterpartyId, int warehouseId, int cashRegisterId, int? receivableAccountId, int? vatAccountId, CancellationToken ct)
    {
        var warehouse = await _warehouseQuery.GetAsync(_queryBuilder.For<Warehouse>()
            .Where(x => x.Id == warehouseId && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE).Build(), ct);
        var register = await _cashRegisterQuery.GetAsync(_queryBuilder.For<FiscalCashRegister>()
            .Where(x => x.Id == cashRegisterId && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE).Build(), ct);
        if (warehouse is null || register is null || (register.WarehouseId.HasValue && register.WarehouseId.Value != warehouseId))
            return Result.Failure(RetailSaleDocErrors.InvalidLine(0));

        if (counterpartyId.HasValue)
        {
            var counterparty = await _counterpartyQuery.GetAsync(_queryBuilder.For<CounterpartyCard>()
                .Where(x => x.Id == counterpartyId.Value && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE).Build(), ct);
            if (counterparty is null)
                return Result.Failure(RetailSaleDocErrors.InvalidLine(0));
        }

        var accountIds = new[] { receivableAccountId, vatAccountId }.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (accountIds.Count > 0)
        {
            var accounts = await _chartAccountQuery.GetAllAsync(_queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id).Build(), ct);
            if (accounts.Count != accountIds.Count)
                return Result.Failure(RetailSaleDocErrors.AccountRequired());
        }
        return Result.Success();
    }

    private async Task PersistDetailsAsync(long documentId, RetailSaleDetails details, CancellationToken ct)
    {
        foreach (var item in details.Lines)
            item.Line.OwnerId = documentId;
        var lines = details.Lines.Select(x => x.Line).ToList();
        await _lineCommand.CreateAsync(lines, ct);

        var tables = details.Lines.SelectMany(x => x.Tables.Select(table =>
        {
            table.OwnerId = x.Line.Id;
            return table;
        })).ToList();
        if (tables.Count > 0)
            await _tableCommand.CreateAsync(tables, ct);

        foreach (var payment in details.Payments)
            payment.OwnerId = documentId;
        await _paymentCommand.CreateAsync(details.Payments, ct);
    }

    private async Task<Result> ApplyConfirmChangesAsync(RetailSaleDoc document, RetailSaleDocConfirmDto dto, CancellationToken ct)
    {
        if (dto.Lines.Count > 0)
        {
            var lineMap = document.RetailSaleDocProducts.ToDictionary(x => x.Id);
            var vatIds = document.RetailSaleDocProducts.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();
            var vatMap = vatIds.Count == 0 ? new Dictionary<short, VatRate>() :
                (await _vatRateQuery.GetAllAsync(_queryBuilder.For<VatRate>().Where(x => vatIds.Contains(x.Id)).Build(), ct)).ToDictionary(x => x.Id);

            foreach (var dtoLine in dto.Lines)
            {
                if (!lineMap.TryGetValue(dtoLine.Id, out var line) || dtoLine.UnitPrice < 0m ||
                    dtoLine.CostPrice < 0m || dtoLine.VatAmount is < 0m)
                    return Result.Failure(RetailSaleDocErrors.InvalidLine(0));

                var vatRate = line.VatRateId.HasValue ? vatMap[line.VatRateId.Value].Rate : (decimal?)null;
                var vatAmount = RetailSaleVatCalculator.ResolveTotal(dtoLine.UnitPrice, line.Quantity, dtoLine.VatAmount, vatRate);
                var vatPerUnit = RetailSaleVatCalculator.ResolvePerUnit(vatAmount, line.Quantity);
                line.UnitPrice = dtoLine.UnitPrice;
                line.CostPrice = dtoLine.CostPrice;
                if (line.Product.IsPieceTracked && !line.Product.IsService)
                {
                    foreach (var table in line.RetailSaleDocTables)
                    {
                        table.CostPrice = dtoLine.CostPrice;
                        table.Amount = dtoLine.UnitPrice;
                        table.VatAmount = vatPerUnit;
                        table.TotalAmount = dtoLine.UnitPrice + vatPerUnit;
                    }
                    if (line.RetailSaleDocTables.Count > 0)
                        await _tableCommand.UpdateAsync(line.RetailSaleDocTables, ct);
                }

                line.Amount = line.Quantity * dtoLine.UnitPrice;
                line.VatAmount = vatAmount;
                line.TotalAmount = line.Amount + line.VatAmount;
            }
            await _lineCommand.UpdateAsync(document.RetailSaleDocProducts, ct);
        }

        document.TotalAmount = document.RetailSaleDocProducts.Sum(x => x.Amount);
        document.VatAmount = document.RetailSaleDocProducts.Sum(x => x.VatAmount);
        document.FinalAmount = document.RetailSaleDocProducts.Sum(x => x.TotalAmount);

        if (dto.Payments is not null)
        {
            var payments = await BuildPaymentsAsync(document.OrganizationId, dto.Payments, ct);
            if (!payments.IsSuccess)
                return Result.Failure(payments.Error);
            if (payments.Value.Sum(x => x.Amount) != document.FinalAmount)
                return Result.Failure(RetailSaleDocErrors.PaymentTotalMismatch(document.FinalAmount, payments.Value.Sum(x => x.Amount)));
            if (document.RetailSaleDocPayments.Count > 0)
                await _paymentCommand.DeleteAsync(document.RetailSaleDocPayments, ct);
            foreach (var payment in payments.Value)
                payment.OwnerId = document.Id;
            await _paymentCommand.CreateAsync(payments.Value, ct);
            document.RetailSaleDocPayments = payments.Value;
        }

        await _documentCommand.UpdateAsync(document, ct);
        return Result.Success();
    }

    private Result ValidateForConfirm(RetailSaleDoc document)
    {
        if (document.RetailSaleDocProducts.Count == 0)
            return Result.Failure(RetailSaleDocErrors.EmptyLines());
        if (document.FinalAmount > 0m && (document.ReceivableAccountId is null || document.RetailSaleDocPayments.Count == 0))
            return Result.Failure(RetailSaleDocErrors.AccountRequired());
        if (document.RetailSaleDocPayments.Sum(x => x.Amount) != document.FinalAmount)
            return Result.Failure(RetailSaleDocErrors.PaymentTotalMismatch(document.FinalAmount, document.RetailSaleDocPayments.Sum(x => x.Amount)));

        foreach (var line in document.RetailSaleDocProducts)
        {
            if (line.Quantity <= 0m || line.IncomeAccountId is null)
                return Result.Failure(RetailSaleDocErrors.InvalidLine(line.ProductId));
            if (line.VatAmount > 0m && document.VatAccountId is null)
                return Result.Failure(RetailSaleDocErrors.AccountRequired());

            if (line.Product.IsService || !line.Product.IsPieceTracked)
            {
                if (line.RetailSaleDocTables.Count > 0)
                    return Result.Failure(RetailSaleDocErrors.InvalidLine(line.ProductId));
                if (!line.Product.IsService && (line.InventoryAccountId is null || line.CostAccountId is null))
                    return Result.Failure(RetailSaleDocErrors.AccountRequired());
                continue;
            }

            if (line.Quantity != decimal.Truncate(line.Quantity) ||
                !SaleMarkingPolicy.IsOccurrenceCountAllowed(line.Quantity, line.RetailSaleDocTables.Count) ||
                line.InventoryAccountId is null || line.CostAccountId is null)
            {
                return Result.Failure(RetailSaleDocErrors.InvalidLine(line.ProductId));
            }

            var invalid = line.RetailSaleDocTables.FirstOrDefault(item =>
                item.ProductTable.WarehouseProductTable is null ||
                item.ProductTable.WarehouseProductTable.WarehouseId != document.WarehouseId ||
                item.ProductTable.WarehouseProductTable.StatusId != ProductTableStatusIdConst.IN_STOCK);
            if (invalid is not null)
                return Result.Failure(RetailSaleDocErrors.InvalidProductTable(invalid.ProductTableId));
        }
        return Result.Success();
    }

    private static int[] GetDistinctPhysicalMarkingIds(RetailSaleDoc document) =>
        SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(
                document.RetailSaleDocProducts.Select(line =>
                    line.RetailSaleDocTables.Select(item => item.ProductTableId)))
            .ToArray();

    private async Task<RetailSaleDoc?> GetDocumentAsync(long id, CancellationToken ct)
    {
        var organization = GetOrganizationId();
        if (!organization.IsSuccess)
            return null;

        var query = _queryBuilder.For<RetailSaleDoc>().Where(x => x.Id == id && x.OrganizationId == organization.Value).Build();
        query.AddIncludes(x => x.Include(d => d.RetailSaleDocProducts).ThenInclude(line => line.Product));
        query.AddIncludes(x => x.Include(d => d.RetailSaleDocProducts).ThenInclude(line => line.RetailSaleDocTables).ThenInclude(item => item.ProductTable).ThenInclude(table => table.WarehouseProductTable));
        query.AddIncludes(x => x.Include(d => d.RetailSaleDocPayments));
        return await _documentQuery.GetAsync(query, ct);
    }

    private async Task<RetailSaleDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var organization = GetOrganizationId();
        if (!organization.IsSuccess)
            return null;
        return await _documentQuery.GetAsync(_queryBuilder.For<RetailSaleDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organization.Value).As<RetailSaleDocDto>().Build(), ct);
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(RetailSaleDoc document, string status, string comment, CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.RETAIL_SALE,
            DocumentId = document.Id,
            Status = status,
            PostedAt = DateTime.Now,
            PostedByUserId = _userContext.Id,
            Comment = comment
        };
        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long documentId, CancellationToken ct) =>
        await _postingBatchQuery.GetAsync(_queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.RETAIL_SALE && x.DocumentId == documentId && x.Status == PostingBatchStatusConst.POSTED)
            .Build(), ct);

    private async Task<bool> HasBusinessEffectsAsync(long documentId, CancellationToken ct) =>
        await _entryQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.RETAIL_SALE && x.DocumentId == documentId && x.ReversalEntryId == null, ct) ||
        await _movementQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.RETAIL_SALE && x.DocumentId == documentId, ct);

    private async Task<Result> ReverseAccountingEntriesAsync(long documentId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.RETAIL_SALE && x.DocumentId == documentId && x.ReversalEntryId == null).Build();
        query.AddIncludes(x => x.Include(entry => entry.RegisterEntrySubkontos));
        var entries = await _entryQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Success();

        var now = DateTime.Now;
        var reversals = entries.Select(entry => new AccountingRegisterEntry
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            DebitAccountId = entry.CreditAccountId,
            CreditAccountId = entry.DebitAccountId,
            CurrencyId = entry.CurrencyId,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            OperationTypeId = entry.OperationTypeId,
            DebitQuantity = entry.CreditQuantity,
            CreditQuantity = entry.DebitQuantity,
            Content = $"Reversal: {entry.Content}",
            JournalNumber = entry.JournalNumber,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id,
            RegisterEntrySubkontos = entry.RegisterEntrySubkontos.Select(subkonto => new RegisterEntrySubkonto
            {
                Side = subkonto.Side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT :
                       subkonto.Side == SubkontoSideConst.CREDIT ? SubkontoSideConst.DEBIT : subkonto.Side,
                SubkontoTypeId = subkonto.SubkontoTypeId,
                SortOrder = subkonto.SortOrder,
                EntityId = subkonto.EntityId,
                DisplayValue = subkonto.DisplayValue,
                CreatedDate = now
            }).ToList()
        }).ToList();
        await _entryCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }

    private Result<int> GetOrganizationId() =>
        _userContext.OrganizationId.HasValue
            ? Result.Success(_userContext.OrganizationId.Value)
            : Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

    private sealed record RetailSaleLineDraft(RetailSaleDocProduct Line, List<RetailSaleDocTable> Tables);

    private sealed class RetailSaleDetails
    {
        public RetailSaleDetails(List<RetailSaleLineDraft> lines, List<RetailSaleDocPayment> payments)
        {
            Lines = lines;
            Payments = payments;
        }

        public List<RetailSaleLineDraft> Lines { get; }
        public List<RetailSaleDocPayment> Payments { get; }
        public decimal TotalAmount => Lines.Sum(x => x.Line.Amount);
        public decimal VatAmount => Lines.Sum(x => x.Line.VatAmount);
        public decimal FinalAmount => Lines.Sum(x => x.Line.TotalAmount);
    }
}
