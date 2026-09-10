using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;
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
                if (contextsResult.Error.Code == PostingContextErrors.UnsupportedDocumentType(_userContext.LanguageId).Code)
                    return Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.UnsupportedDocumentType(_userContext.LanguageId));

                return Result.Failure<List<AccountingRegisterEntry>>(contextsResult.Error);
            }

            var entriesResult = await _postingService.BuildEntriesAsync(contextsResult.Value);
            if (!entriesResult.IsSuccess)
                return Result.Failure<List<AccountingRegisterEntry>>(entriesResult.Error);

            var accountingEntries = entriesResult.Value;
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

        // Group (header) accounts aggregate their children and must never receive a direct
        // posting — only active leaf accounts are postable, matching professional ERP ledgers.
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

            var organizationIds = entries
                .Select(entry => entry.OrganizationId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var query = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) &&
                            organizationIds.Contains(x.OrganizationId) &&
                            (x.IsGroup || x.StateId != StateIdConst.ACTIVE))
                .As(x => x.Id)
                .Build();

            var invalidAccountIds = await _chartAccountQuery.GetAllAsync(query, ct);

            var existingQuery = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && organizationIds.Contains(x.OrganizationId))
                .As(x => x.Id)
                .Build();
            var existingAccountIds = await _chartAccountQuery.GetAllAsync(existingQuery, ct);
            invalidAccountIds = invalidAccountIds
                .Concat(accountIds.Except(existingAccountIds))
                .Distinct()
                .ToList();

            return invalidAccountIds.Count == 0
                ? Result.Success()
                : Result.Failure(AccountingRegisterEntryErrors.AccountNotPostable(invalidAccountIds, _userContext.LanguageId));
        }
    }
}
