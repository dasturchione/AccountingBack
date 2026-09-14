using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationService : BaseService, ICurrencyRevaluationService
{
    private readonly ILogger<CurrencyRevaluationService> _logger;
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CurrencyRevaluation> _query;
    private readonly ICommandRepository<CurrencyRevaluation> _command;
    private readonly IQueryRepository<CurrencyRate> _rateQuery;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyQuery;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
    private readonly ICriteriaBuilder<CurrencyRevaluationListDto, CurrencyRevaluationListFilter> _listCriteriaBuilder;

    public CurrencyRevaluationService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<CurrencyRevaluation> query,
        ICommandRepository<CurrencyRevaluation> command,
        IQueryRepository<CurrencyRate> rateQuery,
        IQueryRepository<MoneyRegisterBalance> moneyQuery,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAccountingDispatcher dispatcher,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        ICriteriaBuilder<CurrencyRevaluationListDto, CurrencyRevaluationListFilter> listCriteriaBuilder,
        ILogger<CurrencyRevaluationService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _logger = logger;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
        _rateQuery = rateQuery;
        _moneyQuery = moneyQuery;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _dispatcher = dispatcher;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
        _listCriteriaBuilder = listCriteriaBuilder;
    }

    public async Task<Result<PagedResponse<CurrencyRevaluationListDto>>> GetAllAsync(CurrencyRevaluationListFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<PagedResponse<CurrencyRevaluationListDto>>(CurrencyRevaluationErrors.NoOrganization(_userContext.LanguageId ?? 0));

        var organizationId = _userContext.OrganizationId.Value;
        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
        var query = _queryBuilder.For<CurrencyRevaluation>()
            .Where(x => x.OrganizationId == organizationId)
            .As<CurrencyRevaluationListDto>()
            .Where(_listCriteriaBuilder.Build(filter))
            .OrderBy(items => items
                .OrderByDescending(x => x.RevaluationDate)
                .ThenByDescending(x => x.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .BuildPaged();
        var pagedList = await _query.GetPagedAsync(query, ct);
        return Result.Success(PagedResponseFactory.Create(pagedList, page, pageSize));
    }

    public async Task<Result<CurrencyRevaluationDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<CurrencyRevaluationDto>(CurrencyRevaluationErrors.NoOrganization(_userContext.LanguageId ?? 0));

        var q = _queryBuilder.For<CurrencyRevaluation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<CurrencyRevaluationDto>()
            .Build();
        var item = await _query.GetAsync(q, ct);
        return item is null
            ? Result.Failure<CurrencyRevaluationDto>(CurrencyRevaluationErrors.NotFound(id, _userContext.LanguageId ?? 0))
            : Result.Success(item);
    }

    public Task<Result<CurrencyRevaluationDto>> PreviewAsync(CurrencyRevaluationPreviewDto dto, CancellationToken ct = default) =>
        BuildPreviewAsync(dto, false, ct);

    public Task<Result<CurrencyRevaluationDto>> CreateAsync(CurrencyRevaluationCreateDto dto, CancellationToken ct = default) =>
        BuildPreviewAsync(dto, true, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            _logger.LogInformation("Currency revaluation confirm started. DocumentId={DocumentId}, OrganizationId={OrganizationId}", id, _userContext.OrganizationId);

            if (_userContext.OrganizationId is null)
                return Result.Failure(CurrencyRevaluationErrors.NoOrganization(_userContext.LanguageId ?? 0));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CURRENCYREVALUATION, id, ct);

            var entity = await GetForLifecycleAsync(id, ct);
            if (entity is null)
                return Result.Failure(CurrencyRevaluationErrors.NotFound(id, _userContext.LanguageId ?? 0));

            if (entity.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(CurrencyRevaluationErrors.NotFound(id, _userContext.LanguageId ?? 0));

            if (entity.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(CurrencyRevaluationErrors.AlreadyCancelled(id, _userContext.LanguageId ?? 0));

            if (entity.StatusId == DocumentStatusIdConst.POSTED)
            {
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(CurrencyRevaluationErrors.MissingPostingBatch(id, _userContext.LanguageId ?? 0));
            }

            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CurrencyRevaluationErrors.CannotConfirmInCurrentStatus(id, entity.StatusId, _userContext.LanguageId ?? 0));

            var periodValidation = await _periodValidator.EnsureOpenAsync(entity.OrganizationId, entity.RevaluationDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var validation = await ValidateForConfirmAsync(entity, ct);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(CurrencyRevaluationErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId ?? 0));

            var postingBatch = await CreatePostingBatchAsync(entity, PostingBatchStatusConst.POSTED, "Currency revaluation confirmed", ct);
            var postingResult = await _dispatcher.ProcessAsync(entity, ct, postingBatch.Id);
            if (!postingResult.IsSuccess)
                return Result.Failure(postingResult.Error);

            entity.StatusId = DocumentStatusIdConst.POSTED;
            entity.ConfirmedAt ??= DateTime.Now;
            entity.PostedByUserId ??= _userContext.Id;
            await _command.UpdateAsync(entity, ct);

            _logger.LogInformation("Currency revaluation confirm completed. DocumentId={DocumentId}", id);

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            _logger.LogInformation("Currency revaluation cancel started. DocumentId={DocumentId}, OrganizationId={OrganizationId}", id, _userContext.OrganizationId);

            if (_userContext.OrganizationId is null)
                return Result.Failure(CurrencyRevaluationErrors.NoOrganization(_userContext.LanguageId ?? 0));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CURRENCYREVALUATION, id, ct);

            var entity = await GetForLifecycleAsync(id, ct);
            if (entity is null)
                return Result.Failure(CurrencyRevaluationErrors.NotFound(id, _userContext.LanguageId ?? 0));

            if (entity.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(CurrencyRevaluationErrors.NotFound(id, _userContext.LanguageId ?? 0));

            if (entity.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (entity.StatusId != DocumentStatusIdConst.DRAFT && entity.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(CurrencyRevaluationErrors.CannotCancelInCurrentStatus(id, entity.StatusId, _userContext.LanguageId ?? 0));

            var periodValidation = await _periodValidator.EnsureOpenAsync(entity.OrganizationId, entity.RevaluationDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(entity.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            if (entity.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(CurrencyRevaluationErrors.MissingPostingBatch(id, _userContext.LanguageId ?? 0));

                var validateCancel = await ValidateForCancelAsync(entity, ct);
                if (!validateCancel.IsSuccess)
                    return validateCancel;

                var reversalBatch = await CreatePostingBatchAsync(entity, PostingBatchStatusConst.REVERSAL, "Currency revaluation cancelled", ct);
                var reverseResult = await ReverseAccountingEntriesAsync(entity, reversalBatch.Id, ct);
                if (!reverseResult.IsSuccess)
                    return reverseResult;

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = DateTime.Now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);
            }

            entity.StatusId = DocumentStatusIdConst.CANCELLED;
            entity.CancelledAt ??= DateTime.Now;
            entity.CancelledByUserId ??= _userContext.Id;
            await _command.UpdateAsync(entity, ct);

            _logger.LogInformation("Currency revaluation cancel completed. DocumentId={DocumentId}", id);

            return Result.Success();
        }, ct);

    private async Task<Result<CurrencyRevaluationDto>> BuildPreviewAsync(CurrencyRevaluationBaseDto dto, bool persist, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<CurrencyRevaluationDto>(CurrencyRevaluationErrors.NoOrganization(_userContext.LanguageId ?? 0));

        if (dto.RevaluationDate.Date > DateTime.Today)
            return Result.Failure<CurrencyRevaluationDto>(CurrencyRevaluationErrors.InvalidDate(_userContext.LanguageId ?? 0));

        var lines = await BuildLinesAsync(_userContext.OrganizationId.Value, dto, ct);
        if (persist && lines.Count == 0)
            return Result.Failure<CurrencyRevaluationDto>(CurrencyRevaluationErrors.NoRevaluationLines(_userContext.LanguageId ?? 0));

        var model = new CurrencyRevaluationDto
        {
            OrganizationId = _userContext.OrganizationId.Value,
            RevaluationDate = dto.RevaluationDate,
            ProviderRateDate = dto.ProviderRateDate,
            StatusId = persist ? DocumentStatusIdConst.DRAFT : DocumentStatusIdConst.PENDING,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now,
            Lines = lines
        };

        if (!persist)
            return Result.Success(model);

        var entity = new CurrencyRevaluation
        {
            OrganizationId = model.OrganizationId,
            RevaluationDate = model.RevaluationDate,
            ProviderRateDate = model.ProviderRateDate,
            StatusId = model.StatusId,
            StateId = model.StateId,
            CreatedDate = model.CreatedDate,
            Lines = model.Lines.Select(x => new CurrencyRevaluationLine
            {
                BaseCurrencyId = x.BaseCurrencyId,
                TargetCurrencyId = x.TargetCurrencyId,
                BalanceAmount = x.BalanceAmount,
                OpeningRate = x.OpeningRate,
                CurrentRate = x.CurrentRate,
                DifferenceAmount = x.DifferenceAmount,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            }).ToList()
        };

        await _command.CreateAsync(entity, ct);
        model.Id = entity.Id;
        return Result.Success(model);
    }

    private async Task<List<CurrencyRevaluationLineDto>> BuildLinesAsync(int organizationId, CurrencyRevaluationBaseDto dto, CancellationToken ct)
    {
        var balancesQuery = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.CurrencyId != CurrencyIdConst.UZS &&
                        x.DocDate <= dto.RevaluationDate)
            .Build();
        balancesQuery.AddIncludes(b => b.Include(x => x.Currency));

        var balances = await _moneyQuery.GetAllAsync(balancesQuery, ct);

        var result = new List<CurrencyRevaluationLineDto>();
        var currencyGroups = balances
            .Where(x => !dto.TargetCurrencyId.HasValue || x.CurrencyId == dto.TargetCurrencyId.Value)
            .GroupBy(x => x.CurrencyId);

        foreach (var currencyGroup in currencyGroups)
        {
            var currentRate = await GetRateAsync(currencyGroup.Key, dto.ProviderRateDate ?? dto.RevaluationDate, ct);
            if (currentRate <= 0)
                continue;

            var balanceAmount = 0m;
            var carryingAmount = 0m;
            foreach (var balance in currencyGroup)
            {
                var openingRate = await GetRateAsync(balance.CurrencyId, balance.DocDate, ct);
                if (openingRate <= 0)
                    continue;

                var signedAmount = balance.DirectionId * balance.Amount;
                balanceAmount += signedAmount;
                carryingAmount += signedAmount * openingRate;
            }

            if (balanceAmount == 0m)
                continue;

            var weightedOpeningRate = carryingAmount / balanceAmount;
            var diff = Math.Round(balanceAmount * currentRate - carryingAmount, 2);
            if (diff == 0)
                continue;

            result.Add(new CurrencyRevaluationLineDto
            {
                BaseCurrencyId = CurrencyIdConst.UZS,
                TargetCurrencyId = currencyGroup.Key,
                TargetCurrencyCode = currencyGroup.First().Currency.Code,
                BalanceAmount = balanceAmount,
                OpeningRate = weightedOpeningRate,
                CurrentRate = currentRate,
                DifferenceAmount = diff
            });
        }

        return result;
    }

    private async Task<decimal> GetRateAsync(short targetCurrencyId, DateTime date, CancellationToken ct)
    {
        var query = _queryBuilder.For<CurrencyRate>()
            .Where(x => x.BaseCurrencyId == CurrencyIdConst.UZS && x.TargetCurrencyId == targetCurrencyId && x.EffectiveDate <= date)
            .OrderBy(x => x.EffectiveDate).Desc()
            .As(x => x.OfficialRate)
            .Build();

        var rates = await _rateQuery.GetAllAsync(query, ct);
        return rates.FirstOrDefault();
    }

    private async Task<CurrencyRevaluation?> GetForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<CurrencyRevaluation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.Lines));
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateForConfirmAsync(CurrencyRevaluation entity, CancellationToken ct)
    {
        if (entity.Lines.Count == 0)
                return Result.Failure(CurrencyRevaluationErrors.NoRevaluationLines(_userContext.LanguageId ?? 0));

        var lineIds = entity.Lines.Select(x => x.TargetCurrencyId).Distinct().ToList();
        var openBalancesQuery = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.OrganizationId == entity.OrganizationId &&
                        lineIds.Contains(x.CurrencyId) &&
                        x.DocDate <= entity.RevaluationDate)
            .Build();
        var openBalances = await _moneyQuery.GetAllAsync(openBalancesQuery, ct);

        if (!openBalances
                .GroupBy(x => x.CurrencyId)
                .Any(group => group.Sum(x => x.DirectionId * x.Amount) != 0m))
            return Result.Failure(CurrencyRevaluationErrors.NoRevaluationLines(_userContext.LanguageId ?? 0));

        return Result.Success();
    }

    private async Task<Result> ValidateForCancelAsync(CurrencyRevaluation entity, CancellationToken ct)
    {
        var hasPostedEntries = await _accountingRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.CURRENCYREVALUATION &&
            x.DocumentId == entity.Id &&
            x.ReversalEntryId == null, ct);

        return hasPostedEntries
            ? Result.Success()
            : Result.Failure(CurrencyRevaluationErrors.MissingAccountingRegisterEntries(entity.Id, _userContext.LanguageId ?? 0));
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CURRENCYREVALUATION &&
                        x.DocumentId == id &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long id, CancellationToken ct) =>
        await _accountingRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.CURRENCYREVALUATION &&
            x.DocumentId == id &&
            x.ReversalEntryId == null, ct);

    private async Task<PostingBatch> CreatePostingBatchAsync(CurrencyRevaluation entity, string status, string comment, CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = entity.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CURRENCYREVALUATION,
            DocumentId = entity.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = DateTime.Now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<Result> ReverseAccountingEntriesAsync(CurrencyRevaluation entity, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CURRENCYREVALUATION &&
                        x.DocumentId == entity.Id &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(b => b.Include(x => x.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Failure(CurrencyRevaluationErrors.MissingAccountingRegisterEntries(entity.Id, _userContext.LanguageId ?? 0));

        var reversals = AccountingRegisterEntryReversalFactory.Create(
            entries,
            reversalBatchId);

        await _accountingRegisterCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }
}
