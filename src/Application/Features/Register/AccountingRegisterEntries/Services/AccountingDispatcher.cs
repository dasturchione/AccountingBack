using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class AccountingDispatcher : IAccountingDispatcher
    {
        private readonly IUserContext _userContext;
        private readonly IPostingContextDispatcher _postingContextDispatcher;
        private readonly IPostingService _postingService;
        private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

        public AccountingDispatcher(IUserContext userContext,
                                    IPostingContextDispatcher postingContextDispatcher,
                                    IPostingService postingService,
                                    ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand)
        {
            _userContext = userContext;
            _postingContextDispatcher = postingContextDispatcher;
            _postingService = postingService;
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
    }
}
