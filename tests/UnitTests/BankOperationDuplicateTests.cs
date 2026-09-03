using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.BankOperations;
using Application.Features.BankParsers;
using Application.Features.CashCollections;
using Application.Features.DocumentNumbers;
using Domain.Entities;
using Infrastructure.Query;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class BankOperationDuplicateTests
{
    [Fact]
    public async Task CreateManyAsync_RejectsDuplicateIdentitiesInsideRequest()
    {
        var repository = new InMemoryQueryRepository<BankOperation>();
        var queryBuilder = new QueryBuilder(new NullQueryBuilderResolver());
        var command = new RecordingCommandRepository<BankOperation>();
        var service = new BankOperationService(
            new TestUserContext(),
            queryBuilder,
            null!,
            null!,
            new StubDocumentNumberService(),
            new SuccessfulClassificationValidator(),
            new SuccessfulRelatedDocumentService(),
            new BankOperationDuplicateChecker(repository, queryBuilder),
            repository,
            command,
            NullLogger<BankOperationService>.Instance,
            new RecordingUnitOfWork());
        var first = new BankOperationCreateDto
        {
            BankAccountId = 11,
            BankDocumentNumber = "PAY-15",
            DocDate = new DateTime(2026, 8, 29, 8, 0, 0),
            DirectionId = MovementDirectionIdConst.IN,
            CurrencyId = 1,
            Amount = 100m
        };
        var second = new BankOperationCreateDto
        {
            BankAccountId = 11,
            BankDocumentNumber = " PAY-15 ",
            DocDate = new DateTime(2026, 8, 29, 18, 45, 0),
            DirectionId = MovementDirectionIdConst.OUT,
            CurrencyId = 1,
            Amount = 200m
        };

        var result = await service.CreateManyAsync(new BankOperationsCreateDto
        {
            Operations = [first, second]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("BankOperation.DuplicateBankDocumentNumber", result.Error.Code);
        Assert.Empty(command.Created);
    }

    [Fact]
    public async Task CreateAsync_RejectsExistingBankOperationIdentity()
    {
        var existing = new BankOperation
        {
            Id = 10,
            OrganizationId = 7,
            BankAccountId = 11,
            BankDocumentNumber = "42",
            DocDate = new DateTime(2026, 8, 29, 8, 15, 0),
            StateId = StateIdConst.ACTIVE
        };
        var repository = new InMemoryQueryRepository<BankOperation>(existing);
        var queryBuilder = new QueryBuilder(new NullQueryBuilderResolver());
        var command = new RecordingCommandRepository<BankOperation>();
        var service = new BankOperationService(
            new TestUserContext(),
            queryBuilder,
            null!,
            null!,
            null!,
            null!,
            null!,
            new BankOperationDuplicateChecker(repository, queryBuilder),
            repository,
            command,
            NullLogger<BankOperationService>.Instance,
            new RecordingUnitOfWork());

        var result = await service.CreateAsync(new BankOperationCreateDto
        {
            BankAccountId = 11,
            BankDocumentNumber = " 42 ",
            DocDate = new DateTime(2026, 8, 29, 22, 30, 0),
            DirectionId = MovementDirectionIdConst.IN,
            CurrencyId = 1,
            Amount = 100m
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("BankOperation.DuplicateBankDocumentNumber", result.Error.Code);
        Assert.Empty(command.Created);
    }

    [Fact]
    public async Task ParseAsync_MarksExistingTransactionAsNotNew()
    {
        var bank = new Bank { Id = 2, Code = "SQB", Name = "SQB" };
        var bankAccount = new BankAccount
        {
            Id = 20,
            OrganizationId = 7,
            BankId = bank.Id,
            Bank = bank,
            AccountNumber = "20208000607099548001",
            StateId = StateIdConst.ACTIVE
        };
        var operationRepository = new InMemoryQueryRepository<BankOperation>(
            new BankOperation
            {
                Id = 10,
                OrganizationId = 7,
                BankAccountId = bankAccount.Id,
                BankDocumentNumber = "27",
                DocDate = new DateTime(2024, 7, 8, 23, 59, 59),
                StateId = StateIdConst.ACTIVE
            });
        var queryBuilder = new QueryBuilder(new NullQueryBuilderResolver());
        var service = new BankStatementParserService(
            new TestUserContext(),
            new InMemoryQueryRepository<BankStatementTemplate>(
                BankStatementTemplateTestData.CreateUzsanoatqurilishbank()),
            new InMemoryQueryRepository<Bank>(bank),
            new InMemoryQueryRepository<BankBranch>(),
            new InMemoryQueryRepository<BankAccount>(bankAccount),
            new InMemoryQueryRepository<CounterpartyCard>(),
            new InMemoryQueryRepository<CounterpartyBankAccount>(),
            queryBuilder,
            new PassThroughClassifier(),
            new BankOperationDuplicateChecker(operationRepository, queryBuilder));
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseAsync(stream, bank.Id);

        Assert.True(result.IsSuccess);
        var transactions = Assert.Single(result.Value.Accounts).Transactions;
        Assert.False(Assert.Single(transactions, x => x.BankDocumentNumber == "27").IsNewOperation);
        Assert.True(Assert.Single(transactions, x => x.BankDocumentNumber == "204243890").IsNewOperation);
    }

    [Fact]
    public async Task FindExistingAsync_MatchesCalendarDateAndIgnoresTime()
    {
        var repository = new InMemoryQueryRepository<BankOperation>(
            new BankOperation
            {
                Id = 10,
                OrganizationId = 7,
                BankAccountId = 11,
                BankDocumentNumber = "42",
                DocDate = new DateTime(2026, 8, 29, 8, 15, 0),
                StateId = StateIdConst.ACTIVE
            });
        var checker = new BankOperationDuplicateChecker(
            repository,
            new QueryBuilder(new NullQueryBuilderResolver()));
        var sameDay = new BankOperationIdentity(11, "42", new DateOnly(2026, 8, 29));
        var nextDay = new BankOperationIdentity(11, "42", new DateOnly(2026, 8, 30));

        var existing = await checker.FindExistingAsync(7, [sameDay, nextDay]);

        Assert.Single(existing);
        Assert.Contains(sameDay, existing);
        Assert.DoesNotContain(nextDay, existing);
    }

    private sealed class InMemoryQueryRepository<TEntity>(params TEntity[] entities)
        : IQueryRepository<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _entities = [.. entities];

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().FirstOrDefault(specification.Criteria));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .FirstOrDefault(specification.ResultCriteria));

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .Where(specification.ResultCriteria)
                .ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingCommandRepository<TEntity> : ICommandRepository<TEntity>
        where TEntity : class
    {
        public List<TEntity> Created { get; } = [];

        public Task CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            Created.Add(entity);
            return Task.CompletedTask;
        }

        public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            Created.AddRange(entities);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
        public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubDocumentNumberService : IDocumentNumberService
    {
        private long _next;

        public Task<Result<DocumentNumberResult>> GetNextAsync(
            int organizationId,
            short documentTypeId,
            DateTime documentDate,
            CancellationToken ct = default) =>
            Task.FromResult(Result.Success(new DocumentNumberResult(
                ++_next,
                _next.ToString(),
                documentDate)));

        public Task<Result<DocumentNumberResult>> GetNextHistoricalAsync(
            int organizationId,
            short documentTypeId,
            DateTime documentDate,
            CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class SuccessfulClassificationValidator : IBankOperationClassificationSelectionValidator
    {
        public Task<Result> ValidateAsync(
            int organizationId,
            int bankAccountId,
            short? categoryId,
            int? ruleId,
            CancellationToken ct = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class SuccessfulRelatedDocumentService : IBankOperationRelatedDocumentService
    {
        public Task<Result<BankOperationRelatedDocumentLink?>> ResolveDraftAsync(
            long? relatedDocumentId,
            int organizationId,
            int bankAccountId,
            short directionId,
            short currencyId,
            decimal amount,
            short? classificationCategoryId,
            long? currentBankOperationId,
            CancellationToken ct) =>
            Task.FromResult(Result.Success<BankOperationRelatedDocumentLink?>(null));

        public Task<Result<CashCollectionDoc?>> ValidateCashCollectionForConfirmAsync(BankOperation operation, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result<CashCollectionDoc?>> GetLinkedCashCollectionAsync(BankOperation operation, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<bool> HasActiveCashCollectionLinkAsync(long cashCollectionDocId, long? excludedBankOperationId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyDictionary<long, long>> GetActiveCashCollectionBankOperationIdsAsync(
            IReadOnlyCollection<long> cashCollectionDocIds,
            CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyDictionary<long, long>> GetCashCollectionRegistryIdsAsync(
            IReadOnlyCollection<long> cashCollectionDocIds,
            CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private sealed class NullQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>()
        {
            if (typeof(TEntity) == typeof(BankOperation) && typeof(TResult) == typeof(BankOperationDto))
                return (IProjectionBuilder<TEntity, TResult>)(object)new BankOperationDtoProjection();

            throw new NotSupportedException();
        }
        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }

    private sealed class PassThroughClassifier : IBankOperationClassifier
    {
        public Task<Result<BankExportDto>> ClassifyAsync(
            BankExportDto export,
            int bankId,
            CancellationToken ct = default) =>
            Task.FromResult(Result.Success(export));
    }

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => 7;
        public List<int> AllowedOrganizationIds => [7];
        public int? BranchId => null;
    }

    private static MemoryStream CreateWorkbookStream()
    {
        using var workbook = BankStatementTemplateParserTests.CreateUzsanoatqurilishbankWorkbook();
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
