using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AiAssistant;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using SharedKernel.Time;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class ContractExpiryLookupToolTests
{
    [Fact]
    public async Task ReturnsExpiredTodayAndThirtyDayBoundaryButExcludesOtherContracts()
    {
        var today = TashkentTime.Today;
        var tool = CreateTool(
            allowedOrganizationIds: [10],
            currentOrganizationId: 10,
            contracts:
            [
                Contract("expired", 10, today.AddDays(-1)),
                Contract("today", 10, today),
                Contract("thirty", 10, today.AddDays(30)),
                Contract("thirty-one", 10, today.AddDays(31)),
                Contract("inactive", 10, today.AddDays(5), StateIdConst.PASSIVE),
                Contract("no-end", 10, null)
            ]);

        var result = await tool.ExecuteAsync(ExecutionRequest(10, AiLanguage.English));

        Assert.Equal(AiToolResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal(["expired", "today", "thirty"], result.Data!.Items.Select(item => item.ContractNumber));
        Assert.Equal(-1, result.Data.Items[0].DaysFromToday);
        Assert.Equal(0, result.Data.Items[1].DaysFromToday);
        Assert.Equal(30, result.Data.Items[2].DaysFromToday);
        Assert.Equal("Expired", result.Data.Items[0].StatusLabel);
        Assert.Equal("Expiring soon", result.Data.Items[1].StatusLabel);
    }

    [Fact]
    public async Task ReturnsNoDataWhenNoEligibleContractExists()
    {
        var tool = CreateTool([10], 10, [Contract("inactive", 10, TashkentTime.Today, StateIdConst.PASSIVE)]);

        var result = await tool.ExecuteAsync(ExecutionRequest(10, AiLanguage.English));

        Assert.Equal(AiToolResultStatus.NoData, result.Status);
        Assert.Null(result.Data);
        Assert.Single(result.Evidence);
    }

    [Fact]
    public async Task RejectsOrganizationOutsideAllowedScope()
    {
        var tool = CreateTool([10], 10, [Contract("other-org", 20, TashkentTime.Today)]);

        var result = await tool.ExecuteAsync(ExecutionRequest(20, AiLanguage.English));

        Assert.Equal(AiToolResultStatus.Error, result.Status);
        Assert.Equal(AiToolErrorKind.UnauthorizedOrganization, result.Error?.Kind);
    }

    [Fact]
    public async Task OrchestratorUsesDefaultOrganizationAndDoesNotCallProvider()
    {
        var today = TashkentTime.Today;
        var provider = new NoCallProvider();
        var orchestrator = CreateOrchestrator(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10, 20],
            organizations: [Organization(10, "Alpha LLC", "100000001"), Organization(20, "Beta LLC", "100000002")],
            contracts: [Contract("alpha-contract", 10, today), Contract("beta-contract", 20, today)],
            provider);

        var response = await orchestrator.ExecuteAsync(
            new AiAssistantRequest("contract expiry", AccountingToolNames.ContractExpiryLookup));

        Assert.Equal(AiEvidenceState.Grounded, response.EvidenceState);
        Assert.Equal("Alpha LLC", response.Organization?.Name);
        Assert.Equal("100000001", response.Organization?.Inn);
        Assert.Contains("alpha-contract", response.Answer);
        Assert.DoesNotContain("beta-contract", response.Answer);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task OrchestratorResolvesRequestedOrganizationOnlyByAllowedNameAndInn()
    {
        var today = TashkentTime.Today;
        var orchestrator = CreateOrchestrator(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10, 20],
            organizations: [Organization(10, "Alpha LLC", "100000001"), Organization(20, "Beta LLC", "100000002")],
            contracts: [Contract("alpha-contract", 10, today), Contract("beta-contract", 20, today)],
            new NoCallProvider());

        var response = await orchestrator.ExecuteAsync(new AiAssistantRequest(
            "contract expiry",
            AccountingToolNames.ContractExpiryLookup,
            Organization: new AiOrganizationContextRequest(Name: "beta llc", Inn: "100000002")));

        Assert.Equal(AiEvidenceState.Grounded, response.EvidenceState);
        Assert.Equal("Beta LLC", response.Organization?.Name);
        Assert.Equal("100000002", response.Organization?.Inn);
        Assert.Contains("beta-contract", response.Answer);
        Assert.DoesNotContain("alpha-contract", response.Answer);
    }

    [Fact]
    public async Task OrchestratorRejectsUnauthorizedOrganizationBeforeQueryingContracts()
    {
        var toolRepository = new InMemoryQueryRepository<Contract>([Contract("other-org", 20, TashkentTime.Today)]);
        var context = new TestUserContext(10, [10]);
        var tool = new ContractExpiryLookupTool(context, toolRepository);
        var resolver = new OrganizationContextResolver(
            context,
            new InMemoryQueryRepository<Organization>([Organization(10, "Alpha LLC", "100000001"), Organization(20, "Beta LLC", "100000002")]));
        var orchestrator = new AiAssistantOrchestrator(
            resolver,
            new LanguageContextResolver(context),
            new AiToolExecutionGuard(resolver),
            new AiReadOnlyToolRegistry([tool]),
            new NoCallProvider());

        var response = await orchestrator.ExecuteAsync(new AiAssistantRequest(
            "contract expiry",
            AccountingToolNames.ContractExpiryLookup,
            Organization: new AiOrganizationContextRequest(OrganizationId: 20)));

        Assert.Equal(AiSafeFallbackKind.UnauthorizedOrganization, response.SafeFallback?.Kind);
        Assert.Equal(0, toolRepository.GetAllCallCount);
    }

    [Fact]
    public async Task OrchestratorReturnsNoDataFallbackForNoEligibleContracts()
    {
        var orchestrator = CreateOrchestrator(
            currentOrganizationId: 10,
            allowedOrganizationIds: [10],
            organizations: [Organization(10, "Alpha LLC", "100000001")],
            contracts: [Contract("outside-window", 10, TashkentTime.Today.AddDays(31))],
            new NoCallProvider());

        var response = await orchestrator.ExecuteAsync(
            new AiAssistantRequest("contract expiry", AccountingToolNames.ContractExpiryLookup));

        Assert.Equal(AiEvidenceState.NoData, response.EvidenceState);
        Assert.Equal(AiSafeFallbackKind.NoData, response.SafeFallback?.Kind);
        Assert.Equal("Alpha LLC", response.Organization?.Name);
    }

    [Theory]
    [InlineData(AiLanguage.UzbekLatin, "Muddati tugagan")]
    [InlineData(AiLanguage.UzbekCyrillic, "Муддати тугаган")]
    [InlineData(AiLanguage.Russian, "Срок истёк")]
    [InlineData(AiLanguage.English, "Expired")]
    public async Task BuildsHumanReadableAnswerInRequestedLanguage(AiLanguage language, string expectedText)
    {
        var tool = CreateTool([10], 10, [Contract("expired", 10, TashkentTime.Today.AddDays(-1))]);

        var result = await tool.ExecuteAsync(new AiToolInvocationRequest(
            new ContractExpiryLookupInput(),
            new AiOrganizationContext(10, "Alpha LLC", "100000001"),
            AiLanguageContext.From(language, true, false)));

        Assert.Equal(AiToolResultStatus.Success, result.Status);
        Assert.Contains(expectedText, result.Answer);
        Assert.Contains("Alpha LLC", result.Answer);
        Assert.Contains("100000001", result.Answer);
    }

    [Fact]
    public void HasNoNotificationDependency()
    {
        var dependencies = typeof(ContractExpiryLookupTool)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType.Name);

        Assert.DoesNotContain("INotificationService", dependencies);
        Assert.DoesNotContain("ICommandRepository`1", dependencies);
    }

    private static ContractExpiryLookupTool CreateTool(
        IReadOnlyCollection<int> allowedOrganizationIds,
        int currentOrganizationId,
        IReadOnlyCollection<Contract> contracts) =>
        new(new TestUserContext(currentOrganizationId, allowedOrganizationIds), new InMemoryQueryRepository<Contract>(contracts));

    private static AiToolExecutionRequest<ContractExpiryLookupInput> ExecutionRequest(int organizationId, AiLanguage language) =>
        new(
            new ContractExpiryLookupInput(),
            new AiOrganizationContext(organizationId, "Alpha LLC", "100000001"),
            AiLanguageContext.From(language, true, false));

    private static AiAssistantOrchestrator CreateOrchestrator(
        int currentOrganizationId,
        IReadOnlyCollection<int> allowedOrganizationIds,
        IReadOnlyCollection<Organization> organizations,
        IReadOnlyCollection<Contract> contracts,
        IAiProvider provider)
    {
        var context = new TestUserContext(currentOrganizationId, allowedOrganizationIds);
        var resolver = new OrganizationContextResolver(context, new InMemoryQueryRepository<Organization>(organizations));
        var tool = new ContractExpiryLookupTool(context, new InMemoryQueryRepository<Contract>(contracts));

        return new AiAssistantOrchestrator(
            resolver,
            new LanguageContextResolver(context),
            new AiToolExecutionGuard(resolver),
            new AiReadOnlyToolRegistry([tool]),
            provider);
    }

    private static Contract Contract(string number, int organizationId, DateTime? endDate, short stateId = StateIdConst.ACTIVE) => new()
    {
        OrganizationId = organizationId,
        CounterpartyId = 1,
        ContractTypeId = 1,
        ContractNumber = number,
        ContractDate = TashkentTime.Today,
        EndDate = endDate,
        StateId = stateId,
        Counterparty = new CounterpartyCard { ShortName = "Counterparty" }
    };

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        StateId = StateIdConst.ACTIVE
    };

    private sealed class TestUserContext(int? organizationId, IReadOnlyCollection<int> allowedOrganizationIds) : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.EN;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => allowedOrganizationIds.ToList();
        public int? BranchId => null;
    }

    private sealed class NoCallProvider : IAiProvider
    {
        public int CallCount { get; private set; }

        public Task<AiProviderChatResult> CompleteAsync(AiProviderChatRequest request, CancellationToken ct = default)
        {
            CallCount++;
            throw new InvalidOperationException("Provider must not be called for a deterministic tool response.");
        }
    }

    private sealed class InMemoryQueryRepository<TEntity>(IReadOnlyCollection<TEntity> items) : IQueryRepository<TEntity>
        where TEntity : class
    {
        public int GetAllCallCount { get; private set; }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).FirstOrDefault());

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault());

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            GetAllCallCount++;
            var query = items.AsQueryable().Where(specification.Criteria);
            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);
            return Task.FromResult(query.ToList());
        }

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            GetAllCallCount++;
            var query = items.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector);
            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);
            return Task.FromResult(query.Where(specification.ResultCriteria).ToList());
        }

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
