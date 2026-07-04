using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class AccountingDispatcher : IAccountingDispatcher
    {
        private readonly IUserContext _userContext;
        private readonly IPostingContextDispatcher _postingContextDispatcher;
        private readonly IPostingService _postingService;
        private readonly IAccountingPostingValidator _postingValidator;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
        private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

        public AccountingDispatcher(IUserContext userContext,
                                    IPostingContextDispatcher postingContextDispatcher,
                                    IPostingService postingService,
                                    IAccountingPostingValidator postingValidator,
                                    IQueryBuilder queryBuilder,
                                    IQueryRepository<ChartAccount> chartAccountQuery,
                                    ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand)
        {
            _userContext = userContext;
            _postingContextDispatcher = postingContextDispatcher;
            _postingService = postingService;
            _postingValidator = postingValidator;
            _queryBuilder = queryBuilder;
            _chartAccountQuery = chartAccountQuery;
            _accountingRegisterCommand = accountingRegisterCommand;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
        {
            var contextsResult = await _postingContextDispatcher.ProcessAsync(document, ct);
            if (!contextsResult.IsSuccess)
            {
                if (contextsResult.Error.Code == PostingContextErrors.UnsupportedDocumentType().Code)
                    return Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.UnsupportedDocumentType(_userContext.LanguageId));

                return Result.Failure<List<AccountingRegisterEntry>>(contextsResult.Error);
            }

            try
            {
                var accountingEntries = await _postingService.BuildEntriesAsync(contextsResult.Value);
                if (postingBatchId.HasValue)
                {
                    foreach (var entry in accountingEntries)
                        entry.PostingBatchId = postingBatchId.Value;
                }

                var validation = _postingValidator.Validate(accountingEntries);
                if (!validation.IsSuccess)
                    return Result.Failure<List<AccountingRegisterEntry>>(validation.Error);

                var groupAccountValidation = await EnsureNoGroupAccountsAsync(accountingEntries, ct);
                if (!groupAccountValidation.IsSuccess)
                    return Result.Failure<List<AccountingRegisterEntry>>(groupAccountValidation.Error);

                if (accountingEntries.Count > 0)
                    await _accountingRegisterCommand.CreateAsync(accountingEntries, ct);

                return Result.Success(accountingEntries);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<AccountingRegisterEntry>>(
                    SharedKernel.Results.Error.Problem(
                        "AccountingRegisterEntry.PostingFailed",
                        ex.Message));
            }
        }

        // Group (header) accounts aggregate their children and must never receive a direct
        // posting — only leaf accounts are postable, matching professional ERP ledgers.
        private async Task<Result> EnsureNoGroupAccountsAsync(List<AccountingRegisterEntry> entries, CancellationToken ct)
        {
            var accountIds = entries
                .SelectMany(e => new[] { e.DebitAccountId, e.CreditAccountId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (accountIds.Count == 0)
                return Result.Success();

            var query = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && x.IsGroup)
                .As(x => x.Id)
                .Build();

            var groupAccountIds = await _chartAccountQuery.GetAllAsync(query, ct);

            return groupAccountIds.Count == 0
                ? Result.Success()
                : Result.Failure(AccountingRegisterEntryErrors.GroupAccountNotPostable(groupAccountIds, _userContext.LanguageId));
        }
    }
}
