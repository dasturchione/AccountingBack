using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaDisposals;

public class FaDisposalService : BaseService, IFaDisposalService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IFaDisposalLifecycleService _lifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<FaDisposalDoc> _query;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IFaDisposalCommandRepository _command;

    public FaDisposalService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IFaDisposalLifecycleService lifecycleService,
        IDocumentNumberService documentNumberService,
        IQueryRepository<FaDisposalDoc> query,
        IQueryRepository<FaAsset> faAssetQuery,
        IFaDisposalCommandRepository command,
        ILogger<FaDisposalService> logger)
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

    public Task<Result<PagedResponse<FaDisposalListDto>>> GetAllAsync(FaDisposalListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaDisposalDoc, FaDisposalListDto, FaDisposalListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaDisposalDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var item = await GetByIdInternalAsync(id, ct);
            return item is null
                ? Result.Failure<FaDisposalDto>(FaDisposalErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(item);
        });

    public Task<Result<long>> CreateAsync(FaDisposalCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var linesResult = await BuildLinesAsync(_userContext.OrganizationId.Value, dto, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                _userContext.OrganizationId.Value,
                DocumentTypeIdConst.FADISPOSAL,
                dto.DisposalDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var doc = new FaDisposalDoc
            {
                OrganizationId = _userContext.OrganizationId.Value,
                StateId = StateIdConst.ACTIVE,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DisposalDate = NormalizeDateTime(dto.DisposalDate),
                DisposalTypeId = dto.DisposalTypeId,
                Reason = dto.Reason?.Trim(),
                DisposalAccountId = dto.DisposalAccountId,
                CustomerAccountId = dto.CustomerAccountId,
                VatAccountId = dto.VatAccountId,
                GainAccountId = dto.GainAccountId,
                LossAccountId = dto.LossAccountId,
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
                await _auditLogService.CreateAsync(AuditLogTableConst.FaDisposalDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, FaDisposalUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaDisposalErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(FaDisposalErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

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

            doc.DisposalDate = NormalizeDateTime(dto.DisposalDate);
            doc.DisposalTypeId = dto.DisposalTypeId;
            doc.Reason = dto.Reason?.Trim();
            doc.DisposalAccountId = dto.DisposalAccountId;
            doc.CustomerAccountId = dto.CustomerAccountId;
            doc.VatAccountId = dto.VatAccountId;
            doc.GainAccountId = dto.GainAccountId;
            doc.LossAccountId = dto.LossAccountId;
            doc.StateId = dto.StateId;
            doc.UpdatedDate = DateTime.Now;
            doc.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaDisposalDoc, id.ToString(), AuditLogOperationTypeConst.Update);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => _lifecycleService.CancelAsync(id, ct);

    private async Task<Result<List<FaDisposalDocLine>>> BuildLinesAsync(int organizationId, FaDisposalBaseDto dto, CancellationToken ct)
    {
        if (dto.Lines.Count == 0)
            return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.LinesRequired(_userContext.LanguageId));
        
        var assetIds = dto.Lines.Select(x => x.FaAssetId).Distinct().ToList();

        if (assetIds.Count != dto.Lines.Count)
        {
            var duplicateId = dto.Lines.GroupBy(x => x.FaAssetId).First(x => x.Count() > 1).Key;
            return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.DuplicateAsset(duplicateId, _userContext.LanguageId));
        }

        var assetQuery = _queryBuilder.For<FaAsset>()
            .Where(x => assetIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .Build();
        assetQuery.AddIncludes(x => x.Include(a => a.Status));
        var assets = await _faAssetQuery.GetAllAsync(assetQuery, ct);
        var assetById = assets.ToDictionary(x => x.Id);

        if (dto.DisposalTypeId == FaDisposalTypeIdConst.SALE && dto.Lines.All(x => x.SaleAmount <= 0))
            return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.SaleAmountRequired(_userContext.LanguageId));

        var lines = new List<FaDisposalDocLine>();
        foreach (var lineDto in dto.Lines)
        {
            if (!assetById.TryGetValue(lineDto.FaAssetId, out var asset))
                return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.AssetNotFound(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StateId != StateIdConst.ACTIVE)
                return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.AssetInactive(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StatusId == FaAssetStatusIdConst.DISPOSED)
                return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.AssetAlreadyDisposed(lineDto.FaAssetId, _userContext.LanguageId));

            if (asset.StatusId != FaAssetStatusIdConst.ACTIVE)
                return Result.Failure<List<FaDisposalDocLine>>(FaDisposalErrors.AssetInactive(lineDto.FaAssetId, _userContext.LanguageId));

            lines.Add(new FaDisposalDocLine
            {
                FaAssetId = asset.Id,
                SaleAmount = lineDto.SaleAmount,
                Note = lineDto.Note?.Trim(),
                AssetAccountId = lineDto.AssetAccountId,
                AccumulatedDepreciationAccountId = lineDto.AccumulatedDepreciationAccountId
            });
        }

        return Result.Success(lines);
    }

    private async Task<FaDisposalDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDisposalDoc>().Where(x => x.Id == id).As<FaDisposalDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaDisposalDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDisposalDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines));
        return await _query.GetAsync(query, ct);
    }

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
}
