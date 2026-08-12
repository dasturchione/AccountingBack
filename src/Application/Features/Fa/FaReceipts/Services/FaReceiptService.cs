using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.Fa;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaReceipts;

public class FaReceiptService : BaseService, IFaReceiptService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IFaReceiptLifecycleService _lifecycleService;
    private readonly IAuditLogService _auditLogService;
    private readonly IFaDocumentAccountValidator _accountValidator;
    private readonly IQueryRepository<FaReceiptDoc> _query;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<FaReceiptType> _receiptTypeQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly IQueryRepository<FaGroup> _faGroupQuery;
    private readonly IQueryRepository<FaOkof> _okofQuery;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IFaReceiptCommandRepository _command;
    private readonly IDocumentNumberService _documentNumberService;

    public FaReceiptService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IFaReceiptLifecycleService lifecycleService,
        IAuditLogService auditLogService,
        IFaDocumentAccountValidator accountValidator,
        IQueryRepository<FaReceiptDoc> query,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<FaReceiptType> receiptTypeQuery,
        IQueryRepository<VatRate> vatRateQuery,
        IQueryRepository<FaGroup> faGroupQuery,
        IQueryRepository<FaOkof> okofQuery,
        IQueryRepository<FaAsset> faAssetQuery,
        IFaReceiptCommandRepository command,
        IDocumentNumberService documentNumberService,
        ILogger<FaReceiptService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _lifecycleService = lifecycleService;
        _auditLogService = auditLogService;
        _accountValidator = accountValidator;
        _query = query;
        _counterpartyQuery = counterpartyQuery;
        _currencyQuery = currencyQuery;
        _receiptTypeQuery = receiptTypeQuery;
        _vatRateQuery = vatRateQuery;
        _faGroupQuery = faGroupQuery;
        _okofQuery = okofQuery;
        _faAssetQuery = faAssetQuery;
        _command = command;
        _documentNumberService = documentNumberService;
    }

    public Task<Result<PagedResponse<FaReceiptListDto>>> GetAllAsync(
        FaReceiptListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaReceiptDoc, FaReceiptListDto, FaReceiptListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaReceiptDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var entity = await GetByIdInternalAsync(id, ct);
            return entity is null
                ? Result.Failure<FaReceiptDto>(FaReceiptErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(FaReceiptCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var headerValidation = await ValidateHeaderReferencesAsync(dto, organizationId, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure<long>(headerValidation.Error);

            var linesResult = await BuildLinesAsync(organizationId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var accountValidation = await ValidateAccountsAsync(
                organizationId,
                dto.SupplierAccountId,
                dto.Lines,
                ct);
            if (!accountValidation.IsSuccess)
                return Result.Failure<long>(accountValidation.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.FARECEIPT,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var lines = linesResult.Value;
            var doc = new FaReceiptDoc
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = NormalizeDateTime(dto.DocDate),
                CounterpartyId = dto.CounterpartyId,
                CurrencyId = dto.CurrencyId,
                TotalAmount = lines.Sum(line => line.Amount),
                VatAmount = lines.Sum(line => line.VatAmount),
                FinalAmount = lines.Sum(line => line.TotalAmount),
                StatusId = DocumentStatusIdConst.DRAFT,
                ReceiptTypeId = dto.ReceiptTypeId,
                SupplierAccountId = dto.SupplierAccountId,
                CreatedDate = now,
                UpdatedDate = now,
                Lines = lines
            };

            await _command.CreateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto is not null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaReceiptDoc,
                    doc.Id.ToString(),
                    AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, FaReceiptUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaReceiptErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(
                    FaReceiptErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var headerValidation = await ValidateHeaderReferencesAsync(dto, organizationId, ct);
            if (!headerValidation.IsSuccess)
                return headerValidation;

            var linesResult = await BuildLinesAsync(organizationId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            var accountValidation = await ValidateAccountsAsync(
                organizationId,
                dto.SupplierAccountId,
                dto.Lines,
                ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            if (doc.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(doc.Lines.ToList(), ct);
                doc.Lines.Clear();
            }

            foreach (var line in linesResult.Value)
                doc.Lines.Add(line);

            doc.DocDate = NormalizeDateTime(dto.DocDate);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.CurrencyId = dto.CurrencyId;
            doc.TotalAmount = doc.Lines.Sum(line => line.Amount);
            doc.VatAmount = doc.Lines.Sum(line => line.VatAmount);
            doc.FinalAmount = doc.Lines.Sum(line => line.TotalAmount);
            doc.ReceiptTypeId = dto.ReceiptTypeId;
            doc.SupplierAccountId = dto.SupplierAccountId;
            doc.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaReceiptDoc,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaReceiptErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(
                    FaReceiptErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            if (doc.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(doc.Lines.ToList(), ct);
                doc.Lines.Clear();
            }

            doc.StateId = StateIdConst.PASSIVE;
            doc.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaReceiptDoc,
                    id.ToString(),
                    AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    private async Task<Result> ValidateHeaderReferencesAsync(
        FaReceiptBaseDto dto,
        int organizationId,
        CancellationToken ct)
    {
        if (dto.CounterpartyId.HasValue &&
            !await _counterpartyQuery.AnyAsync(counterparty =>
                counterparty.Id == dto.CounterpartyId.Value &&
                counterparty.OrganizationId == organizationId &&
                counterparty.StateId == StateIdConst.ACTIVE, ct))
        {
            return Result.Failure(
                FaReceiptErrors.CounterpartyNotFound(dto.CounterpartyId.Value, _userContext.LanguageId));
        }

        if (!await _currencyQuery.AnyAsync(currency =>
                currency.Id == dto.CurrencyId &&
                currency.StateId == StateIdConst.ACTIVE, ct))
        {
            return Result.Failure(FaReceiptErrors.CurrencyNotFound(dto.CurrencyId, _userContext.LanguageId));
        }

        if (!await _receiptTypeQuery.AnyAsync(receiptType =>
                receiptType.Id == dto.ReceiptTypeId, ct))
        {
            return Result.Failure(
                FaReceiptErrors.InvalidReceiptType(dto.ReceiptTypeId, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<Result<List<FaReceiptDocLine>>> BuildLinesAsync(
        int organizationId,
        IReadOnlyCollection<FaReceiptLineWriteDto> lineDtos,
        CancellationToken ct)
    {
        if (lineDtos.Count == 0)
            return Result.Failure<List<FaReceiptDocLine>>(
                FaReceiptErrors.LinesRequired(_userContext.LanguageId));

        var vatRateIds = lineDtos
            .Where(line => line.VatRateId.HasValue)
            .Select(line => line.VatRateId!.Value)
            .Distinct()
            .ToList();
        var faGroupIds = lineDtos
            .SelectMany(line => line.Assets)
            .Select(asset => asset.FaGroupId)
            .Distinct()
            .ToList();
        var okofIds = lineDtos
            .SelectMany(line => line.Assets)
            .Where(asset => asset.OkofId.HasValue)
            .Select(asset => asset.OkofId!.Value)
            .Distinct()
            .ToList();
        var inventoryNumbers = lineDtos
            .SelectMany(line => line.Assets)
            .Select(asset => asset.InventoryNumber.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var vatRates = await LoadVatRatesAsync(vatRateIds, ct);
        var faGroups = await LoadFaGroupsAsync(faGroupIds, organizationId, ct);
        var okofs = await LoadOkofsAsync(okofIds, ct);

        var existingInventoryNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (inventoryNumbers.Count > 0)
        {
            var query = _queryBuilder.For<FaAsset>()
                .Where(asset =>
                    asset.OrganizationId == organizationId &&
                    inventoryNumbers.Contains(asset.InventoryNumber))
                .As(asset => asset.InventoryNumber)
                .Build();
            existingInventoryNumbers = (await _faAssetQuery.GetAllAsync(query, ct))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        var result = new List<FaReceiptDocLine>();
        var documentInventoryNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var lineDto in lineDtos)
        {
            if (lineDto.VatRateId.HasValue && !vatRates.ContainsKey(lineDto.VatRateId.Value))
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    FaReceiptErrors.VatRateNotFound(lineDto.VatRateId.Value, _userContext.LanguageId));
            }

            if (lineDto.Assets.Count == 0)
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    FaReceiptErrors.AssetLinesRequired(lineDto.Name, _userContext.LanguageId));
            }

            if (lineDto.Quantity != lineDto.Assets.Count)
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    FaReceiptErrors.LineQuantityMismatch(
                        lineDto.Name,
                        lineDto.Quantity,
                        lineDto.Assets.Count,
                        _userContext.LanguageId));
            }

            var amount = Math.Round(lineDto.Price * lineDto.Quantity, 8);
            var vatAmount = lineDto.VatRateId.HasValue
                ? Math.Round(amount * vatRates[lineDto.VatRateId.Value].Rate / 100m, 8)
                : 0m;

            if (vatAmount > 0m && !lineDto.VatAccountId.HasValue)
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    Error.Business(
                        "FaReceipt.VatAccountRequired",
                        "VAT account is required when the receipt line has VAT."));
            }

            var assets = new List<FaReceiptDocAsset>();
            foreach (var assetDto in lineDto.Assets)
            {
                var inventoryNumber = assetDto.InventoryNumber.Trim();
                if (!documentInventoryNumbers.Add(inventoryNumber))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.DuplicateInventoryNumber(inventoryNumber, _userContext.LanguageId));
                }

                if (existingInventoryNumbers.Contains(inventoryNumber))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.InventoryNumberConflict(inventoryNumber, _userContext.LanguageId));
                }

                if (!faGroups.Contains(assetDto.FaGroupId))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.FaGroupNotFound(assetDto.FaGroupId, _userContext.LanguageId));
                }

                if (assetDto.OkofId.HasValue && !okofs.Contains(assetDto.OkofId.Value))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.OkofNotFound(assetDto.OkofId.Value, _userContext.LanguageId));
                }

                assets.Add(new FaReceiptDocAsset
                {
                    InventoryNumber = inventoryNumber,
                    Name = assetDto.Name.Trim(),
                    InitialCost = assetDto.InitialCost,
                    FaGroupId = assetDto.FaGroupId,
                    OkofId = assetDto.OkofId,
                    AssetAccountId = assetDto.AssetAccountId
                });
            }

            var assetInitialCostTotal = assets.Sum(asset => asset.InitialCost);
            if (Math.Abs(assetInitialCostTotal - amount) > 0.01m)
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    FaReceiptErrors.LineAmountMismatch(
                        lineDto.Name,
                        amount,
                        assetInitialCostTotal,
                        _userContext.LanguageId));
            }

            result.Add(new FaReceiptDocLine
            {
                Name = lineDto.Name.Trim(),
                Quantity = lineDto.Quantity,
                Price = lineDto.Price,
                Amount = amount,
                VatRateId = lineDto.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                CapitalInvestmentAccountId = lineDto.CapitalInvestmentAccountId,
                VatAccountId = lineDto.VatAccountId,
                Assets = assets
            });
        }

        return Result.Success(result);
    }

    private Task<Result> ValidateAccountsAsync(
        int organizationId,
        int supplierAccountId,
        IReadOnlyCollection<FaReceiptLineWriteDto> lines,
        CancellationToken ct)
    {
        var requirements = new List<FaDocumentAccountRequirement>
        {
            new(
                supplierAccountId,
                FaDocumentAccountRoleCodeConst.SupplierSettlement)
        };

        foreach (var line in lines)
        {
            requirements.Add(new(
                line.CapitalInvestmentAccountId,
                FaDocumentAccountRoleCodeConst.CapitalInvestment));
            requirements.Add(new(
                line.VatAccountId,
                FaDocumentAccountRoleCodeConst.InputVat,
                line.VatRateId.HasValue));
            requirements.AddRange(line.Assets.Select(asset =>
                new FaDocumentAccountRequirement(
                    asset.AssetAccountId,
                    FaDocumentAccountRoleCodeConst.FixedAsset)));
        }

        return _accountValidator.ValidateAsync(
            organizationId,
            DocumentTypeIdConst.FARECEIPT,
            requirements,
            ct);
    }

    private async Task<FaReceiptDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>()
            .Where(receipt => receipt.Id == id)
            .Build();
        query.AddIncludes(include =>
            include.Include(receipt => receipt.Lines)
                .ThenInclude(line => line.Assets));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaReceiptDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>()
            .Where(receipt => receipt.Id == id)
            .As<FaReceiptDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<Dictionary<short, VatRate>> LoadVatRatesAsync(
        IReadOnlyCollection<short> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<short, VatRate>();

        var query = _queryBuilder.For<VatRate>()
            .Where(rate => ids.Contains(rate.Id) && rate.StateId == StateIdConst.ACTIVE)
            .Build();
        return (await _vatRateQuery.GetAllAsync(query, ct)).ToDictionary(rate => rate.Id);
    }

    private async Task<HashSet<int>> LoadFaGroupsAsync(
        IReadOnlyCollection<int> ids,
        int organizationId,
        CancellationToken ct)
    {
        if (ids.Count == 0)
            return [];

        var query = _queryBuilder.For<FaGroup>()
            .Where(group =>
                ids.Contains(group.Id) &&
                group.OrganizationId == organizationId &&
                group.StateId == StateIdConst.ACTIVE)
            .As(group => group.Id)
            .Build();
        return (await _faGroupQuery.GetAllAsync(query, ct)).ToHashSet();
    }

    private async Task<HashSet<short>> LoadOkofsAsync(
        IReadOnlyCollection<short> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
            return [];

        var query = _queryBuilder.For<FaOkof>()
            .Where(okof => ids.Contains(okof.Id) && okof.StateId == StateIdConst.ACTIVE)
            .As(okof => okof.Id)
            .Build();
        return (await _okofQuery.GetAllAsync(query, ct)).ToHashSet();
    }

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
}
