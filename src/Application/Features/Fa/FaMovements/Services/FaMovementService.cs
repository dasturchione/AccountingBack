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

namespace Application.Features.FaMovements;

public class FaMovementService : BaseService, IFaMovementService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IFaMovementLifecycleService _lifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<FaMovementDoc> _query;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IFaMovementCommandRepository _command;

    public FaMovementService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IFaMovementLifecycleService lifecycleService,
        IDocumentNumberService documentNumberService,
        IQueryRepository<FaMovementDoc> query,
        IQueryRepository<FaAsset> faAssetQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<User> userQuery,
        IFaMovementCommandRepository command,
        ILogger<FaMovementService> logger)
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
        _departmentQuery = departmentQuery;
        _userQuery = userQuery;
        _command = command;
    }

    public Task<Result<PagedResponse<FaMovementListDto>>> GetAllAsync(FaMovementListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaMovementDoc, FaMovementListDto, FaMovementListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaMovementDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var entity = await GetByIdInternalAsync(id, ct);
            return entity is null
                ? Result.Failure<FaMovementDto>(FaMovementErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(FaMovementCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var draftDataResult = await BuildDraftDataAsync(organizationId, dto, ct);
            if (!draftDataResult.IsSuccess)
                return Result.Failure<long>(draftDataResult.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.FAMOVEMENT,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var draftData = draftDataResult.Value;
            var doc = new FaMovementDoc
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = NormalizeDateTime(dto.DocDate),
                StatusId = DocumentStatusIdConst.DRAFT,
                FromDepartmentId = draftData.FromDepartmentId,
                ToDepartmentId = dto.ToDepartmentId,
                FromResponsibleUserId = draftData.FromResponsibleUserId,
                ToResponsibleUserId = dto.ToResponsibleUserId,
                Note = dto.Note?.Trim(),
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                UpdatedDate = now,
                UpdatedByUserId = _userContext.Id,
                Lines = draftData.Lines
            };

            await _command.CreateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto is not null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaMovementDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, FaMovementUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaMovementErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(FaMovementErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var draftDataResult = await BuildDraftDataAsync(_userContext.OrganizationId.Value, dto, ct);
            if (!draftDataResult.IsSuccess)
                return Result.Failure(draftDataResult.Error);

            var draftData = draftDataResult.Value;
            if (doc.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(doc.Lines.ToList(), ct);
                doc.Lines.Clear();
            }

            foreach (var line in draftData.Lines)
                doc.Lines.Add(line);

            doc.DocDate = NormalizeDateTime(dto.DocDate);
            doc.FromDepartmentId = draftData.FromDepartmentId;
            doc.ToDepartmentId = dto.ToDepartmentId;
            doc.FromResponsibleUserId = draftData.FromResponsibleUserId;
            doc.ToResponsibleUserId = dto.ToResponsibleUserId;
            doc.Note = dto.Note?.Trim();
            doc.StateId = dto.StateId;
            doc.UpdatedDate = DateTime.Now;
            doc.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaMovementDoc, id.ToString(), AuditLogOperationTypeConst.Update);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    private async Task<FaMovementDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaMovementDoc>().Where(x => x.Id == id).As<FaMovementDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaMovementDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaMovementDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines));
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result<FaMovementDraftData>> BuildDraftDataAsync(int organizationId, FaMovementBaseDto dto, CancellationToken ct)
    {
        if (dto.Lines.Count == 0)
            return Result.Failure<FaMovementDraftData>(FaMovementErrors.LinesRequired(_userContext.LanguageId));

        if (!dto.ToDepartmentId.HasValue && !dto.ToResponsibleUserId.HasValue)
            return Result.Failure<FaMovementDraftData>(FaMovementErrors.NoTargetChange(_userContext.LanguageId));

        if (dto.ToDepartmentId.HasValue &&
            !await _departmentQuery.AnyAsync(x => x.Id == dto.ToDepartmentId.Value &&
                                                  x.OrganizationId == organizationId &&
                                                  x.StateId == StateIdConst.ACTIVE, ct))
        {
            return Result.Failure<FaMovementDraftData>(FaMovementErrors.DepartmentNotFound(dto.ToDepartmentId.Value, _userContext.LanguageId));
        }

        if (dto.ToResponsibleUserId.HasValue &&
            !await _userQuery.AnyAsync(x => x.Id == dto.ToResponsibleUserId.Value &&
                                            x.StateId == StateIdConst.ACTIVE, ct))
        {
            return Result.Failure<FaMovementDraftData>(FaMovementErrors.ResponsibleUserNotFound(dto.ToResponsibleUserId.Value, _userContext.LanguageId));
        }

        var assetIds = dto.Lines.Select(x => x.FaAssetId).Distinct().ToList();
        if (assetIds.Count != dto.Lines.Count)
        {
            var duplicateId = dto.Lines.GroupBy(x => x.FaAssetId).First(x => x.Count() > 1).Key;
            return Result.Failure<FaMovementDraftData>(FaMovementErrors.DuplicateAsset(duplicateId, _userContext.LanguageId));
        }

        var assetQuery = _queryBuilder.For<FaAsset>()
            .Where(x => assetIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .Build();
        var assets = await _faAssetQuery.GetAllAsync(assetQuery, ct);
        var assetById = assets.ToDictionary(x => x.Id);

        var lines = new List<FaMovementDocLine>();
        int? fromDepartmentId = null;
        int? fromResponsibleUserId = null;

        foreach (var lineDto in dto.Lines)
        {
            if (!assetById.TryGetValue(lineDto.FaAssetId, out var asset))
                return Result.Failure<FaMovementDraftData>(FaMovementErrors.AssetNotFound(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StateId != StateIdConst.ACTIVE)
                return Result.Failure<FaMovementDraftData>(FaMovementErrors.AssetInactive(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StatusId != FaAssetStatusIdConst.ACTIVE)
                return Result.Failure<FaMovementDraftData>(FaMovementErrors.AssetDisposed(lineDto.FaAssetId, _userContext.LanguageId));

            fromDepartmentId ??= asset.DepartmentId;
            fromResponsibleUserId ??= asset.ResponsibleUserId;

            if (asset.DepartmentId != fromDepartmentId || asset.ResponsibleUserId != fromResponsibleUserId)
                return Result.Failure<FaMovementDraftData>(FaMovementErrors.MixedSourceOwnership(_userContext.LanguageId));

            lines.Add(new FaMovementDocLine
            {
                FaAssetId = asset.Id,
                Note = lineDto.Note?.Trim()
            });
        }

        if (fromDepartmentId == dto.ToDepartmentId && fromResponsibleUserId == dto.ToResponsibleUserId)
            return Result.Failure<FaMovementDraftData>(FaMovementErrors.NoTargetChange(_userContext.LanguageId));

        return Result.Success(new FaMovementDraftData
        {
            FromDepartmentId = fromDepartmentId,
            FromResponsibleUserId = fromResponsibleUserId,
            Lines = lines
        });
    }

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

    private sealed class FaMovementDraftData
    {
        public int? FromDepartmentId { get; init; }
        public int? FromResponsibleUserId { get; init; }
        public List<FaMovementDocLine> Lines { get; init; } = new();
    }
}
