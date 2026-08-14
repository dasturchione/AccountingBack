using Application.Abstractions.Authentication;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.Edo.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SharedKernel.Exceptions;

namespace IntegrationTests;

public sealed class EdoHistoricalImportPersistenceTests
{
    private static readonly DateTime Now = new(2026, 8, 11, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task RejectsSecondActiveJobForSameOrganization()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        await store.AddJobAsync(CreateJob(11));

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            store.AddJobAsync(CreateJob(11)));
    }

    [Fact]
    public async Task RejectsDuplicateJobProvider()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = CreateJob(11);
        await store.AddJobAsync(job);
        await store.AddProviderAsync(11, CreateProvider(job.Id, "EDOCS"));

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            store.AddProviderAsync(11, CreateProvider(job.Id, "EDOCS")));
    }

    [Fact]
    public async Task RejectsDuplicateCandidateProviderIdentity()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = CreateJob(11);
        await store.AddJobAsync(job);
        await store.AddProviderAsync(11, CreateProvider(job.Id, "DIDOX"));
        await store.AddCandidateAsync(CreateCandidate(job.Id, 11, "DIDOX", "document-1"));

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            store.AddCandidateAsync(CreateCandidate(job.Id, 11, "DIDOX", "document-1")));
    }

    [Fact]
    public async Task RejectsDuplicateCandidateLineNumber()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var candidate = await SeedCandidateAsync(store, "EDOCS");
        await store.AddLineAsync(11, CreateLine(candidate.Id, 1));

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            store.AddLineAsync(11, CreateLine(candidate.Id, 1)));
    }

    [Fact]
    public async Task RejectsDuplicateMarkingNumberWithinLine()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var candidate = await SeedCandidateAsync(store, "DIDOX");
        var line = CreateLine(candidate.Id, 1);
        await store.AddLineAsync(11, line);
        await store.AddMarkingAsync(11, CreateMarking(line.Id, "010-test-marking"));

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            store.AddMarkingAsync(11, CreateMarking(line.Id, "010-test-marking")));
    }

    [Fact]
    public async Task StoreQueriesAreOrganizationScoped()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var candidate = await SeedCandidateAsync(store, "EDOCS");

        Assert.NotNull(await store.GetCandidateAsync(11, candidate.Id));
        Assert.Null(await store.GetCandidateAsync(12, candidate.Id));
        Assert.Null(await store.GetJobAsync(12, candidate.JobId));
    }

    [Fact]
    public async Task CandidateOutsideJobOrganizationScopeIsRejected()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = CreateJob(11);
        await store.AddJobAsync(job);
        await store.AddProviderAsync(11, CreateProvider(job.Id, "EDOCS"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.AddCandidateAsync(CreateCandidate(job.Id, 12, "EDOCS", "document-1")));
    }

    [Fact]
    public void ModelContainsRequiredUniqueAndRecoveryIndexes()
    {
        using var context = CreateContext();

        AssertIndex(
            context,
            typeof(EdoImportJob),
            "ux_edo_import_job_active_organization",
            unique: true,
            expectedFilter: "status IN");
        AssertAlternateKey(context, typeof(EdoImportJobProvider), "ux_edo_import_job_provider_job_provider");
        AssertIndex(context, typeof(EdoImportJobProvider), "idx_edo_import_job_provider_status_retry", unique: false);
        AssertIndex(context, typeof(EdoImportCandidate), "ux_edo_import_candidate_job_provider_document", unique: true);
        AssertIndex(context, typeof(EdoImportCandidate), "idx_edo_import_candidate_header_fingerprint", unique: false);
        AssertIndex(context, typeof(EdoImportCandidate), "idx_edo_import_candidate_content_fingerprint", unique: false);
        AssertIndex(context, typeof(EdoImportCandidate), "ux_edo_import_candidate_imported_purchase", unique: true);
        AssertIndex(context, typeof(EdoImportCandidateLine), "ux_edo_import_candidate_line_candidate_number", unique: true);
        AssertIndex(context, typeof(EdoImportCandidateMarking), "ux_edo_import_candidate_marking_line_number", unique: true);
        AssertIndex(context, typeof(Contract), "ux_cmn_contract_provider_identity", unique: true,
            expectedFilter: "provider_code IS NOT NULL");

        var productName = context.Model.FindEntityType(typeof(EdoImportCandidateLine))!
            .FindProperty(nameof(EdoImportCandidateLine.ProviderProductName))!;
        Assert.True(productName.IsNullable);
        Assert.Equal(500, productName.GetMaxLength());
        var contract = context.Model.FindEntityType(typeof(Contract))!;
        Assert.True(contract.FindProperty(nameof(Contract.ProviderCode))!.IsNullable);
        Assert.True(contract.FindProperty(nameof(Contract.ProviderContractNumber))!.IsNullable);
        Assert.True(contract.FindProperty(nameof(Contract.ProviderContractDate))!.IsNullable);
    }

    [Fact]
    public void ExistingEdoDocumentIdentityAndPurchaseLinksRemainBackwardCompatible()
    {
        using var context = CreateContext();
        var edoDocument = context.Model.FindEntityType(typeof(EdoDocument))!;
        var providerIdentities = edoDocument.GetIndexes().Where(index =>
            index.GetDatabaseName() == "ux_edo_document_organization_provider_document").ToArray();
        var candidate = context.Model.FindEntityType(typeof(EdoImportCandidate))!;

        Assert.NotEmpty(providerIdentities);
        Assert.All(providerIdentities, providerIdentity =>
        {
            Assert.True(providerIdentity.IsUnique);
            Assert.Equal(
                [nameof(EdoDocument.OrganizationId), nameof(EdoDocument.Provider), nameof(EdoDocument.ProviderDocumentId)],
                providerIdentity.Properties.Select(property => property.Name));
        });
        Assert.True(candidate.FindProperty(nameof(EdoImportCandidate.ExistingPurchaseId))!.IsNullable);
        Assert.True(candidate.FindProperty(nameof(EdoImportCandidate.ImportedPurchaseId))!.IsNullable);
        Assert.Null(typeof(EdoDocument).GetProperty("HeaderFingerprint"));
        Assert.Null(typeof(EdoDocument).GetProperty("ContentFingerprint"));
        Assert.Null(typeof(EdoDocument).GetProperty("SharedDocumentIdentity"));
    }

    private static async Task<EdoImportCandidate> SeedCandidateAsync(EdoImportStore store, string providerCode)
    {
        var job = CreateJob(11);
        await store.AddJobAsync(job);
        await store.AddProviderAsync(11, CreateProvider(job.Id, providerCode));
        var candidate = CreateCandidate(job.Id, 11, providerCode, "document-1");
        await store.AddCandidateAsync(candidate);
        return candidate;
    }

    private static EdoImportJob CreateJob(int organizationId) =>
        new(organizationId, 7, new DateOnly(2026, 1, 1), new DateOnly(2026, 8, 11), Now);

    private static EdoImportJobProvider CreateProvider(long jobId, string providerCode) => new()
    {
        JobId = jobId,
        ProviderCode = providerCode,
        Status = EdoImportProviderCheckpointStatus.Queued,
        CurrentPage = 1,
        PageSize = 20,
        CreatedDate = Now
    };

    private static EdoImportCandidate CreateCandidate(
        long jobId,
        int organizationId,
        string providerCode,
        string providerDocumentId) =>
        new(jobId, organizationId, providerCode, providerDocumentId, Now);

    private static EdoImportCandidateLine CreateLine(long candidateId, int lineNumber) => new()
    {
        CandidateId = candidateId,
        ProviderLineNumber = lineNumber,
        MappingStatus = EdoImportMappingStatus.Unresolved,
        CreatedDate = Now
    };

    private static EdoImportCandidateMarking CreateMarking(long lineId, string markingNumber) => new()
    {
        CandidateLineId = lineId,
        MarkingNumber = markingNumber,
        ProviderVerificationState = EdoImportMarkingVerificationState.Unverified,
        CreatedDate = Now
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"edo-import-{Guid.NewGuid():N}")
            .Options;
        var context = new AppDbContext(options);
        context.SetUserContext(new SuperAdminUserContext());
        return context;
    }

    private static void AssertIndex(
        AppDbContext context,
        Type entityType,
        string databaseName,
        bool unique,
        string? expectedFilter = null)
    {
        var index = context.Model.FindEntityType(entityType)!
            .GetIndexes()
            .Single(candidate => candidate.GetDatabaseName() == databaseName);

        Assert.Equal(unique, index.IsUnique);
        if (expectedFilter is not null)
            Assert.Contains(expectedFilter, index.GetFilter(), StringComparison.Ordinal);
    }

    private static void AssertAlternateKey(AppDbContext context, Type entityType, string constraintName)
    {
        var key = context.Model.FindEntityType(entityType)!
            .GetKeys()
            .Single(candidate => candidate.GetName() == constraintName);

        Assert.False(key.IsPrimaryKey());
    }

    private sealed class SuperAdminUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.SuperAdmin;
        public short? LanguageId => 1;
        public int? TenantId => 1;
        public int? OrganizationId => 11;
        public List<int> AllowedOrganizationIds => [11, 12];
        public int? BranchId => null;
    }
}
