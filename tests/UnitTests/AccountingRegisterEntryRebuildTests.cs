using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace UnitTests;

public sealed class AccountingRegisterEntryRebuildTests
{
    [Fact]
    public async Task RebuildAsync_DeletesExistingEntriesBeforeDispatchingCurrentDocument()
    {
        var document = new BankOperation
        {
            Id = 42,
            OrganizationId = 7,
            StatusId = DocumentStatusIdConst.POSTED
        };
        var repository = new InMemoryRebuildRepository(
            new AccountingRegisterEntryRebuildSource(document, 55),
            [
                new AccountingRegisterEntry { Id = 1, OrganizationId = 7, DocumentTypeId = DocumentTypeIdConst.BANKOPERATION, DocumentId = 42 },
                new AccountingRegisterEntry { Id = 2, OrganizationId = 7, DocumentTypeId = DocumentTypeIdConst.BANKOPERATION, DocumentId = 42 }
            ]);
        var dispatcher = new RebuildDispatcher(repository);
        var unitOfWork = new RecordingUnitOfWork();
        var service = new AccountingRegisterEntryRebuildService(
            new TestUserContext(7),
            new SuccessfulPostingLock(),
            repository,
            dispatcher,
            NullLogger<AccountingRegisterEntryRebuildService>.Instance,
            unitOfWork);

        var result = await service.RebuildAsync(DocumentTypeIdConst.BANKOPERATION, 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.DeletedCount);
        Assert.Equal(1, result.Value.CreatedCount);
        var rebuilt = Assert.Single(repository.Entries);
        Assert.Equal(55, rebuilt.PostingBatchId);
        Assert.Equal(42, rebuilt.DocumentId);
        Assert.True(unitOfWork.Committed);
        Assert.False(unitOfWork.RolledBack);
    }

    [Fact]
    public async Task RebuildAsync_RollsBackWhenDispatcherProducesNoEntries()
    {
        var document = new BankOperation
        {
            Id = 42,
            OrganizationId = 7,
            StatusId = DocumentStatusIdConst.POSTED
        };
        var repository = new InMemoryRebuildRepository(
            new AccountingRegisterEntryRebuildSource(document, 55),
            [
                new AccountingRegisterEntry { Id = 1, OrganizationId = 7, DocumentTypeId = DocumentTypeIdConst.BANKOPERATION, DocumentId = 42 }
            ]);
        var unitOfWork = new RecordingUnitOfWork();
        var service = new AccountingRegisterEntryRebuildService(
            new TestUserContext(7),
            new SuccessfulPostingLock(),
            repository,
            new EmptyDispatcher(repository),
            NullLogger<AccountingRegisterEntryRebuildService>.Instance,
            unitOfWork);

        var result = await service.RebuildAsync(DocumentTypeIdConst.BANKOPERATION, 42);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingRegisterEntry.RebuildProducedNoEntries", result.Error.Code);
        Assert.False(unitOfWork.Committed);
        Assert.True(unitOfWork.RolledBack);
    }

    private sealed class InMemoryRebuildRepository(
        AccountingRegisterEntryRebuildSource source,
        IEnumerable<AccountingRegisterEntry> entries)
        : IAccountingRegisterEntryRebuildRepository
    {
        public List<AccountingRegisterEntry> Entries { get; } = [.. entries];

        public bool Supports(short documentTypeId) => documentTypeId == DocumentTypeIdConst.BANKOPERATION;

        public Task<AccountingRegisterEntryRebuildSource?> GetSourceAsync(
            short documentTypeId,
            long documentId,
            int organizationId,
            CancellationToken ct = default) =>
            Task.FromResult<AccountingRegisterEntryRebuildSource?>(source);

        public Task<int> DeleteEntriesAsync(
            short documentTypeId,
            long documentId,
            int organizationId,
            CancellationToken ct = default)
        {
            var deleted = Entries.RemoveAll(entry =>
                entry.OrganizationId == organizationId &&
                entry.DocumentTypeId == documentTypeId &&
                entry.DocumentId == documentId);
            return Task.FromResult(deleted);
        }
    }

    private sealed class RebuildDispatcher(InMemoryRebuildRepository repository) : IAccountingDispatcher
    {
        public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(
            object document,
            CancellationToken ct = default,
            long? postingBatchId = null)
        {
            Assert.Empty(repository.Entries);
            var bankOperation = Assert.IsType<BankOperation>(document);
            var entry = new AccountingRegisterEntry
            {
                OrganizationId = bankOperation.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                DocumentId = bankOperation.Id,
                PostingBatchId = postingBatchId
            };
            repository.Entries.Add(entry);
            return Task.FromResult(Result.Success(new List<AccountingRegisterEntry> { entry }));
        }
    }

    private sealed class EmptyDispatcher(InMemoryRebuildRepository repository) : IAccountingDispatcher
    {
        public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(
            object document,
            CancellationToken ct = default,
            long? postingBatchId = null)
        {
            Assert.Empty(repository.Entries);
            return Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));
        }
    }

    private sealed class SuccessfulPostingLock : IDocumentPostingLock
    {
        public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireInventoryAsync(int organizationId, int warehouseId, IReadOnlyCollection<int> productIds, IReadOnlyCollection<int> productTableIds, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireMoneyAsync(int organizationId, string sourceType, int sourceId, CancellationToken ct = default) => Task.CompletedTask;
        public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }

        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default)
        {
            Committed = true;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken ct = default)
        {
            RolledBack = true;
            return Task.CompletedTask;
        }
    }

    private sealed class TestUserContext(int organizationId) : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
    }
}
