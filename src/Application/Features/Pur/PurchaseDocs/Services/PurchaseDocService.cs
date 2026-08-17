using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Abstractions.Integration.Edo;
using Application.Features.AuditLogs;
using Application.Features.Contracts;
using Application.Features.CounterpartyCards;
using Application.Features.DocumentNumbers;
using Application.Features.PurchaseDocTables;
using Application.Features.Warehouses;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocService : BaseService, IPurchaseDocService, IEdoHistoricalPurchaseDraftFactory
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IPurchaseLifecycleService _purchaseLifecycleService;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly ICommandRepository<PurchaseDocProduct> _productLineCommand;
    private readonly ICommandRepository<PurchaseDocTable> _tableLineCommand;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IActiveEdoProviderResolver _activeEdoProviderResolver;
    private readonly IEdoDocumentStore _edoDocumentStore;
    private readonly IQueryRepository<OrganizationConfig> _organizationConfigQuery;
    private readonly IQueryRepository<OrganizationDefault> _organizationDefaultQuery;
    private readonly IBackgroundOrganizationScope? _backgroundOrganizationScope;

    public PurchaseDocService(IUserContext userContext,
                              IQueryBuilder queryBuilder,
                              IPurchaseLifecycleService purchaseLifecycleService,
                              IAuditLogService auditLogService,
                              IDocumentNumberService documentNumberService,
                              IQueryRepository<PurchaseDoc> query,
                              IQueryRepository<VatRate> vatRateQuery,
                              IQueryRepository<Contract> contractQuery,
                              IQueryRepository<CounterpartyCard> counterpartyQuery,
                              IQueryRepository<Warehouse> warehouseQuery,
                              IQueryRepository<Currency> currencyQuery,
                              IQueryRepository<Unit> unitQuery,
                              IQueryRepository<Product> productQuery,
                              ICommandRepository<ProductTable> productTableCommand,
                              IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                              ICommandRepository<PurchaseDoc> command,
                              ICommandRepository<PurchaseDocProduct> productLineCommand,
                              ICommandRepository<PurchaseDocTable> tableLineCommand,
                              IActiveEdoProviderResolver activeEdoProviderResolver,
                              IEdoDocumentStore edoDocumentStore,
                              IQueryRepository<OrganizationConfig> organizationConfigQuery,
                              IQueryRepository<OrganizationDefault> organizationDefaultQuery,
                              ILogger<PurchaseDocService> logger,
                              IUnitOfWork unitOfWork,
                              IBackgroundOrganizationScope? backgroundOrganizationScope = null)
            : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _productLineCommand = productLineCommand;
        _tableLineCommand = tableLineCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _purchaseLifecycleService = purchaseLifecycleService;
        _auditLogService = auditLogService;
        _vatRateQuery = vatRateQuery;
        _contractQuery = contractQuery;
        _counterpartyQuery = counterpartyQuery;
        _warehouseQuery = warehouseQuery;
        _currencyQuery = currencyQuery;
        _unitQuery = unitQuery;
        _productQuery = productQuery;
        _productTableCommand = productTableCommand;
        _purchaseDocTableQuery = purchaseDocTableQuery;
        _documentNumberService = documentNumberService;
        _activeEdoProviderResolver = activeEdoProviderResolver;
        _edoDocumentStore = edoDocumentStore;
        _organizationConfigQuery = organizationConfigQuery;
        _organizationDefaultQuery = organizationDefaultQuery;
        _backgroundOrganizationScope = backgroundOrganizationScope;
    }

    public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<PurchaseDoc, PurchaseDocListDto, PurchaseDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PurchaseDocDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>()
                .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
                .As<PurchaseDocDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<PurchaseDocDto>(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<PurchaseDocPreviewDto>> PreviewAsync(
        PurchaseDocPreviewRequestDto request,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(PreviewAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PurchaseDocPreviewDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var provider = await _activeEdoProviderResolver.GetActiveProviderAsync(ct);
            EnsureCapability(provider, EdoCapabilityKind.GetDetail);

            var identity = request.DocumentIdentity.Trim();
            var document = await provider.GetDocumentDetailsAsync(
                EdoDirection.INBOX,
                "FACTURA",
                identity,
                ct);

            if (document.Status.Code != EdoDocumentStatusCode.SIGNED)
                return Result.Failure<PurchaseDocPreviewDto>(
                    Error.Business(
                        "PurchasePreview.SignedDocumentRequired",
                        "Only SIGNED EDO documents can be used for Purchase preview."));

            if (!string.Equals(document.DocumentType, "FACTURA", StringComparison.OrdinalIgnoreCase))
                return Result.Failure<PurchaseDocPreviewDto>(
                    Error.Business(
                        "PurchasePreview.UnsupportedDocumentType",
                        "Only FACTURA EDO documents can be used for Purchase preview."));

            var duplicate = await _edoDocumentStore.FindByProviderDocumentIdAsync(
                organizationId,
                provider.Code,
                identity,
                ct);
            var duplicateResult = ResolvePreviewDuplicate(duplicate);

            var errors = new List<PurchaseDocPreviewValidationErrorDto>();
            var counterparty = await ResolveCounterpartyAsync(
                organizationId,
                request.CounterpartyId,
                document.PreviewSellerTin ?? document.Seller?.TaxIdentifier,
                errors,
                ct);
            var contract = await ResolveContractAsync(
                organizationId,
                request.ContractId,
                counterparty.Id,
                document.PreviewContractNumber,
                document.PreviewContractDate,
                document.DocumentDate,
                errors,
                ct);
            var lines = await ResolveLinesAsync(
                organizationId,
                document,
                request.Lines,
                errors,
                ct);
            var currency = await ResolveCurrencyAsync(
                organizationId,
                request.CurrencyId,
                errors,
                ct);
            var warehouse = await ResolveWarehouseAsync(
                organizationId,
                request.WarehouseId,
                errors,
                ct);

            return Result.Success(new PurchaseDocPreviewDto
            {
                Provider = provider.Code,
                DocumentIdentity = identity,
                Status = EdoDocumentStatusCode.SIGNED,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                DocumentDateTime = document.DocumentDateTime,
                Counterparty = counterparty.Dto,
                Contract = contract.Dto,
                Lines = lines,
                Currency = currency,
                Warehouse = warehouse,
                ValidationErrors = errors,
                Duplicate = duplicateResult,
                CanCreateDraft = !duplicateResult.IsDuplicate
                    && errors.Count == 0
                    && counterparty.Dto.IsResolved
                    && contract.Dto.IsResolved
                    && currency.IsResolved
                    && warehouse.IsResolved
                    && lines.Count > 0
                    && lines.All(x => x.IsResolved)
            });
        });

    internal static PurchaseDocPreviewDuplicateDto ResolvePreviewDuplicate(EdoDocument? document)
    {
        var isPurchaseLinked = document is not null
            && document.InternalDocumentId > 0
            && string.Equals(
                document.InternalDocumentType,
                "PURCHASE",
                StringComparison.OrdinalIgnoreCase);

        return new PurchaseDocPreviewDuplicateDto
        {
            IsDuplicate = isPurchaseLinked,
            ExistingPurchaseId = isPurchaseLinked ? document!.InternalDocumentId : null
        };
    }

    public Task<Result<PurchaseDocDto>> CreateFromEdoAsync(
        PurchaseDocFromEdoRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateFromEdoAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PurchaseDocDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var provider = await _activeEdoProviderResolver.GetActiveProviderAsync(ct);
            EnsureCapability(provider, EdoCapabilityKind.GetDetail);

            var identity = request.DocumentIdentity.Trim();
            var localDocument = await _edoDocumentStore.FindByProviderDocumentIdAsync(
                organizationId,
                provider.Code,
                identity,
                ct);

            if (localDocument is null)
                return Result.Failure<PurchaseDocDto>(
                    Error.NotFound(
                        "PurchaseFromEdo.InboxDocumentRequired",
                        "The EDO document must first be available in the organization inbox scope."));

            if (!string.Equals(localDocument.Direction, EdoDirection.INBOX.ToString(), StringComparison.OrdinalIgnoreCase))
                return Result.Failure<PurchaseDocDto>(
                    Error.Business(
                        "PurchaseFromEdo.InboxDocumentRequired",
                        "Only EDO inbox documents can be imported as a Purchase."));

            if (localDocument.InternalDocumentId > 0)
                return Result.Failure<PurchaseDocDto>(
                    Error.Conflict(
                        "PurchaseFromEdo.Duplicate",
                        "A Purchase already exists for this EDO document."));

            var document = await provider.GetDocumentDetailsAsync(
                EdoDirection.INBOX,
                "FACTURA",
                identity,
                ct);

            if (!string.Equals(document.ProviderDocumentId, identity, StringComparison.Ordinal))
                throw new IntegrationHttpException(
                    "The provider detail response did not match the requested document identity.",
                    502);

            return await CreateDraftFromNormalizedEdoAsync(
                organizationId, provider.Code, document, request, localDocument,
                historicalImport: false, ct);
        }, ct);

    public Task<Result<PurchaseDocDto>> CreateFromHistoricalSnapshotAsync(
        EdoDocumentDto document,
        PurchaseDocFromEdoRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateFromHistoricalSnapshotAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PurchaseDocDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            if (document.ProviderCode is not EdoProviderCode.EDOCS and not EdoProviderCode.DIDOX
                || string.IsNullOrWhiteSpace(document.ProviderDocumentId)
                || !string.Equals(document.ProviderDocumentId, request.DocumentIdentity, StringComparison.Ordinal))
                return Result.Failure<PurchaseDocDto>(CreateFromHistoricalEdoValidationError([]));

            var organizationId = _userContext.OrganizationId.Value;
            var localDocument = await _edoDocumentStore.FindByProviderDocumentIdAsync(
                organizationId, document.ProviderCode, document.ProviderDocumentId, ct);
            if (localDocument?.InternalDocumentId > 0)
                return Result.Failure<PurchaseDocDto>(Error.Conflict(
                    "PurchaseFromEdo.Duplicate",
                    "A Purchase already exists for this EDO document."));

            return await CreateDraftFromNormalizedEdoAsync(
                organizationId, document.ProviderCode, document, request, localDocument,
                historicalImport: true, ct);
        }, ct);

    private async Task<Result<PurchaseDocDto>> CreateDraftFromNormalizedEdoAsync(
        int organizationId,
        EdoProviderCode providerCode,
        EdoDocumentDto document,
        PurchaseDocFromEdoRequestDto request,
        EdoDocument? localDocument,
        bool historicalImport,
        CancellationToken ct)
    {
        if (document.Direction != EdoDirection.INBOX)
            return Result.Failure<PurchaseDocDto>(Error.Business(
                "PurchaseFromEdo.InboxDocumentRequired",
                "Only EDO inbox documents can be imported as a Purchase."));
        if (document.Status.Code != EdoDocumentStatusCode.SIGNED)
            return Result.Failure<PurchaseDocDto>(Error.Business(
                "PurchaseFromEdo.SignedDocumentRequired",
                "Only SIGNED EDO documents can be imported as a Purchase."));
        if (!string.Equals(document.DocumentType, "FACTURA", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<PurchaseDocDto>(Error.Business(
                "PurchaseFromEdo.UnsupportedDocumentType",
                "Only FACTURA EDO documents can be imported as a Purchase."));
        if (!document.DocumentDate.HasValue)
            return Result.Failure<PurchaseDocDto>(Error.Business(
                "PurchaseFromEdo.DocumentDateRequired",
                "The provider document date is required."));

        var errors = new List<PurchaseDocPreviewValidationErrorDto>();
        var counterparty = await ResolveCounterpartyAsync(
            organizationId, request.CounterpartyId,
            document.PreviewSellerTin ?? document.Seller?.TaxIdentifier, errors, ct);
        PurchaseDocPreviewContractDto contract;
        if (!request.ContractId.HasValue)
        {
            AddPreviewError(errors, "CONTRACT_SELECTION_REQUIRED", "contractId", "A contract selection is required.");
            contract = new PurchaseDocPreviewContractDto { RequiresSelection = true };
        }
        else
        {
            var resolvedContract = await ResolveContractAsync(
                organizationId, request.ContractId, counterparty.Id,
                document.PreviewContractNumber, document.PreviewContractDate,
                document.DocumentDate, errors, ct);
            contract = resolvedContract.Dto;
            if (document.PreviewContractDate.HasValue
                && contract.IsResolved
                && contract.Date != document.PreviewContractDate)
                AddPreviewError(errors, "CONTRACT_DATE_MISMATCH", "contractId", "The selected contract date does not match the provider contract date.");
        }

        var mappings = request.Lines.Select(line => new PurchaseDocPreviewLineMappingDto
        {
            LineNumber = line.LineNumber,
            ProductId = line.ProductId,
            UnitId = line.UnitId,
            VatRateId = line.VatRateId,
            Items = line.Items
        }).ToList();
        var resolvedLines = await ResolveLinesAsync(organizationId, document, mappings, errors, ct);
        ValidateLineCoverage(document, request.Lines, errors);
        var currency = await ResolveCurrencyAsync(organizationId, request.CurrencyId, errors, ct);
        var warehouse = await ResolveWarehouseAsync(organizationId, request.WarehouseId, errors, ct);
        if (errors.Count > 0 || counterparty.Id is null || contract.Id is null
            || currency.Id is null || warehouse.Id is null)
            return Result.Failure<PurchaseDocDto>(historicalImport
                ? CreateFromHistoricalEdoValidationError(errors)
                : CreateFromEdoValidationError());

        var purchaseLines = BuildPurchaseLinesFromEdo(document, request.Lines, resolvedLines, errors);
        if (errors.Count > 0)
            return Result.Failure<PurchaseDocDto>(historicalImport
                ? CreateFromHistoricalEdoValidationError(errors)
                : CreateFromEdoValidationError());
        var purchaseDto = new PurchaseDocCreateDto
        {
            ExternalId = document.ProviderDocumentId,
            ExternalDocNumber = document.DocumentNumber,
            DocDate = ResolveEdoDocumentDateTime(document),
            CounterpartyId = counterparty.Id.Value,
            WarehouseId = warehouse.Id.Value,
            CurrencyId = (short)currency.Id.Value,
            ContractId = contract.Id.Value,
            Comment = request.Comment,
            ProcessingMode = PurchaseProcessingMode.StepByStep,
            Lines = purchaseLines
        };
        var builtLines = await BuildAllLinesAsync(organizationId, purchaseDto.Lines, ct);
        if (!builtLines.IsSuccess)
            return Result.Failure<PurchaseDocDto>(builtLines.Error);
        ValidateProviderTotals(document, builtLines.Value, errors);
        if (errors.Count > 0)
            return Result.Failure<PurchaseDocDto>(historicalImport
                ? CreateFromHistoricalEdoValidationError(errors)
                : CreateFromEdoValidationError());

        var created = await CreateCoreAsync(purchaseDto, historicalImport, ct);
        if (!created.IsSuccess)
            return Result.Failure<PurchaseDocDto>(created.Error);

        var now = DateTime.UtcNow;
        if (localDocument is null)
        {
            localDocument = new EdoDocument
            {
                OrganizationId = organizationId,
                Provider = providerCode.ToString(),
                Direction = EdoDirection.INBOX.ToString(),
                InternalDocumentType = "PURCHASE",
                InternalDocumentId = created.Value,
                ProviderDocumentId = document.ProviderDocumentId,
                DocumentType = document.DocumentType,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                DocumentDateTime = document.DocumentDateTime,
                Status = document.Status.Code.ToString(),
                ProviderStatusCode = document.Status.ProviderStatusCode,
                OperationType = "PURCHASE_FROM_EDO",
                CreatedAt = now,
                UpdatedAt = now
            };
            await _edoDocumentStore.AddAsync(localDocument, ct);
        }
        else
        {
            localDocument.InternalDocumentType = "PURCHASE";
            localDocument.InternalDocumentId = created.Value;
            localDocument.OperationType = "PURCHASE_FROM_EDO";
            localDocument.Status = document.Status.Code.ToString();
            localDocument.ProviderStatusCode = document.Status.ProviderStatusCode;
            localDocument.DocumentType = document.DocumentType;
            localDocument.DocumentNumber = document.DocumentNumber;
            localDocument.DocumentDate = document.DocumentDate;
            localDocument.DocumentDateTime = document.DocumentDateTime;
            localDocument.UpdatedAt = now;
            await _edoDocumentStore.UpdateAsync(localDocument, ct);
        }

        _auditLogService.SetNewValues(new
        {
            operation = "PURCHASE_FROM_EDO",
            provider = providerCode.ToString(),
            documentId = localDocument.Id,
            purchaseId = created.Value
        });
        await _auditLogService.CreateAsync(
            AuditLogTableConst.PurchaseDoc,
            created.Value.ToString(),
            AuditLogOperationTypeConst.Update,
            historicalImport && _backgroundOrganizationScope?.IsActive == true
                ? "BACKGROUND_EDO_HISTORICAL_DRAFT_IMPORT"
                : null,
            organizationId);
        var result = await GetByIdInternalAsync(created.Value, ct);
        return result is null
            ? Result.Failure<PurchaseDocDto>(PurchaseDocErrors.NotFound(created.Value, _userContext.LanguageId))
            : Result.Success(result);
    }

    internal static DateTime ResolveEdoDocumentDateTime(EdoDocumentDto document)
    {
        var value = document.DocumentDateTime
            ?? document.DocumentDate?.ToDateTime(TimeOnly.MinValue)
            ?? throw new InvalidOperationException("The provider document date is required.");
        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    public Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), () =>
            CreateCoreAsync(dto, historicalImport: false, ct), ct);

    private async Task<Result<long>> CreateCoreAsync(
        PurchaseDocCreateDto dto,
        bool historicalImport,
        CancellationToken ct)
    {
            var organizationId = historicalImport && _backgroundOrganizationScope?.OrganizationId is { } scopeOrganizationId
                ? scopeOrganizationId
                : _userContext.OrganizationId;
            if (!organizationId.HasValue)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var headerValidation = await ValidateHeaderReferencesAsync(dto, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure<long>(headerValidation.Error);

            var allLinesResult = await BuildAllLinesAsync(organizationId.Value, dto.Lines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure<long>(allLinesResult.Error);

            var allLines = allLinesResult.Value;
            var documentNumberResult = historicalImport
                ? await _documentNumberService.GetNextHistoricalAsync(
                    organizationId.Value,
                    DocumentTypeIdConst.PURCHASE,
                    dto.DocDate,
                    ct)
                : await _documentNumberService.GetNextAsync(
                    organizationId.Value,
                    DocumentTypeIdConst.PURCHASE,
                    dto.DocDate,
                    ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var doc = new PurchaseDoc
            {
                OrganizationId = organizationId.Value,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                ExternalId = dto.ExternalId,
                ExternalDocNumber = dto.ExternalDocNumber,
                DocDate = dto.DocDate,
                CurrencyId = dto.CurrencyId,
                ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
                PurchaseDocProducts = allLines,
                TotalAmount = allLines.Sum(l => l.Amount),
                VatAmount = allLines.Sum(l => l.VatAmount),
                FinalAmount = allLines.Sum(l => l.TotalAmount),
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                WarehouseId = dto.WarehouseId,
                CounterpartyId = dto.CounterpartyId,
                ContractId = dto.ContractId,
                SupplierAccountId = dto.SupplierAccountId,
            };

            await _command.CreateAsync(doc, ct);

            if (dto.ProcessingMode == PurchaseProcessingMode.Immediate)
            {
                var confirmResult = await _purchaseLifecycleService.ConfirmAsync(doc.Id, ct);
                if (!confirmResult.IsSuccess)
                    return Result.Failure<long>(confirmResult.Error);
            }

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
    }

    public Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>()
                .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PurchaseDocErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var headerValidation = await ValidateHeaderReferencesAsync(dto, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure(headerValidation.Error);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure(allLinesResult.Error);

            var newLines = allLinesResult.Value;
            var existingTableLinks = await GetPurchaseTableLinksAsync(id, ct);
            var oldPurchaseDocLineIds = existingTableLinks.Select(x => x.OwnerId).Distinct().ToList();
            var oldProductTableIds = existingTableLinks.Select(x => x.ProductTableId).Distinct().ToList();

            // Eski qatorlarni o'chirib, yangilarini yozamiz
            if (oldPurchaseDocLineIds.Count > 0)
                await _tableLineCommand.DeleteAsync(l => oldPurchaseDocLineIds.Contains(l.OwnerId), ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);
            if (oldProductTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => oldProductTableIds.Contains(x.Id), ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _productLineCommand.CreateAsync(newLines, ct);

            doc.OrganizationId = _userContext.OrganizationId.Value;
            doc.ExternalId = dto.ExternalId;
            doc.ExternalDocNumber = dto.ExternalDocNumber;
            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId = dto.WarehouseId;
            doc.CurrencyId = dto.CurrencyId;
            doc.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
            doc.ContractId = dto.ContractId;
            doc.SupplierAccountId = dto.SupplierAccountId;
            doc.TotalAmount = newLines.Sum(l => l.Amount);
            doc.VatAmount = newLines.Sum(l => l.VatAmount);
            doc.FinalAmount = newLines.Sum(l => l.TotalAmount);
            doc.Comment = dto.Comment;
            // State is lifecycle-managed; draft updates must not overwrite it from request payload.

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _purchaseLifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _purchaseLifecycleService.CancelAsync(id, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PurchaseDocErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var productTableIds = (await GetPurchaseTableLinksAsync(id, ct))
                .Select(x => x.ProductTableId)
                .Distinct()
                .ToList();

            // Avval barcha qatorlarni o'chiramiz, keyin hujjatni
            await _tableLineCommand.DeleteAsync(l => l.Owner.OwnerId == id, ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);
            if (productTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => productTableIds.Contains(x.Id), ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    private async Task<PurchaseDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<PurchaseDoc>()
            .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
            .As<PurchaseDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<List<PurchaseTableLink>> GetPurchaseTableLinksAsync(long purchaseDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => x.Owner.OwnerId == purchaseDocId)
            .As(x => new PurchaseTableLink(x.OwnerId, x.ProductTableId))
            .Build();

        return await _purchaseDocTableQuery.GetAllAsync(query, ct);
    }

    private static Error CreateFromEdoValidationError() =>
        Error.Business(
            "PurchaseFromEdo.ValidationFailed",
            "The EDO document mappings or provider values are not valid for Purchase creation.");

    private static Error CreateFromHistoricalEdoValidationError(
        IReadOnlyCollection<PurchaseDocPreviewValidationErrorDto> errors)
    {
        var reason = errors.Select(error => error.Code)
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code));
        var normalized = string.IsNullOrWhiteSpace(reason)
            ? "GENERAL"
            : new string(reason.ToUpperInvariant()
                .Select(character => char.IsAsciiLetterOrDigit(character) ? character : '_')
                .ToArray()).Trim('_');
        if (normalized.Length > 40)
            normalized = normalized[..40];

        return Error.Business(
            $"PurchaseFromEdo.HistoricalValidation.{normalized}",
            "The historical EDO snapshot is not valid for Draft Purchase creation.");
    }

    private static void ValidateLineCoverage(
        EdoDocumentDto document,
        IReadOnlyCollection<PurchaseDocFromEdoLineDto> requestLines,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors)
    {
        var providerLineNumbers = document.PreviewLines.Select(line => line.Number).ToHashSet();
        var requestLineNumbers = requestLines.Select(line => line.LineNumber).ToHashSet();

        foreach (var lineNumber in providerLineNumbers.Except(requestLineNumbers))
            AddPreviewError(errors, "LINE_MAPPING_REQUIRED", $"lines[{lineNumber}]", "A mapping is required for every provider line.");

        foreach (var lineNumber in requestLineNumbers.Except(providerLineNumbers))
            AddPreviewError(errors, "LINE_MAPPING_UNKNOWN", $"lines[{lineNumber}]", "The requested line does not exist in the provider document.");
    }

    internal static List<PurchaseDocLineDto> BuildPurchaseLinesFromEdo(
        EdoDocumentDto document,
        IReadOnlyCollection<PurchaseDocFromEdoLineDto> requestLines,
        IReadOnlyCollection<PurchaseDocPreviewLineDto> resolvedLines,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors)
    {
        var sourceByNumber = document.PreviewLines.ToDictionary(line => line.Number);
        var resolvedByNumber = resolvedLines.ToDictionary(line => line.Number);
        var result = new List<PurchaseDocLineDto>(requestLines.Count);

        foreach (var requestLine in requestLines.OrderBy(line => line.LineNumber))
        {
            if (!sourceByNumber.TryGetValue(requestLine.LineNumber, out var sourceLine)
                || !resolvedByNumber.TryGetValue(requestLine.LineNumber, out var resolvedLine)
                || !resolvedLine.IsResolved
                || resolvedLine.ProductId is null
                || resolvedLine.UnitId is null)
            {
                continue;
            }

            if (!sourceLine.Quantity.HasValue || !sourceLine.UnitPrice.HasValue)
                continue;

            result.Add(new PurchaseDocLineDto
            {
                ProductId = resolvedLine.ProductId.Value,
                Quantity = sourceLine.Quantity.Value,
                UnitId = resolvedLine.UnitId.Value,
                UnitPrice = sourceLine.UnitPrice.Value,
                VatRateId = resolvedLine.VatRateId,
                ProviderNetAmount = sourceLine.NetAmount,
                ProviderVatAmount = sourceLine.VatAmount,
                ProviderTotalAmount = sourceLine.TotalWithVat,
                DebitAccountId = requestLine.DebitAccountId,
                VatAccountId = requestLine.VatAccountId,
                Items = resolvedLine.IsService == true
                    ? []
                    : requestLine.Items
                        .Select(item => new PurchaseDocLineItemDto
                        {
                            MarkingNumber = item.MarkingNumber,
                            SerialNumber = item.SerialNumber
                        })
                        .ToList()
            });
        }

        if (result.Count != requestLines.Count)
            AddPreviewError(errors, "LINE_MAPPING_REQUIRED", "lines", "Every provider line must have a valid local mapping.");

        return result;
    }

    private static void ValidateProviderTotals(
        EdoDocumentDto document,
        IReadOnlyCollection<PurchaseDocProduct> builtLines,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors)
    {
        var providerLines = document.PreviewLines.OrderBy(line => line.Number).ToList();
        var persistedLines = builtLines.ToList();
        if (providerLines.Count != persistedLines.Count)
        {
            AddPreviewError(errors, "TOTAL_MAPPING_REQUIRED", "lines", "Provider and local line totals could not be reconciled.");
            return;
        }

        for (var index = 0; index < providerLines.Count; index++)
        {
            var providerTotal = providerLines[index].TotalWithVat;
            if (providerTotal.HasValue
                && Math.Abs(providerTotal.Value - persistedLines[index].TotalAmount) > 0.01m)
            {
                AddPreviewError(errors, "TOTAL_MISMATCH", $"lines[{providerLines[index].Number}].totalWithVat", "Provider and local line totals do not match.");
            }
        }

        if (document.TotalAmount.HasValue
            && Math.Abs(document.TotalAmount.Value - persistedLines.Sum(line => line.TotalAmount)) > 0.01m)
        {
            AddPreviewError(errors, "TOTAL_MISMATCH", "totalAmount", "Provider and local document totals do not match.");
        }
    }

    private async Task<Result> ValidateHeaderReferencesAsync(PurchaseDocBaseDto dto, CancellationToken ct)
    {
        var counterpartyExists = await _counterpartyQuery.AnyAsync(x => x.Id == dto.CounterpartyId, ct);
        if (!counterpartyExists)
            return Result.Failure(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

        var warehouseExists = await _warehouseQuery.AnyAsync(x => x.Id == dto.WarehouseId, ct);
        if (!warehouseExists)
            return Result.Failure(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

        var currencyExists = await _currencyQuery.AnyAsync(x => x.Id == dto.CurrencyId, ct);
        if (!currencyExists)
            return Result.Failure(PurchaseDocErrors.CurrencyNotFound(dto.CurrencyId, _userContext.LanguageId));

        if (dto.ContractId.HasValue)
        {
            var contractExists = await _contractQuery.AnyAsync(x => x.Id == dto.ContractId.Value, ct);
            if (!contractExists)
                return Result.Failure(ContractErrors.NotFound(dto.ContractId.Value, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<(PurchaseDocPreviewCounterpartyDto Dto, int? Id)> ResolveCounterpartyAsync(
        int organizationId,
        int? requestedId,
        string? sellerTin,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        CancellationToken ct)
    {
        var normalizedTin = string.IsNullOrWhiteSpace(sellerTin) ? null : sellerTin.Trim();
        CounterpartyCard? counterparty = null;

        if (requestedId.HasValue)
        {
            counterparty = await _counterpartyQuery.GetAsync(
                _queryBuilder.For<CounterpartyCard>()
                    .Where(x => x.Id == requestedId.Value
                                && x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct);

            if (counterparty is null)
            {
                AddPreviewError(errors, "COUNTERPARTY_NOT_FOUND", "counterpartyId", "The selected counterparty is not available in the current organization.");
            }
            else if (normalizedTin is null)
            {
                AddPreviewError(errors, "COUNTERPARTY_TIN_UNAVAILABLE", "counterpartyId", "The provider response did not contain a seller TIN for exact counterparty verification.");
                counterparty = null;
            }
            else if (!string.Equals(counterparty.Inn?.Trim(), normalizedTin, StringComparison.Ordinal))
            {
                AddPreviewError(errors, "COUNTERPARTY_TIN_MISMATCH", "counterpartyId", "The selected counterparty INN does not match the provider seller TIN.");
                counterparty = null;
            }
        }
        else if (normalizedTin is not null)
        {
            var matches = await _counterpartyQuery.GetAllAsync(
                _queryBuilder.For<CounterpartyCard>()
                    .Where(x => x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE
                                && x.Inn == normalizedTin)
                    .Build(),
                ct);

            if (matches.Count == 1)
                counterparty = matches[0];
            else if (matches.Count > 1)
                AddPreviewError(errors, "COUNTERPARTY_AMBIGUOUS", "counterpartyId", "More than one local counterparty matches the provider seller TIN.");
            else
                AddPreviewError(errors, "COUNTERPARTY_NOT_FOUND", "counterpartyId", "No local counterparty matches the provider seller TIN.");
        }
        else
        {
            AddPreviewError(errors, "COUNTERPARTY_TIN_UNAVAILABLE", "counterpartyId", "The provider response did not contain a seller TIN.");
        }

        return (new PurchaseDocPreviewCounterpartyDto
        {
            Id = counterparty?.Id,
            Name = counterparty?.FullName ?? counterparty?.ShortName,
            Inn = counterparty?.Inn ?? normalizedTin,
            IsResolved = counterparty is not null,
            RequiresSelection = counterparty is null
        }, counterparty?.Id);
    }

    private async Task<(PurchaseDocPreviewContractDto Dto, long? Id)> ResolveContractAsync(
        int organizationId,
        long? requestedId,
        int? counterpartyId,
        string? providerNumber,
        DateOnly? providerDate,
        DateOnly? documentDate,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        CancellationToken ct)
    {
        if (!documentDate.HasValue)
        {
            AddPreviewError(errors, "DOCUMENT_DATE_REQUIRED", "documentDate", "A document date is required to validate the contract period.");
            return (new PurchaseDocPreviewContractDto { RequiresSelection = true }, null);
        }

        var day = documentDate.Value.ToDateTime(TimeOnly.MinValue);
        Contract? contract = null;
        if (requestedId.HasValue)
        {
            contract = await _contractQuery.GetAsync(
                _queryBuilder.For<Contract>()
                    .Where(x => x.Id == requestedId.Value
                                && x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE
                                && (counterpartyId == null || x.CounterpartyId == counterpartyId.Value)
                                && (x.StartDate == null || x.StartDate <= day)
                                && (x.EndDate == null || x.EndDate >= day))
                    .Build(),
                ct);

            if (contract is null)
                AddPreviewError(errors, "CONTRACT_NOT_FOUND", "contractId", "The selected contract is not active, valid for the document date, or in the current organization scope.");
            else if (!IsSelectedContractValid(contract, organizationId, counterpartyId, day))
            {
                AddPreviewError(errors, "CONTRACT_NOT_FOUND", "contractId", "The selected contract is not active, valid for the document date, or in the current organization scope.");
                contract = null;
            }
        }
        else if (!string.IsNullOrWhiteSpace(providerNumber) && providerDate.HasValue)
        {
            var providerDay = providerDate.Value.ToDateTime(TimeOnly.MinValue);
            var matches = await _contractQuery.GetAllAsync(
                _queryBuilder.For<Contract>()
                    .Where(x => x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE
                                && (counterpartyId == null || x.CounterpartyId == counterpartyId.Value)
                                && x.ContractNumber == providerNumber
                                && x.ContractDate >= providerDay
                                && x.ContractDate < providerDay.AddDays(1)
                                && (x.StartDate == null || x.StartDate <= day)
                                && (x.EndDate == null || x.EndDate >= day))
                    .Build(),
                ct);

            if (matches.Count == 1)
                contract = matches[0];
            else if (matches.Count > 1)
                AddPreviewError(errors, "CONTRACT_AMBIGUOUS", "contractId", "More than one local contract matches the provider contract.");
            else
                AddPreviewError(errors, "CONTRACT_NOT_FOUND", "contractId", "No active local contract matches the provider contract and document date.");
        }
        else
        {
            AddPreviewError(errors, "CONTRACT_SELECTION_REQUIRED", "contractId", "A valid local contract selection is required because the provider contract data is incomplete.");
        }

        return (new PurchaseDocPreviewContractDto
        {
            Id = contract?.Id,
            Number = contract?.ContractNumber ?? providerNumber,
            Date = contract is null ? providerDate : DateOnly.FromDateTime(contract.ContractDate),
            IsResolved = contract is not null,
            RequiresSelection = contract is null
        }, contract?.Id);
    }

    private async Task<IReadOnlyCollection<PurchaseDocPreviewLineDto>> ResolveLinesAsync(
        int organizationId,
        EdoDocumentDto document,
        IReadOnlyCollection<PurchaseDocPreviewLineMappingDto> mappings,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        CancellationToken ct)
    {
        if (document.PreviewLines.Count == 0)
        {
            AddPreviewError(errors, "LINE_DATA_UNAVAILABLE", "lines", "The provider detail did not contain a supported purchase line shape.");
            return [];
        }

        var mappingByNumber = mappings
            .GroupBy(x => x.LineNumber)
            .ToDictionary(x => x.Key, x => x.Last());
        var result = new List<PurchaseDocPreviewLineDto>(document.PreviewLines.Count);

        foreach (var sourceLine in document.PreviewLines)
        {
            mappingByNumber.TryGetValue(sourceLine.Number, out var mapping);
            var product = await ResolveProductAsync(organizationId, sourceLine.CatalogCode, mapping?.ProductId, errors, sourceLine.Number, ct);
            var unit = await ResolveUnitAsync(sourceLine, product, mapping?.UnitId, errors, ct);
            var vatRate = await ResolveVatRateAsync(sourceLine.VatRate, mapping?.VatRateId, document.DocumentDate, errors, sourceLine.Number, ct);

            if (!sourceLine.Quantity.HasValue || sourceLine.Quantity <= 0
                || !sourceLine.UnitPrice.HasValue || sourceLine.UnitPrice < 0
                || !sourceLine.TotalWithVat.HasValue)
            {
                AddPreviewError(errors, "LINE_VALUES_INVALID", $"lines[{sourceLine.Number}]", "Quantity, unit price and total with VAT are required provider values.");
            }

            var providerMarkings = sourceLine.MarkingCodes.Count > 0
                ? sourceLine.MarkingCodes
                : document.PreviewLines.Count == 1 ? document.MarkingCodes : [];
            var markingErrorCode = GetMarkingValidationErrorCode(
                product,
                sourceLine.Quantity,
                providerMarkings,
                mapping?.Items ?? []);
            if (markingErrorCode is not null)
            {
                AddPreviewError(
                    errors,
                    markingErrorCode,
                    $"lines[{sourceLine.Number}].items",
                    GetMarkingValidationMessage(markingErrorCode));
            }

            var resolved = product is not null && unit is not null && vatRate is not null
                           && sourceLine.Quantity.HasValue && sourceLine.UnitPrice.HasValue && sourceLine.TotalWithVat.HasValue
                           && markingErrorCode is null;
            result.Add(new PurchaseDocPreviewLineDto
            {
                Number = sourceLine.Number,
                CatalogCode = sourceLine.CatalogCode,
                PackageCode = sourceLine.PackageCode,
                PackageName = sourceLine.PackageName,
                ItemType = product is null
                    ? PurchaseDocPreviewItemType.UNKNOWN
                    : product.IsService ? PurchaseDocPreviewItemType.SERVICE : PurchaseDocPreviewItemType.PRODUCT,
                ProductId = product?.Id,
                ProductName = product?.Name,
                IsService = product?.IsService,
                UnitId = unit?.Id,
                UnitName = unit?.Name,
                Quantity = sourceLine.Quantity,
                UnitPrice = sourceLine.UnitPrice,
                VatRate = sourceLine.VatRate,
                VatRateId = vatRate?.Id,
                TotalWithVat = sourceLine.TotalWithVat,
                IsResolved = resolved,
                RequiresManualMapping = !resolved
            });
        }

        return result;
    }

    private async Task<Product?> ResolveProductAsync(
        int organizationId,
        string? catalogCode,
        int? requestedId,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        int lineNumber,
        CancellationToken ct)
    {
        if (requestedId.HasValue)
        {
            var product = await _productQuery.GetAsync(
                _queryBuilder.For<Product>()
                    .Where(x => x.Id == requestedId.Value
                                && x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct);
            if (product is null)
                AddPreviewError(errors, "PRODUCT_NOT_FOUND", $"lines[{lineNumber}].productId", "The selected product is not available in the current organization.");
            else if (!string.IsNullOrWhiteSpace(catalogCode)
                     && !string.Equals(product.Mxik, catalogCode, StringComparison.Ordinal))
            {
                AddPreviewError(errors, "PRODUCT_MXIK_MISMATCH", $"lines[{lineNumber}].productId", "The selected product MXIK does not match the provider catalog code.");
                return null;
            }

            return product;
        }

        if (string.IsNullOrWhiteSpace(catalogCode))
        {
            AddPreviewError(errors, "PRODUCT_MAPPING_REQUIRED", $"lines[{lineNumber}].productId", "The provider line did not contain a catalog code.");
            return null;
        }

        var matches = await _productQuery.GetAllAsync(
            _queryBuilder.For<Product>()
                .Where(x => x.OrganizationId == organizationId
                            && x.StateId == StateIdConst.ACTIVE
                            && x.Mxik == catalogCode)
                .Build(),
            ct);
        if (matches.Count == 1)
            return matches[0];

        AddPreviewError(
            errors,
            matches.Count > 1 ? "PRODUCT_MXIK_AMBIGUOUS" : "PRODUCT_MAPPING_REQUIRED",
            $"lines[{lineNumber}].productId",
            matches.Count > 1
                ? "More than one local product matches the provider catalog code."
                : "No local product matches the provider catalog code; manual mapping is required.");
        return null;
    }

    private async Task<Unit?> ResolveUnitAsync(
        EdoDocumentPreviewLineDto sourceLine,
        Product? product,
        short? requestedId,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        CancellationToken ct)
    {
        if (!requestedId.HasValue)
        {
            AddPreviewError(
                errors,
                "UNIT_MAPPING_REQUIRED",
                $"lines[{sourceLine.Number}].unitId",
                "An active local unit must be selected explicitly; automatic unit creation or provider-code matching is disabled.");
            return null;
        }

        var unit = await _unitQuery.GetAsync(
            _queryBuilder.For<Unit>()
                .Where(x => x.Id == requestedId.Value && x.StateId == StateIdConst.ACTIVE)
                .Build(),
            ct);

        if (unit is null)
        {
            AddPreviewError(
                errors,
                "UNIT_MAPPING_REQUIRED",
                $"lines[{sourceLine.Number}].unitId",
                "The selected local unit is missing or inactive.");
            return null;
        }

        if (product is not null && !IsSelectedUnitValid(product, unit))
        {
            AddPreviewError(
                errors,
                "UNIT_PRODUCT_MISMATCH",
                $"lines[{sourceLine.Number}].unitId",
                "The selected unit does not match the selected product unit.");
            return null;
        }

        return unit;
    }

    internal static bool IsSelectedContractValid(
        Contract contract,
        int organizationId,
        int? counterpartyId,
        DateTime documentDate) =>
        contract.OrganizationId == organizationId
        && contract.StateId == StateIdConst.ACTIVE
        && (!counterpartyId.HasValue || contract.CounterpartyId == counterpartyId.Value)
        && (!contract.StartDate.HasValue || contract.StartDate.Value <= documentDate)
        && (!contract.EndDate.HasValue || contract.EndDate.Value >= documentDate);

    internal static bool IsSelectedUnitValid(Product product, Unit unit) =>
        unit.StateId == StateIdConst.ACTIVE
        && unit.Id == product.UnitId;

    internal static string? GetMarkingValidationErrorCode(
        Product? product,
        decimal? quantity,
        IReadOnlyCollection<string> providerMarkings,
        IReadOnlyCollection<PurchaseDocLineItemDto> requestItems)
    {
        if (product is null || product.IsService)
            return null;
        if (!product.IsPieceTracked)
            return providerMarkings.Count > 0 || requestItems.Count > 0
                ? "PRODUCT_PIECE_TRACKING_REQUIRED"
                : null;

        var requestMarkings = requestItems
            .Select(item => item.MarkingNumber?.Trim())
            .ToList();
        if (requestMarkings.Count == 0 || requestMarkings.Any(string.IsNullOrWhiteSpace))
            return "MARKING_MAPPING_REQUIRED";

        if (requestMarkings.Distinct(StringComparer.OrdinalIgnoreCase).Count() != requestMarkings.Count)
            return "MARKING_MAPPING_DUPLICATE";

        if (!quantity.HasValue
            || quantity.Value <= 0
            || quantity.Value != decimal.Truncate(quantity.Value)
            || requestMarkings.Count != (int)quantity.Value)
        {
            return "MARKING_MAPPING_COUNT_MISMATCH";
        }

        var providerSet = providerMarkings
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToHashSet(StringComparer.Ordinal);
        if (providerSet.Count == 0)
            return "MARKING_PROVIDER_DATA_REQUIRED";

        var requestSet = requestMarkings
            .Select(code => code!)
            .ToHashSet(StringComparer.Ordinal);
        return requestSet.SetEquals(providerSet)
            ? null
            : "MARKING_MAPPING_MISMATCH";
    }

    private static string GetMarkingValidationMessage(string errorCode) => errorCode switch
    {
        "MARKING_MAPPING_REQUIRED" => "The selected piece-tracked product requires marking items.",
        "MARKING_MAPPING_DUPLICATE" => "Duplicate marking numbers are not allowed.",
        "MARKING_MAPPING_COUNT_MISMATCH" => "The marking item count must equal the provider line quantity.",
        "MARKING_PROVIDER_DATA_REQUIRED" => "Provider marking data is required to verify a piece-tracked product.",
        "PRODUCT_PIECE_TRACKING_REQUIRED" => "Marked goods require a piece-tracked local product.",
        _ => "Purchase marking items must exactly match the provider document markings."
    };

    private async Task<VatRate?> ResolveVatRateAsync(
        decimal? providerRate,
        short? requestedId,
        DateOnly? documentDate,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        int lineNumber,
        CancellationToken ct)
    {
        VatRate? vatRate;
        if (requestedId.HasValue)
        {
            vatRate = await _vatRateQuery.GetAsync(
                _queryBuilder.For<VatRate>()
                    .Where(x => x.Id == requestedId.Value && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct);
        }
        else if (providerRate.HasValue)
        {
            var date = documentDate ?? DateOnly.FromDateTime(DateTime.Today);
            var matches = await _vatRateQuery.GetAllAsync(
                _queryBuilder.For<VatRate>()
                    .Where(x => x.StateId == StateIdConst.ACTIVE
                                && x.Rate == providerRate.Value
                                && (x.EffectiveFrom == null || x.EffectiveFrom <= date)
                                && (x.EffectiveTo == null || x.EffectiveTo >= date))
                    .Build(),
                ct);
            vatRate = matches.Count == 1 ? matches[0] : null;
            if (matches.Count > 1)
                AddPreviewError(errors, "VAT_RATE_AMBIGUOUS", $"lines[{lineNumber}].vatRateId", "More than one active VAT rate matches the provider VAT rate.");
        }
        else
        {
            vatRate = null;
        }

        if (vatRate is null && !errors.Any(x => x.Code == "VAT_RATE_AMBIGUOUS" && x.Field == $"lines[{lineNumber}].vatRateId"))
            AddPreviewError(errors, "VAT_RATE_MAPPING_REQUIRED", $"lines[{lineNumber}].vatRateId", "No existing local VAT rate matches the provider VAT rate.");

        return vatRate;
    }

    private async Task<PurchaseDocPreviewReferenceDto> ResolveCurrencyAsync(
        int organizationId,
        short? requestedId,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        CancellationToken ct)
    {
        if (requestedId.HasValue)
        {
            var currency = await _currencyQuery.GetAsync(
                _queryBuilder.For<Currency>()
                    .Where(x => x.Id == requestedId.Value && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct);
            if (currency is null)
            {
                AddPreviewError(errors, "CURRENCY_NOT_FOUND", "currencyId", "The selected currency is not active.");
                return new PurchaseDocPreviewReferenceDto { RequiresSelection = true };
            }

            return new PurchaseDocPreviewReferenceDto
            {
                Id = currency.Id,
                Name = currency.Name,
                IsResolved = true
            };
        }

        var config = await _organizationConfigQuery.GetAsync(
            _queryBuilder.For<OrganizationConfig>()
                .Where(x => x.OrganizationId == organizationId)
                .Build(),
            ct);
        var defaultCurrency = config?.BaseCurrencyId is short baseCurrencyId
            ? await _currencyQuery.GetAsync(
                _queryBuilder.For<Currency>()
                    .Where(x => x.Id == baseCurrencyId && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct)
            : null;
        AddPreviewError(errors, "CURRENCY_SELECTION_REQUIRED", "currencyId", "Provider currency is unavailable; select a currency explicitly.");
        return new PurchaseDocPreviewReferenceDto
        {
            Id = defaultCurrency?.Id,
            Name = defaultCurrency?.Name,
            IsSuggested = defaultCurrency is not null,
            RequiresSelection = true
        };
    }

    private async Task<PurchaseDocPreviewReferenceDto> ResolveWarehouseAsync(
        int organizationId,
        int? requestedId,
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        CancellationToken ct)
    {
        if (requestedId.HasValue)
        {
            var warehouse = await _warehouseQuery.GetAsync(
                _queryBuilder.For<Warehouse>()
                    .Where(x => x.Id == requestedId.Value
                                && x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct);
            if (warehouse is null)
            {
                AddPreviewError(errors, "WAREHOUSE_NOT_FOUND", "warehouseId", "The selected warehouse is not available in the current organization.");
                return new PurchaseDocPreviewReferenceDto { RequiresSelection = true };
            }

            return new PurchaseDocPreviewReferenceDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                IsResolved = true
            };
        }

        var defaults = await _organizationDefaultQuery.GetAsync(
            _queryBuilder.For<OrganizationDefault>()
                .Where(x => x.OrganizationId == organizationId)
                .Build(),
            ct);
        var defaultWarehouse = defaults?.WarehouseId is int warehouseId
            ? await _warehouseQuery.GetAsync(
                _queryBuilder.For<Warehouse>()
                    .Where(x => x.Id == warehouseId
                                && x.OrganizationId == organizationId
                                && x.StateId == StateIdConst.ACTIVE)
                    .Build(),
                ct)
            : null;
        AddPreviewError(errors, "WAREHOUSE_SELECTION_REQUIRED", "warehouseId", "Provider warehouse is unavailable; select a warehouse explicitly.");
        return new PurchaseDocPreviewReferenceDto
        {
            Id = defaultWarehouse?.Id,
            Name = defaultWarehouse?.Name,
            IsSuggested = defaultWarehouse is not null,
            RequiresSelection = true
        };
    }

    private static void EnsureCapability(IEdoProvider provider, EdoCapabilityKind capability)
    {
        var status = provider.Capabilities.Capabilities
            .FirstOrDefault(item => item.Kind == capability)?.Status;
        if (status != EdoCapabilityStatus.SUPPORTED)
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(),
                capability.ToString(),
                status?.ToString() ?? EdoCapabilityStatus.UNKNOWN.ToString());
    }

    private static void AddPreviewError(
        ICollection<PurchaseDocPreviewValidationErrorDto> errors,
        string code,
        string field,
        string message) =>
        errors.Add(new PurchaseDocPreviewValidationErrorDto
        {
            Code = code,
            Field = field,
            Message = message
        });

    private async Task<Result<List<PurchaseDocProduct>>> BuildAllLinesAsync(
        int organizationId,
        List<PurchaseDocLineDto> productLineDtos,
        CancellationToken ct)
    {
        var allLines = new List<PurchaseDocProduct>();

        if (productLineDtos.Count > 0)
        {
            var productResult = await BuildProductLinesAsync(organizationId, productLineDtos, ct);
            if (!productResult.IsSuccess)
                return Result.Failure<List<PurchaseDocProduct>>(productResult.Error);

            allLines.AddRange(productResult.Value);
        }

        return allLines;
    }

    private async Task<Result<List<PurchaseDocProduct>>> BuildProductLinesAsync(
        int organizationId, List<PurchaseDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<PurchaseDocProduct>();
        var markingNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var productIds = lineDtos.Select(x => x.ProductId).Distinct().ToList();
        var unitIds = lineDtos.Select(x => x.UnitId).Distinct().ToList();
        var vatRateIds = lineDtos.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();

        var productsQuery = _queryBuilder.For<Product>()
            .Where(x => productIds.Contains(x.Id))
            .Build();
        var products = await _productQuery.GetAllAsync(productsQuery, ct);
        var productById = products.ToDictionary(x => x.Id);

        var unitsQuery = _queryBuilder.For<Unit>()
            .Where(x => unitIds.Contains(x.Id))
            .Build();
        var units = await _unitQuery.GetAllAsync(unitsQuery, ct);
        var unitIdsFound = units.Select(x => x.Id).ToHashSet();

        var vatRateById = new Dictionary<short, VatRate>();
        if (vatRateIds.Count > 0)
        {
            var vatRatesQuery = _queryBuilder.For<VatRate>()
                .Where(x => vatRateIds.Contains(x.Id))
                .Build();
            var vatRates = await _vatRateQuery.GetAllAsync(vatRatesQuery, ct);
            vatRateById = vatRates.ToDictionary(x => x.Id);
        }

        foreach (var dto in lineDtos)
        {
            if (!productById.TryGetValue(dto.ProductId, out var product))
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocErrors.ProductNotFound(dto.ProductId, _userContext.LanguageId));

            if (!unitIdsFound.Contains(dto.UnitId))
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocErrors.UnitNotFound(dto.UnitId, _userContext.LanguageId));

            if (dto.Quantity <= 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductQuantity(dto.ProductId, dto.Quantity, _userContext.LanguageId));

            if (dto.UnitPrice < 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductUnitPrice(dto.ProductId, dto.UnitPrice, _userContext.LanguageId));

            if (product.IsService)
            {
                if (dto.Items.Count > 0)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocErrors.ServiceItemsNotAllowed(dto.ProductId, _userContext.LanguageId));
            }
            else if (!product.IsPieceTracked)
            {
                if (dto.Items.Count > 0)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocErrors.ProductPieceTrackingRequired(dto.ProductId, _userContext.LanguageId));
            }
            else
            {
                if (dto.Quantity != decimal.Truncate(dto.Quantity))
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.InvalidProductQuantity(dto.ProductId, dto.Quantity, _userContext.LanguageId));

                if (dto.Items.Count == 0)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.ProductItemsRequired(dto.ProductId, _userContext.LanguageId));

                if (dto.Quantity != dto.Items.Count)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.ProductQuantityItemsMismatch(dto.ProductId, dto.Quantity, dto.Items.Count, _userContext.LanguageId));

                foreach (var item in dto.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.MarkingNumber))
                        return Result.Failure<List<PurchaseDocProduct>>(
                            PurchaseDocTableErrors.MarkingNumberRequired(dto.ProductId, _userContext.LanguageId));

                    if (!markingNumbers.Add(item.MarkingNumber.Trim()))
                        return Result.Failure<List<PurchaseDocProduct>>(
                            PurchaseDocTableErrors.DuplicateMarkingNumber(item.MarkingNumber, _userContext.LanguageId));
                }
            }

            var vatRateId = dto.VatRateId;
            VatRate? vatRate = null;

            if (vatRateId.HasValue)
            {
                if (!vatRateById.TryGetValue(vatRateId.Value, out vatRate))
                    return Result.Failure<List<PurchaseDocProduct>>(PurchaseDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));
            }

            var lineAmounts = ResolveLineAmounts(dto, vatRate?.Rate);

            var itemVatAmounts = product.IsService
                ? new List<decimal>()
                : SplitAmount(lineAmounts.VatAmount, dto.Items.Count, lineAmounts.IsProviderSourced ? 2 : 8);

            lines.Add(new PurchaseDocProduct
            {
                ProductId = dto.ProductId,
                UnitId = dto.UnitId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Amount = lineAmounts.Amount,
                VatRateId = vatRateId,
                DebitAccountId = dto.DebitAccountId,
                VatAccountId = dto.VatAccountId,
                VatAmount = lineAmounts.VatAmount,
                TotalAmount = lineAmounts.TotalAmount,
                PurchaseDocTables = product.IsService
                    ? new List<PurchaseDocTable>()
                    : dto.Items.Select((item, index) => new PurchaseDocTable
                    {
                        Amount = dto.UnitPrice,
                        VatRateId = vatRateId,
                        VatAmount = itemVatAmounts[index],
                        TotalAmount = dto.UnitPrice + itemVatAmounts[index],
                        ProductTable = new ProductTable
                        {
                            ProductId = dto.ProductId,
                            SerialNumber = item.SerialNumber,
                            MarkingNumber = item.MarkingNumber.Trim(),
                            CreatedDate = DateTime.Now,
                        }
                    }).ToList()
            });
        }

        return lines;
    }

    internal static PurchaseDocLineAmounts ResolveLineAmounts(PurchaseDocLineDto dto, decimal? vatRate)
    {
        if (dto.ProviderVatAmount.HasValue && dto.ProviderTotalAmount.HasValue)
        {
            var vatAmount = RoundMoney(dto.ProviderVatAmount.Value);
            var totalAmount = RoundMoney(dto.ProviderTotalAmount.Value);
            var amount = dto.ProviderNetAmount.HasValue
                ? RoundMoney(dto.ProviderNetAmount.Value)
                : RoundMoney(totalAmount - vatAmount);

            return new PurchaseDocLineAmounts(amount, vatAmount, totalAmount, IsProviderSourced: true);
        }

        var calculatedAmount = dto.UnitPrice * dto.Quantity;
        var calculatedVatAmount = vatRate.HasValue
            ? Math.Round(calculatedAmount * vatRate.Value / 100m, 8)
            : 0m;
        return new PurchaseDocLineAmounts(
            calculatedAmount,
            calculatedVatAmount,
            calculatedAmount + calculatedVatAmount,
            IsProviderSourced: false);
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static List<decimal> SplitAmount(decimal amount, int count, int precision = 8)
    {
        if (count <= 0)
            return new List<decimal>();

        var split = Math.Round(amount / count, precision, MidpointRounding.AwayFromZero);
        var result = Enumerable.Repeat(split, count).ToList();
        result[^1] += amount - result.Sum();
        return result;
    }

    private sealed record PurchaseTableLink(long OwnerId, int ProductTableId);

    internal readonly record struct PurchaseDocLineAmounts(
        decimal Amount,
        decimal VatAmount,
        decimal TotalAmount,
        bool IsProviderSourced);
}
