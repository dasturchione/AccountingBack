using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.FaAssets;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaMovements;

public class FaMovementLifecycleService : BaseService, IFaMovementLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<FaMovementDoc> _query;
    private readonly IFaMovementCommandRepository _command;
    private readonly IFaAssetCommandRepository _faAssetCommand;

    public FaMovementLifecycleService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IQueryRepository<FaMovementDoc> query,
        IFaMovementCommandRepository command,
        IFaAssetCommandRepository faAssetCommand,
        ILogger<FaMovementLifecycleService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _faAssetCommand = faAssetCommand;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FAMOVEMENT, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaMovementErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(FaMovementErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(FaMovementErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var now = DateTime.Now;
            foreach (var asset in doc.Lines.Select(x => x.FaAsset).DistinctBy(x => x.Id))
            {
                asset.DepartmentId = doc.ToDepartmentId;
                asset.ResponsibleUserId = doc.ToResponsibleUserId;
                asset.UpdatedDate = now;
                await _faAssetCommand.UpdateAsync(asset, ct);
            }

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= now;
            doc.PostedByUserId ??= _userContext.Id;
            doc.UpdatedDate = now;
            doc.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaMovementDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FAMOVEMENT, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaMovementErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
            {
                return Result.Failure(FaMovementErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var now = DateTime.Now;
            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var validation = ValidateForCancel(doc);
                if (!validation.IsSuccess)
                    return validation;

                foreach (var asset in doc.Lines.Select(x => x.FaAsset).DistinctBy(x => x.Id))
                {
                    asset.DepartmentId = doc.FromDepartmentId;
                    asset.ResponsibleUserId = doc.FromResponsibleUserId;
                    asset.UpdatedDate = now;
                    await _faAssetCommand.UpdateAsync(asset, ct);
                }
            }

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt ??= now;
            doc.CancelledByUserId ??= _userContext.Id;
            doc.UpdatedDate = now;
            doc.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaMovementDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private Result ValidateForConfirm(FaMovementDoc doc)
    {
        if (doc.Lines.Count == 0)
            return Result.Failure(FaMovementErrors.LinesRequired(_userContext.LanguageId));

        if (doc.FromDepartmentId == doc.ToDepartmentId && doc.FromResponsibleUserId == doc.ToResponsibleUserId)
            return Result.Failure(FaMovementErrors.NoTargetChange(_userContext.LanguageId));

        foreach (var asset in doc.Lines.Select(x => x.FaAsset))
        {
            if (asset.StateId != StateIdConst.ACTIVE)
                return Result.Failure(FaMovementErrors.AssetInactive(asset.Id, _userContext.LanguageId));

            if (asset.StatusId != FaAssetStatusIdConst.ACTIVE)
                return Result.Failure(FaMovementErrors.AssetDisposed(asset.Id, _userContext.LanguageId));

            if (asset.DepartmentId != doc.FromDepartmentId || asset.ResponsibleUserId != doc.FromResponsibleUserId)
                return Result.Failure(FaMovementErrors.MixedSourceOwnership(_userContext.LanguageId));
        }

        return Result.Success();
    }

    private Result ValidateForCancel(FaMovementDoc doc)
    {
        foreach (var asset in doc.Lines.Select(x => x.FaAsset))
        {
            if (asset.StateId != StateIdConst.ACTIVE)
                return Result.Failure(FaMovementErrors.AssetInactive(asset.Id, _userContext.LanguageId));

            if (asset.StatusId != FaAssetStatusIdConst.ACTIVE)
                return Result.Failure(FaMovementErrors.AssetDisposed(asset.Id, _userContext.LanguageId));

            if (asset.DepartmentId != doc.ToDepartmentId || asset.ResponsibleUserId != doc.ToResponsibleUserId)
                return Result.Failure(FaMovementErrors.NoTargetChange(_userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<FaMovementDoc?> GetForLifecycleAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaMovementDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.FaAsset).ThenInclude(a => a.Department));
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.FaAsset).ThenInclude(a => a.ResponsibleUser));
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.FaAsset).ThenInclude(a => a.Status));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaMovementDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaMovementDoc>().Where(x => x.Id == id).As<FaMovementDto>().Build();
        return await _query.GetAsync(query, ct);
    }
}
