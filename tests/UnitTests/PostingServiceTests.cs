using Application.Abstractions;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public class PostingServiceTests
{
    [Fact]
    public async Task BuildEntriesAsync_ShouldUseDirectEntryAndContextMetadata()
    {
        var service = CreateService([
            Account(1027),
            Account(1015)
        ]);

        var docDate = new DateTime(2026, 1, 10);

        var entries = await service.BuildEntriesAsync([
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                DocumentId = 12,
                CurrencyId = 1,
                DocDate = docDate,
                JournalNumber = "BNK-12",
                Entries =
                [
                    new PostingEntryContext
                    {
                        DebitAccountId = 1027,
                        CreditAccountId = 1015,
                        Amount = 100m,
                        Content = "Bank receipt"
                    }
                ]
            }
        ]);

        var entry = Assert.Single(entries);
        Assert.Equal(DocumentTypeIdConst.BANKOPERATION, entry.DocumentTypeId);
        Assert.Equal(12, entry.DocumentId);
        Assert.Equal(1027, entry.DebitAccountId);
        Assert.Equal(1015, entry.CreditAccountId);
        Assert.Equal(100m, entry.Amount);
        Assert.Equal(docDate, entry.DocDate);
        Assert.Equal("BNK-12", entry.JournalNumber);
        Assert.Equal("Bank receipt", entry.Content);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldApplyQuantityOnlyForQuantityAccounts()
    {
        var service = CreateService([
            Account(2910, isQuantity: true),
            Account(6010)
        ]);

        var entries = await service.BuildEntriesAsync([
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                DocumentId = 20,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                Entries =
                [
                    new PostingEntryContext
                    {
                        DebitAccountId = 2910,
                        CreditAccountId = 6010,
                        Amount = 120m,
                        DebitQuantity = 3m,
                        CreditQuantity = 3m
                    }
                ]
            }
        ]);

        var entry = Assert.Single(entries);
        Assert.Equal(3m, entry.DebitQuantity);
        Assert.Null(entry.CreditQuantity);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldAttachConfiguredSubkontosWithFallbackType()
    {
        var service = CreateService([
            Account(1027),
            Account(1015,
                new ChartAccountSubkonto
                {
                    SubkontoTypeId = SubkontoTypeIdConst.CounterpartiesTurnover,
                    SortOrder = 1,
                    IsRequired = true,
                    StateId = StateIdConst.ACTIVE
                })
        ]);

        var entries = await service.BuildEntriesAsync([
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                DocumentId = 12,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                Subkontos =
                [
                    new SubkontoValue
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                        EntityId = 55,
                        DisplayValue = "Customer A"
                    }
                ],
                Entries =
                [
                    new PostingEntryContext
                    {
                        DebitAccountId = 1027,
                        CreditAccountId = 1015,
                        Amount = 100m
                    }
                ]
            }
        ]);

        var entry = Assert.Single(entries);
        var subkonto = Assert.Single(entry.RegisterEntrySubkontos);
        Assert.Equal(SubkontoSideConst.CREDIT, subkonto.Side);
        Assert.Equal(SubkontoTypeIdConst.CounterpartiesTurnover, subkonto.SubkontoTypeId);
        Assert.Equal(55, subkonto.EntityId);
        Assert.Equal("Customer A", subkonto.DisplayValue);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldFilterSubkontosByAppliesToAccountId()
    {
        var service = CreateService([
            Account(5010,
                new ChartAccountSubkonto
                {
                    SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                    SortOrder = 1,
                    IsRequired = true,
                    StateId = StateIdConst.ACTIVE
                }),
            Account(5020,
                new ChartAccountSubkonto
                {
                    SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                    SortOrder = 1,
                    IsRequired = true,
                    StateId = StateIdConst.ACTIVE
                })
        ]);

        var entries = await service.BuildEntriesAsync([
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                DocumentId = 7,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                Subkontos =
                [
                    new SubkontoValue
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                        EntityId = 1,
                        DisplayValue = "Source",
                        AppliesToAccountId = 5010
                    },
                    new SubkontoValue
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                        EntityId = 2,
                        DisplayValue = "Destination",
                        AppliesToAccountId = 5020
                    }
                ],
                Entries =
                [
                    new PostingEntryContext
                    {
                        DebitAccountId = 5020,
                        CreditAccountId = 5010,
                        Amount = 50m
                    }
                ]
            }
        ]);

        var entry = Assert.Single(entries);
        Assert.Contains(entry.RegisterEntrySubkontos, x => x.Side == SubkontoSideConst.CREDIT && x.EntityId == 1);
        Assert.Contains(entry.RegisterEntrySubkontos, x => x.Side == SubkontoSideConst.DEBIT && x.EntityId == 2);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldThrowWhenNoDirectEntriesProvided()
    {
        var service = CreateService([]);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.BuildEntriesAsync([
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                DocumentId = 12,
                CurrencyId = 1,
                DocDate = DateTime.Today
            }
        ]));

        Assert.Contains("12", exception.Message);
    }

    private static PostingService CreateService(List<ChartAccount> accounts) =>
        new(new FakeQueryBuilder(), new FakeQueryRepository<ChartAccount>(accounts));

    private static ChartAccount Account(int id, params ChartAccountSubkonto[] subkontos) =>
        Account(id, false, subkontos);

    private static ChartAccount Account(int id, bool isQuantity, params ChartAccountSubkonto[] subkontos)
    {
        var account = new ChartAccount
        {
            Id = id,
            Number = id.ToString(),
            Name = $"Account {id}",
            IsQuantity = isQuantity,
            StateId = StateIdConst.ACTIVE,
            ChartAccountSubkontos = subkontos.ToList()
        };

        foreach (var subkonto in account.ChartAccountSubkontos)
            subkonto.AccountId = id;

        return account;
    }

    private sealed class FakeQueryBuilder : IQueryBuilder
    {
        public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
            new(new QueryState<TEntity> { Resolver = new FakeResolver() });

        public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
            throw new NotSupportedException();

        public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
            throw new NotSupportedException();

        public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter =>
            throw new NotSupportedException();

        public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter =>
            throw new NotSupportedException();
    }

    private sealed class FakeResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
            throw new NotSupportedException();
    }

    private sealed class FakeQueryRepository<TEntity>(List<TEntity> items) : IQueryRepository<TEntity>
        where TEntity : class
    {
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().FirstOrDefault(specification.Criteria));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).Select(specification.Selector).ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
