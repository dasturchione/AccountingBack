using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.BankOperations;
using Application.Features.BankParsers;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Constants;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class BankStatementTemplateVersionTests
{
    [Fact]
    public async Task ParseExcelAsync_UsesHighestMatchingVersionFirst()
    {
        var version1 = BankStatementTemplateTestData.CreateUzsanoatqurilishbank(version: 1);
        var version2 = BankStatementTemplateTestData.CreateUzsanoatqurilishbank(version: 2);
        version2.Code = "UZSANOATQURILISHBANK_XLSX_V2";
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").ExtractRegex = null;
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").ExtractGroup = null;
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").SourceType = "CONSTANT";
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").AnchorCode = null;
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").AbsoluteRowIndex = null;
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").ColumnIndex = null;
        version2.Fields.Single(field => field.TargetCode == "BANK_NAME").ConstantValue = "VERSION TWO";
        var service = CreateService(version1, version2);
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseExcelAsync(stream, bankId: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal("VERSION TWO", Assert.Single(result.Value.Accounts).BankName);
    }

    [Fact]
    public async Task ParseExcelAsync_FallsBackWhenNewerVersionDoesNotMatch()
    {
        var version1 = BankStatementTemplateTestData.CreateUzsanoatqurilishbank(version: 1);
        var version2 = BankStatementTemplateTestData.CreateUzsanoatqurilishbank(version: 2);
        version2.Code = "UZSANOATQURILISHBANK_XLSX_V2";
        version2.HeaderRules.First().ExpectedValue = "Несуществующий заголовок";
        var service = CreateService(version1, version2);
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseExcelAsync(stream, bankId: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "ТОШКЕНТ Ш., \"УЗСАНОАТКУРИЛИШБАНКИ\" АТБ БОШ ОФИСИ",
            Assert.Single(result.Value.Accounts).BankName);
    }

    [Fact]
    public async Task ParseExcelAsync_ReturnsSuccessfulEmptyExportWhenNoVersionMatches()
    {
        var template = BankStatementTemplateTestData.CreateUzsanoatqurilishbank();
        template.HeaderRules.First().ExpectedValue = "Несуществующий заголовок";
        var service = CreateService(template);
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseExcelAsync(stream, bankId: 2);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Accounts);
    }

    [Fact]
    public async Task ParseAsync_InvokesMandatoryOperationClassifierAfterParsingAndEnrichment()
    {
        var classifier = new RecordingClassifier();
        var service = CreateService(classifier, BankStatementTemplateTestData.CreateUzsanoatqurilishbank());
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseAsync(stream, bankId: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, classifier.BankId);
        Assert.Same(result.Value, classifier.Export);
    }

    [Fact]
    public async Task ParseAsync_UsesStatementBankAsCounterpartyForBankServicePaidToOwnInn()
    {
        var bank = new Bank
        {
            Id = 2,
            Code = "UZSANOATQURILISHBANK",
            Name = "Uzsanoatqurilishbank",
            Inn = "200833707",
            Mfo = "00440",
            StateId = StateIdConst.ACTIVE
        };
        var classifier = new BankServiceClassifier();
        var service = new BankStatementParserService(
            new TestUserContext(),
            new InMemoryQueryRepository<BankStatementTemplate>(BankStatementTemplateTestData.CreateUzsanoatqurilishbank()),
            new InMemoryQueryRepository<Bank>(bank),
            new InMemoryQueryRepository<BankBranch>(),
            new InMemoryQueryRepository<BankAccount>(new BankAccount
            {
                Id = 20,
                OrganizationId = 7,
                BankId = bank.Id,
                Bank = bank,
                AccountNumber = "20208000607099548001",
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<CounterpartyCard>(new CounterpartyCard
            {
                Id = 99,
                OrganizationId = 7,
                ShortName = bank.Name,
                Inn = bank.Inn,
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<CounterpartyBankAccount>(new CounterpartyBankAccount
            {
                Id = 88,
                OrganizationId = 7,
                CounterpartyId = 77,
                AccountNumber = "16401000907099548001",
                StateId = StateIdConst.ACTIVE
            }),
            new QueryBuilder(new NullQueryBuilderResolver()),
            classifier,
            new BankOperationDuplicateChecker(
                new InMemoryQueryRepository<BankOperation>(),
                new QueryBuilder(new NullQueryBuilderResolver())));
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseAsync(stream, bankId: bank.Id);

        Assert.True(result.IsSuccess);
        var transaction = Assert.Single(
            Assert.Single(result.Value.Accounts).Transactions,
            item => item.ClassificationCode == "BANK_SERVICE");
        Assert.Equal("200833707", transaction.CounterpartyInn);
        Assert.Equal("Uzsanoatqurilishbank", transaction.CounterpartyName);
        Assert.Equal("00440", transaction.MfoCounterparty);
        Assert.Equal(99, transaction.CounterpartyId);
        Assert.Null(transaction.CounterpartyBankAccountId);
    }

    [Fact]
    public async Task ParseAsync_DoesNotReplaceCounterpartyForOtherCategory()
    {
        var classifier = new BankServiceClassifier();
        var service = CreateService(classifier, BankStatementTemplateTestData.CreateUzsanoatqurilishbank());
        await using var stream = CreateWorkbookStream();

        var result = await service.ParseAsync(stream, bankId: 2);

        Assert.True(result.IsSuccess);
        var transaction = Assert.Single(
            Assert.Single(result.Value.Accounts).Transactions,
            item => item.ClassificationCode == "COUNTERPARTY");
        Assert.Equal("200833707", transaction.CounterpartyInn);
        Assert.Equal("Айланма кассадаги накд пуллар", transaction.CounterpartyName);
    }

    private static BankStatementParserService CreateService(params BankStatementTemplate[] templates) =>
        CreateService(null!, templates);

    private static BankStatementParserService CreateService(
        IBankOperationClassifier classifier,
        params BankStatementTemplate[] templates) =>
        new(
            new TestUserContext(),
            new InMemoryQueryRepository<BankStatementTemplate>(templates),
            new InMemoryQueryRepository<Bank>(),
            new InMemoryQueryRepository<BankBranch>(),
            new InMemoryQueryRepository<BankAccount>(),
            new InMemoryQueryRepository<CounterpartyCard>(),
            new InMemoryQueryRepository<CounterpartyBankAccount>(),
            new QueryBuilder(new NullQueryBuilderResolver()),
            classifier,
            new BankOperationDuplicateChecker(
                new InMemoryQueryRepository<BankOperation>(),
                new QueryBuilder(new NullQueryBuilderResolver())));

    private static MemoryStream CreateWorkbookStream()
    {
        using var workbook = BankStatementTemplateParserTests.CreateUzsanoatqurilishbankWorkbook();
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
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

    private sealed class NullQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => throw new NotSupportedException();
        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }

    private sealed class RecordingClassifier : IBankOperationClassifier
    {
        public int? BankId { get; private set; }
        public BankExportDto? Export { get; private set; }

        public Task<Result<BankExportDto>> ClassifyAsync(BankExportDto export, int bankId, CancellationToken ct = default)
        {
            Export = export;
            BankId = bankId;
            return Task.FromResult(Result.Success(export));
        }
    }

    private sealed class BankServiceClassifier : IBankOperationClassifier
    {
        public Task<Result<BankExportDto>> ClassifyAsync(
            BankExportDto export,
            int bankId,
            CancellationToken ct = default)
        {
            foreach (var account in export.Accounts)
            {
                foreach (var transaction in account.Transactions)
                {
                    transaction.ClassificationCode = transaction.CounterpartyInn == account.CompanyInn
                        ? "BANK_SERVICE"
                        : "COUNTERPARTY";
                }
            }

            return Task.FromResult(Result.Success(export));
        }
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
}
