using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaRevaluations;

public class FaRevaluationService : BaseService, IFaRevaluationService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IFaRevaluationLifecycleService _lifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<FaRevaluationDoc> _query;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IFaRevaluationCommandRepository _command;

    public FaRevaluationService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IFaRevaluationLifecycleService lifecycleService,
        IDocumentNumberService documentNumberService,
        IQueryRepository<FaRevaluationDoc> query,
        IQueryRepository<FaAsset> faAssetQuery,
        IFaRevaluationCommandRepository command,
        ILogger<FaRevaluationService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _lifecycleService = lifecycleService;
        _documentNumberService = documentNumberService;
        _query = query;
        _faAssetQuery = faAssetQuery;
        _command = command;
    }

    public Task<Result<PagedResponse<FaRevaluationListDto>>> GetAllAsync(FaRevaluationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaRevaluationDoc, FaRevaluationListDto, FaRevaluationListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaRevaluationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var item = await GetByIdInternalAsync(id, ct);
            return item is null
                ? Result.Failure<FaRevaluationDto>(FaRevaluationErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(item);
        });

    public Task<Result<long>> CreateAsync(FaRevaluationCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var linesResult = await BuildLinesAsync(_userContext.OrganizationId.Value, dto, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                _userContext.OrganizationId.Value,
                DocumentTypeIdConst.FAREVALUATION,
                dto.RevaluationDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var doc = new FaRevaluationDoc
            {
                OrganizationId = _userContext.OrganizationId.Value,
                StateId = StateIdConst.ACTIVE,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                RevaluationDate = NormalizeDateTime(dto.RevaluationDate),
                Reason = dto.Reason?.Trim(),
                RevaluationReserveAccountId = dto.RevaluationReserveAccountId,
                RevaluationLossAccountId = dto.RevaluationLossAccountId,
                StatusId = DocumentStatusIdConst.DRAFT,
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                UpdatedDate = now,
                UpdatedByUserId = _userContext.Id,
                Lines = linesResult.Value
            };

            await _command.CreateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var dtoResult = await GetByIdInternalAsync(doc.Id, ct);
            if (dtoResult is not null)
            {
                _auditLogService.SetNewValues(dtoResult);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaRevaluationDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, FaRevaluationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaRevaluationErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(FaRevaluationErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var linesResult = await BuildLinesAsync(_userContext.OrganizationId.Value, dto, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            if (doc.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(doc.Lines.ToList(), ct);
                doc.Lines.Clear();
            }

            foreach (var line in linesResult.Value)
                doc.Lines.Add(line);

            doc.RevaluationDate = NormalizeDateTime(dto.RevaluationDate);
            doc.Reason = dto.Reason?.Trim();
            doc.RevaluationReserveAccountId = dto.RevaluationReserveAccountId;
            doc.RevaluationLossAccountId = dto.RevaluationLossAccountId;
            doc.StateId = dto.StateId;
            doc.UpdatedDate = DateTime.Now;
            doc.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaRevaluationDoc, id.ToString(), AuditLogOperationTypeConst.Update);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => _lifecycleService.CancelAsync(id, ct);

    private async Task<Result<List<FaRevaluationDocLine>>> BuildLinesAsync(int organizationId, FaRevaluationBaseDto dto, CancellationToken ct)
    {
        if (dto.Lines.Count == 0)
            return Result.Failure<List<FaRevaluationDocLine>>(FaRevaluationErrors.LinesRequired(_userContext.LanguageId));

        var assetIds = dto.Lines.Select(x => x.FaAssetId).Distinct().ToList();
        if (assetIds.Count != dto.Lines.Count)
        {
            var duplicateId = dto.Lines.GroupBy(x => x.FaAssetId).First(x => x.Count() > 1).Key;
            return Result.Failure<List<FaRevaluationDocLine>>(FaRevaluationErrors.DuplicateAsset(duplicateId, _userContext.LanguageId));
        }

        var assetQuery = _queryBuilder.For<FaAsset>()
            .Where(x => assetIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .Build();
        assetQuery.AddIncludes(x => x.Include(a => a.Status));
        assetQuery.AddIncludes(x => x.Include(a => a.FaAssetAccounting));
        var assets = await _faAssetQuery.GetAllAsync(assetQuery, ct);
        var assetById = assets.ToDictionary(x => x.Id);

        var lines = new List<FaRevaluationDocLine>();
        foreach (var lineDto in dto.Lines)
        {
            if (!assetById.TryGetValue(lineDto.FaAssetId, out var asset))
                return Result.Failure<List<FaRevaluationDocLine>>(FaRevaluationErrors.AssetNotFound(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StateId != StateIdConst.ACTIVE || asset.StatusId != FaAssetStatusIdConst.ACTIVE)
                return Result.Failure<List<FaRevaluationDocLine>>(FaRevaluationErrors.AssetInactive(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StatusId == FaAssetStatusIdConst.DISPOSED)
                return Result.Failure<List<FaRevaluationDocLine>>(FaRevaluationErrors.AssetDisposed(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.FaAssetAccounting is null)
                return Result.Failure<List<FaRevaluationDocLine>>(FaRevaluationErrors.AssetInactive(lineDto.FaAssetId, _userContext.LanguageId));

            lines.Add(new FaRevaluationDocLine
            {
                FaAssetId = asset.Id,
                NewValue = lineDto.NewValue,
                Note = lineDto.Note?.Trim(),
                AssetAccountId = asset.FaAssetAccounting.AssetAccountId,
                AccumulatedDepreciationAccountId = asset.FaAssetAccounting.AccumulatedDepreciationAccountId
            });
        }

        return Result.Success(lines);
    }

    private async Task<FaRevaluationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaRevaluationDoc>().Where(x => x.Id == id).As<FaRevaluationDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaRevaluationDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaRevaluationDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines));
        return await _query.GetAsync(query, ct);
    }

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
}
