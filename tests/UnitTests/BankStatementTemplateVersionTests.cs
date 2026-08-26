using Application.Abstractions;
using Application.Features.BankParsers;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
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

    private static BankStatementParserService CreateService(params BankStatementTemplate[] templates) =>
        new(
            null!,
            new InMemoryQueryRepository<BankStatementTemplate>(templates),
            new InMemoryQueryRepository<Bank>(),
            new InMemoryQueryRepository<BankBranch>(),
            new InMemoryQueryRepository<BankAccount>(),
            new InMemoryQueryRepository<CounterpartyCard>(),
            new InMemoryQueryRepository<CounterpartyBankAccount>(),
            new QueryBuilder(new NullQueryBuilderResolver()));

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
}
