using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Register.AccountingRegisterEntries;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public sealed class AccountingRegisterEntryRebuildService : BaseService, IAccountingRegisterEntryRebuildService
{
    private readonly IUserContext _userContext;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingRegisterEntryRebuildRepository _repository;
    private readonly IAccountingDispatcher _dispatcher;

    public AccountingRegisterEntryRebuildService(
        IUserContext userContext,
        IDocumentPostingLock postingLock,
        IAccountingRegisterEntryRebuildRepository repository,
        IAccountingDispatcher dispatcher,
        ILogger<AccountingRegisterEntryRebuildService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _postingLock = postingLock;
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public Task<Result<AccountingRegisterEntryRebuildDto>> RebuildAsync(
        short documentTypeId,
        long documentId,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(RebuildAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<AccountingRegisterEntryRebuildDto>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (!_repository.Supports(documentTypeId))
                return Result.Failure<AccountingRegisterEntryRebuildDto>(
                    AccountingRegisterEntryErrors.UnsupportedRebuildDocumentType(
                        documentTypeId,
                        _userContext.LanguageId));

            var acquired = await _postingLock.TryAcquireAsync(documentTypeId, documentId, ct);
            if (!acquired)
                return Result.Failure<AccountingRegisterEntryRebuildDto>(
                    AccountingRegisterEntryErrors.AlreadyRebuilding(
                        documentTypeId,
                        documentId,
                        _userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var source = await _repository.GetSourceAsync(
                documentTypeId,
                documentId,
                organizationId,
                ct);
            if (source is null)
                return Result.Failure<AccountingRegisterEntryRebuildDto>(
                    AccountingRegisterEntryErrors.PostedDocumentNotFound(
                        documentTypeId,
                        documentId,
                        _userContext.LanguageId));

            var deletedCount = await _repository.DeleteEntriesAsync(
                documentTypeId,
                documentId,
                organizationId,
                ct);

            var dispatchResult = await _dispatcher.ProcessAsync(
                source.Document,
                ct,
                source.PostingBatchId);
            if (!dispatchResult.IsSuccess)
                return Result.Failure<AccountingRegisterEntryRebuildDto>(dispatchResult.Error);

            if (dispatchResult.Value.Count == 0)
                return Result.Failure<AccountingRegisterEntryRebuildDto>(
                    AccountingRegisterEntryErrors.RebuildProducedNoEntries(
                        documentTypeId,
                        documentId,
                        _userContext.LanguageId));

            return Result.Success(new AccountingRegisterEntryRebuildDto
            {
                DocumentTypeId = documentTypeId,
                DocumentId = documentId,
                DeletedCount = deletedCount,
                CreatedCount = dispatchResult.Value.Count
            });
        }, ct);
}
