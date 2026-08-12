using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.Fa;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaCommissionings;

public class FaCommissioningService : BaseService, IFaCommissioningService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IFaCommissioningLifecycleService _lifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IFaDocumentAccountValidator _accountValidator;
    private readonly IQueryRepository<FaCommissioningDoc> _query;
    private readonly IQueryRepository<FaCommissioningDocLine> _lineQuery;
    private readonly IQueryRepository<FaAsset> _assetQuery;
    private readonly IQueryRepository<FaDepreciationMethod> _depreciationMethodQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IFaCommissioningCommandRepository _command;

    public FaCommissioningService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IFaCommissioningLifecycleService lifecycleService,
        IDocumentNumberService documentNumberService,
        IFaDocumentAccountValidator accountValidator,
        IQueryRepository<FaCommissioningDoc> query,
        IQueryRepository<FaCommissioningDocLine> lineQuery,
        IQueryRepository<FaAsset> assetQuery,
        IQueryRepository<FaDepreciationMethod> depreciationMethodQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<User> userQuery,
        IFaCommissioningCommandRepository command,
        ILogger<FaCommissioningService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _lifecycleService = lifecycleService;
        _documentNumberService = documentNumberService;
        _accountValidator = accountValidator;
        _query = query;
        _lineQuery = lineQuery;
        _assetQuery = assetQuery;
        _depreciationMethodQuery = depreciationMethodQuery;
        _departmentQuery = departmentQuery;
        _userQuery = userQuery;
        _command = command;
    }

    public Task<Result<PagedResponse<FaCommissioningListDto>>> GetAllAsync(
        FaCommissioningListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<
                FaCommissioningDoc,
                FaCommissioningListDto,
                FaCommissioningListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(
                PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaCommissioningDto>> GetByIdAsync(
        long id,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetByIdInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<FaCommissioningDto>(
                    FaCommissioningErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(
        FaCommissioningCreateDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
            {
                return Result.Failure<long>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            }

            var organizationId = _userContext.OrganizationId.Value;
            var linesResult = await BuildLinesAsync(
                organizationId,
                dto.Lines,
                null,
                ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var accountValidation = await ValidateAccountsAsync(
                organizationId,
                linesResult.Value,
                ct);
            if (!accountValidation.IsSuccess)
                return Result.Failure<long>(accountValidation.Error);

            var numberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.FACOMMISSIONING,
                dto.DocDate,
                ct);
            if (!numberResult.IsSuccess)
                return Result.Failure<long>(numberResult.Error);

            var now = DateTime.Now;
            var document = new FaCommissioningDoc
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                DocNumber = numberResult.Value.DocumentNumber,
                DocDate = NormalizeDateTime(dto.DocDate),
                StatusId = DocumentStatusIdConst.DRAFT,
                Note = dto.Note?.Trim(),
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                UpdatedDate = now,
                UpdatedByUserId = _userContext.Id,
                Lines = linesResult.Value
            };

            await _command.CreateAsync(document, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var createdDto = await GetByIdInternalAsync(document.Id, ct);
            if (createdDto is not null)
            {
                _auditLogService.SetNewValues(createdDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaCommissioningDoc,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Create);
            }

            return Result.Success(document.Id);
        }, ct);

    public Task<Result> UpdateAsync(
        long id,
        FaCommissioningUpdateDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var document = await GetAggregateAsync(id, ct);
            if (document is null)
            {
                return Result.Failure(
                    FaCommissioningErrors.NotFound(id, _userContext.LanguageId));
            }

            if (document.StatusId != DocumentStatusIdConst.DRAFT)
            {
                return Result.Failure(
                    FaCommissioningErrors.CannotUpdate(
                        id,
                        document.StatusId,
                        _userContext.LanguageId));
            }

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var linesResult = await BuildLinesAsync(
                _userContext.OrganizationId.Value,
                dto.Lines,
                id,
                ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            var accountValidation = await ValidateAccountsAsync(
                document.OrganizationId,
                linesResult.Value,
                ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            if (document.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(document.Lines.ToList(), ct);
                document.Lines.Clear();
            }

            foreach (var line in linesResult.Value)
                document.Lines.Add(line);

            document.DocDate = NormalizeDateTime(dto.DocDate);
            document.Note = dto.Note?.Trim();
            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(document, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaCommissioningDoc,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    private async Task<Result<List<FaCommissioningDocLine>>> BuildLinesAsync(
        int organizationId,
        IReadOnlyCollection<FaCommissioningLineWriteDto> lineDtos,
        long? currentDocumentId,
        CancellationToken ct)
    {
        if (lineDtos.Count == 0)
        {
            return Result.Failure<List<FaCommissioningDocLine>>(
                FaCommissioningErrors.LinesRequired(_userContext.LanguageId));
        }

        var duplicateAsset = lineDtos
            .GroupBy(line => line.FaAssetId)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateAsset is not null)
        {
            return Result.Failure<List<FaCommissioningDocLine>>(
                FaCommissioningErrors.DuplicateAsset(
                    duplicateAsset.Key,
                    _userContext.LanguageId));
        }

        var assetIds = lineDtos.Select(line => line.FaAssetId).Distinct().ToList();
        var assetQuery = _queryBuilder.For<FaAsset>()
            .Where(asset =>
                assetIds.Contains(asset.Id) &&
                asset.OrganizationId == organizationId)
            .Build();
        assetQuery.AddIncludes(include =>
            include.Include(asset => asset.FaAssetAccounting)
                .ThenInclude(accounting => accounting!.AssetAccount));
        assetQuery.AddIncludes(include =>
            include.Include(asset => asset.FaReceiptDocAsset)
                .ThenInclude(receiptAsset => receiptAsset!.ReceiptDocLine)
                .ThenInclude(receiptLine => receiptLine.ReceiptDoc));

        var assets = await _assetQuery.GetAllAsync(assetQuery, ct);
        var assetsById = assets.ToDictionary(asset => asset.Id);

        var occupiedAssetIds = await LoadOccupiedAssetIdsAsync(
            assetIds,
            currentDocumentId,
            ct);

        var depreciationMethodIds = lineDtos
            .Select(line => line.DepreciationMethodId)
            .Distinct()
            .ToList();
        var depreciationMethods = await LoadDepreciationMethodsAsync(
            depreciationMethodIds,
            ct);

        var departmentIds = lineDtos
            .Where(line => line.DepartmentId.HasValue)
            .Select(line => line.DepartmentId!.Value)
            .Distinct()
            .ToList();
        var departments = await LoadDepartmentsAsync(
            departmentIds,
            organizationId,
            ct);

        var userIds = lineDtos
            .Where(line => line.ResponsibleUserId.HasValue)
            .Select(line => line.ResponsibleUserId!.Value)
            .Distinct()
            .ToList();
        var users = await LoadUsersAsync(userIds, organizationId, ct);

        var result = new List<FaCommissioningDocLine>();
        foreach (var lineDto in lineDtos)
        {
            if (!assetsById.TryGetValue(lineDto.FaAssetId, out var asset))
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.AssetNotFound(
                        lineDto.FaAssetId,
                        _userContext.LanguageId));
            }

            if (asset.StateId != StateIdConst.ACTIVE ||
                asset.StatusId != FaAssetStatusIdConst.NOT_COMMISSIONED ||
                occupiedAssetIds.Contains(asset.Id))
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.AssetUnavailable(
                        asset.Id,
                        _userContext.LanguageId));
            }

            var accounting = asset.FaAssetAccounting;
            if (accounting is null)
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.AccountingNotFound(
                        asset.Id,
                        _userContext.LanguageId));
            }

            var receiptAsset = asset.FaReceiptDocAsset;
            if (receiptAsset?.ReceiptDocLine.ReceiptDoc.StatusId !=
                DocumentStatusIdConst.POSTED)
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.ReceiptNotPosted(
                        asset.Id,
                        _userContext.LanguageId));
            }

            var capitalInvestmentAccountId =
                receiptAsset.ReceiptDocLine.CapitalInvestmentAccountId;
            if (!capitalInvestmentAccountId.HasValue)
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.CapitalInvestmentAccountMissing(
                        asset.Id,
                        _userContext.LanguageId));
            }

            if (!depreciationMethods.TryGetValue(
                    lineDto.DepreciationMethodId,
                    out var depreciationMethod))
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.ReferenceNotFound(
                        "Depreciation method",
                        lineDto.DepreciationMethodId,
                        _userContext.LanguageId));
            }

            if (lineDto.DepartmentId.HasValue &&
                !departments.Contains(lineDto.DepartmentId.Value))
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.ReferenceNotFound(
                        "Department",
                        lineDto.DepartmentId.Value,
                        _userContext.LanguageId));
            }

            if (lineDto.ResponsibleUserId.HasValue &&
                !users.Contains(lineDto.ResponsibleUserId.Value))
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.ReferenceNotFound(
                        "Responsible user",
                        lineDto.ResponsibleUserId.Value,
                        _userContext.LanguageId));
            }

            if (lineDto.SalvageValue > accounting.InitialCost)
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.SalvageValueTooHigh(
                        asset.Id,
                        _userContext.LanguageId));
            }

            if (depreciationMethod.Code ==
                    FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION &&
                !lineDto.PlannedUnitsTotal.HasValue)
            {
                return Result.Failure<List<FaCommissioningDocLine>>(
                    FaCommissioningErrors.PlannedUnitsRequired(
                        asset.Id,
                        _userContext.LanguageId));
            }

            result.Add(new FaCommissioningDocLine
            {
                FaAssetId = asset.Id,
                FaAsset = asset,
                CapitalizedAmount = accounting.InitialCost,
                DeprStartDate = NormalizeDateTime(lineDto.DeprStartDate),
                SalvageValue = lineDto.SalvageValue,
                UsefulLifeMonths = lineDto.UsefulLifeMonths,
                DepreciationMethodId = lineDto.DepreciationMethodId,
                PlannedUnitsTotal = lineDto.PlannedUnitsTotal,
                DepartmentId = lineDto.DepartmentId,
                ResponsibleUserId = lineDto.ResponsibleUserId,
                CapitalInvestmentAccountId = capitalInvestmentAccountId.Value,
                AccumulatedDepreciationAccountId =
                    lineDto.AccumulatedDepreciationAccountId,
                DepreciationExpenseAccountId =
                    lineDto.DepreciationExpenseAccountId,
                Note = lineDto.Note?.Trim()
            });
        }

        return Result.Success(result);
    }

    private async Task<HashSet<long>> LoadOccupiedAssetIdsAsync(
        IReadOnlyCollection<long> assetIds,
        long? currentDocumentId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<FaCommissioningDocLine>()
            .Where(line =>
                assetIds.Contains(line.FaAssetId) &&
                (!currentDocumentId.HasValue ||
                 line.CommissioningDocId != currentDocumentId.Value) &&
                line.CommissioningDoc.StateId == StateIdConst.ACTIVE &&
                line.CommissioningDoc.StatusId != DocumentStatusIdConst.CANCELLED)
            .As(line => line.FaAssetId)
            .Build();
        return (await _lineQuery.GetAllAsync(query, ct)).ToHashSet();
    }

    private async Task<Dictionary<short, FaDepreciationMethod>>
        LoadDepreciationMethodsAsync(
            IReadOnlyCollection<short> ids,
            CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDepreciationMethod>()
            .Where(method =>
                ids.Contains(method.Id) &&
                method.StateId == StateIdConst.ACTIVE)
            .Build();
        return (await _depreciationMethodQuery.GetAllAsync(query, ct))
            .ToDictionary(method => method.Id);
    }

    private async Task<HashSet<int>> LoadDepartmentsAsync(
        IReadOnlyCollection<int> ids,
        int organizationId,
        CancellationToken ct)
    {
        if (ids.Count == 0)
            return [];

        var query = _queryBuilder.For<Department>()
            .Where(department =>
                ids.Contains(department.Id) &&
                department.OrganizationId == organizationId &&
                department.StateId == StateIdConst.ACTIVE)
            .As(department => department.Id)
            .Build();
        return (await _departmentQuery.GetAllAsync(query, ct)).ToHashSet();
    }

    private async Task<HashSet<int>> LoadUsersAsync(
        IReadOnlyCollection<int> ids,
        int organizationId,
        CancellationToken ct)
    {
        if (ids.Count == 0)
            return [];

        var query = _queryBuilder.For<User>()
            .Where(user =>
                ids.Contains(user.Id) &&
                user.StateId == StateIdConst.ACTIVE &&
                user.UserOrganizations.Any(membership =>
                    membership.OrganizationId == organizationId &&
                    membership.StateId == StateIdConst.ACTIVE))
            .As(user => user.Id)
            .Build();
        return (await _userQuery.GetAllAsync(query, ct)).ToHashSet();
    }

    private Task<Result> ValidateAccountsAsync(
        int organizationId,
        IReadOnlyCollection<FaCommissioningDocLine> lines,
        CancellationToken ct)
    {
        var requirements = lines
            .SelectMany(line => new[]
            {
                new FaDocumentAccountRequirement(
                    line.CapitalInvestmentAccountId,
                    FaDocumentAccountRoleCodeConst.CapitalInvestment),
                new FaDocumentAccountRequirement(
                    line.FaAsset.FaAssetAccounting?.AssetAccountId,
                    FaDocumentAccountRoleCodeConst.FixedAsset),
                new FaDocumentAccountRequirement(
                    line.AccumulatedDepreciationAccountId,
                    FaDocumentAccountRoleCodeConst.AccumulatedDepreciation),
                new FaDocumentAccountRequirement(
                    line.DepreciationExpenseAccountId,
                    FaDocumentAccountRoleCodeConst.DepreciationExpense)
            })
            .ToList();

        return _accountValidator.ValidateAsync(
            organizationId,
            DocumentTypeIdConst.FACOMMISSIONING,
            requirements,
            ct);
    }

    private async Task<FaCommissioningDoc?> GetAggregateAsync(
        long id,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<FaCommissioningDoc>()
            .Where(document => document.Id == id)
            .Build();
        query.AddIncludes(include => include.Include(document => document.Lines));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaCommissioningDto?> GetByIdInternalAsync(
        long id,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<FaCommissioningDoc>()
            .Where(document => document.Id == id)
            .As<FaCommissioningDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
}

