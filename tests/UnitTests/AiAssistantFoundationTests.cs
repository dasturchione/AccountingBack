using Application.Abstractions.Authentication;
using Application.Abstractions;
using Application.Features.AiAssistant;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class AiAssistantFoundationTests
{
    [Fact]
    public async Task DefaultOrganizationIsResolvedOnlyFromAllowedContext()
    {
        var resolver = CreateOrganizationResolver(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10],
            organizations: [Organization(10, "Alpha LLC", "100000001")]);

        var result = await resolver.ResolveAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.OrganizationId);
        Assert.Equal("Alpha LLC", result.Value.Name);
    }

    [Fact]
    public async Task UnauthorizedOrganizationIsNotResolved()
    {
        var resolver = CreateOrganizationResolver(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10],
            organizations: [Organization(10, "Alpha LLC", "100000001"), Organization(20, "Beta LLC", "100000002")]);

        var result = await resolver.ResolveAsync(new AiOrganizationContextRequest(OrganizationId: 20));

        Assert.False(result.IsSuccess);
        Assert.Equal("AiAssistant.OrganizationContextUnavailable", result.Error.Code);
    }

    [Fact]
    public async Task ToolExecutionGuardRejectsOrganizationOutsideAllowedScope()
    {
        var guard = new AiToolExecutionGuard(CreateOrganizationResolver(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10],
            organizations: [Organization(10, "Alpha LLC", "100000001")]));

        var result = await guard.ValidateAsync(new AiOrganizationContext(20, "Beta LLC", "100000002"));

        Assert.False(result.IsSuccess);
        Assert.Equal("AiAssistant.OrganizationContextUnavailable", result.Error.Code);
    }

    [Fact]
    public async Task NameAndInnResolveTheSameAllowedOrganization()
    {
        var resolver = CreateOrganizationResolver(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10, 20],
            organizations: [Organization(10, "Alpha LLC", "100000001"), Organization(20, "Beta LLC", "100000002")]);

        var result = await resolver.ResolveAsync(
            new AiOrganizationContextRequest(Name: " beta llc ", Inn: "100000002"));

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value.OrganizationId);
        Assert.Equal("100000002", result.Value.Inn);
    }

    [Fact]
    public void InvalidLanguageFallsBackToDetectedUserLanguage()
    {
        var resolver = new LanguageContextResolver(new TestUserContext { LanguageIdValue = LanguageIdConst.EN });

        var result = resolver.Resolve("unknown", "shartnoma bo'yicha hisobot kerak");

        Assert.Equal(AiLanguage.UzbekLatin, result.Language);
        Assert.True(result.IsDetected);
        Assert.Equal("uz", result.Code);
    }

    [Fact]
    public void MissingLanguageFallsBackToProjectUserContext()
    {
        var resolver = new LanguageContextResolver(new TestUserContext { LanguageIdValue = LanguageIdConst.RU });

        var result = resolver.Resolve(null, null);

        Assert.Equal(AiLanguage.Russian, result.Language);
        Assert.False(result.IsDetected);
        Assert.Equal("ru", result.Code);
    }

    [Fact]
    public void ToolRegistryExposesOnlyTheReadOnlyAllowlist()
    {
        var registry = new AiReadOnlyToolRegistry([]);

        Assert.Equal(5, registry.Descriptors.Count);
        Assert.True(registry.IsAllowed(AccountingToolNames.ContractExpiryLookup));
        Assert.True(registry.IsAllowed(AccountingToolNames.EdoMappingIssueLookup));
        Assert.False(registry.IsAllowed("accounting.write-off.execute"));
        Assert.False(registry.TryGet("accounting.write-off.execute", out _));
    }

    [Fact]
    public async Task OrchestratorReturnsSafeNoDataResponse()
    {
        var orchestrator = CreateOrchestrator(
            new StubAiToolExecutor(
                AccountingToolNames.ContractExpiryLookup,
                AiToolExecutionOutcome.NoData()));

        var response = await orchestrator.ExecuteAsync(
            new AiAssistantRequest("contract expiry", AccountingToolNames.ContractExpiryLookup));

        Assert.Equal(AiEvidenceState.NoData, response.EvidenceState);
        Assert.Equal(AiSafeFallbackKind.NoData, response.SafeFallback?.Kind);
        Assert.Equal("Alpha LLC", response.Organization?.Name);
    }

    [Fact]
    public async Task OrchestratorReturnsProviderUnavailableFallbackWithoutPayload()
    {
        var orchestrator = CreateOrchestrator(
            new StubAiToolExecutor(
                AccountingToolNames.ContractExpiryLookup,
                AiToolExecutionOutcome.Success(
                [new AiToolEvidence("contracts", "One contract expires within the selected period.")])));

        var response = await orchestrator.ExecuteAsync(
            new AiAssistantRequest("contract expiry", AccountingToolNames.ContractExpiryLookup));

        Assert.Equal(AiSafeFallbackKind.ProviderUnavailable, response.SafeFallback?.Kind);
        Assert.DoesNotContain("provider", response.Answer, StringComparison.OrdinalIgnoreCase);
        Assert.Single(response.Evidence);
    }

    [Fact]
    public void UzbekCyrillicIsDetectedSeparatelyFromRussian()
    {
        var resolver = new LanguageContextResolver(new TestUserContext { LanguageIdValue = LanguageIdConst.EN });

        var result = resolver.Resolve(null, "шартнома бўйича ҳисобот керак");

        Assert.Equal(AiLanguage.UzbekCyrillic, result.Language);
        Assert.True(result.IsDetected);
        Assert.Equal(LanguageCodeConst.UZ_CYRL, result.Code);
    }

    [Fact]
    public void EnglishAndRussianLanguageCodesAreSupported()
    {
        var resolver = new LanguageContextResolver(new TestUserContext { LanguageIdValue = LanguageIdConst.EN });

        Assert.Equal(AiLanguage.English, resolver.Resolve("en", null).Language);
        Assert.Equal(AiLanguage.Russian, resolver.Resolve("ru", null).Language);
    }

    [Fact]
    public async Task AllowedToolWithoutRegisteredExecutorGetsSafeFallback()
    {
        var orchestrator = CreateOrchestrator();

        var response = await orchestrator.ExecuteAsync(
            new AiAssistantRequest("contract expiry", AccountingToolNames.ContractExpiryLookup));

        Assert.Equal(AiSafeFallbackKind.ToolUnavailable, response.SafeFallback?.Kind);
        Assert.Empty(response.Evidence);
    }

    [Fact]
    public async Task UnavailableProviderDoesNotReturnProviderPayload()
    {
        var provider = new UnavailableAiProvider();
        var result = await provider.CompleteAsync(
            new AiProviderChatRequest([], AiLanguageContext.From(AiLanguage.English, false, false)));

        Assert.Equal(AiProviderChatStatus.NotConfigured, result.Status);
        Assert.Null(result.Response);
        Assert.Equal(AiProviderErrorKind.NotConfigured, result.Error?.Kind);
    }

    private static AiAssistantOrchestrator CreateOrchestrator(
        params IAiReadOnlyToolExecutor[] executors)
    {
        var resolver = CreateOrganizationResolver(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10],
            organizations: [Organization(10, "Alpha LLC", "100000001")]);

        return new AiAssistantOrchestrator(
            resolver,
            new LanguageContextResolver(new TestUserContext { LanguageIdValue = LanguageIdConst.EN }),
            new AiToolExecutionGuard(resolver),
            new AiReadOnlyToolRegistry(executors),
            new UnavailableAiProvider());
    }

    private static OrganizationContextResolver CreateOrganizationResolver(
        int currentOrganizationId,
        IReadOnlyCollection<int> allowedOrganizationIds,
        IReadOnlyCollection<Organization> organizations) =>
        new(
            new TestUserContext
            {
                OrganizationIdValue = currentOrganizationId,
                AllowedOrganizationIdsValue = allowedOrganizationIds.ToList()
            },
            new InMemoryQueryRepository<Organization>(organizations));

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        StateId = StateIdConst.ACTIVE
    };

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdValue;
        public short? LanguageIdValue { get; init; }
        public int? TenantId => 1;
        public int? OrganizationId => OrganizationIdValue;
        public int? OrganizationIdValue { get; init; }
        public List<int> AllowedOrganizationIds => AllowedOrganizationIdsValue;
        public List<int> AllowedOrganizationIdsValue { get; init; } = [];
        public int? BranchId => null;
    }

    private sealed class InMemoryQueryRepository<TEntity> : IQueryRepository<TEntity>
        where TEntity : class
    {
        private readonly IReadOnlyCollection<TEntity> _items;

        public InMemoryQueryRepository(IReadOnlyCollection<TEntity> items) => _items = items;

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Where(specification.Criteria).FirstOrDefault());

        public Task<TResult?> GetAsync<TResult>(
            QuerySpecification<TEntity, TResult> specification,
            CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector.Compile())
                .FirstOrDefault());

        public Task<List<TEntity>> GetAllAsync(
            QuerySpecification<TEntity> specification,
            CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(
            QuerySpecification<TEntity, TResult> specification,
            CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector.Compile())
                .Where(specification.ResultCriteria.Compile())
                .ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(
            PagedQuerySpecification<TEntity> specification,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(
            PagedQuerySpecification<TEntity, TResult> specification,
            CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class StubAiToolExecutor(
        string name,
        AiToolExecutionOutcome outcome) : IAiReadOnlyToolExecutor
    {
        public AiToolDescriptor Descriptor { get; } =
            new(name, true, AiToolAuthorizationRequirement.AuthenticatedOrganizationMember);

        public Task<AiToolExecutionOutcome> ExecuteAsync(
            AiToolInvocationRequest request,
            CancellationToken ct = default) =>
            Task.FromResult(outcome);
    }
}
