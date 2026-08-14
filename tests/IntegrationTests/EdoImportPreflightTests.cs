using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.PurchaseDocs;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Integration.Didox.Configs;
using Integration.Didox.Facturas;
using Integration.Didox.Historical;
using Integration.Didox.Http;
using Integration.Didox.Services;
using Integration.Edo.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Net;
using System.Text;
using WebApi.Controllers;

namespace IntegrationTests;

public sealed class EdoImportPreflightTests
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task Historical_numbering_accepts_old_provider_date_but_regular_numbering_rejects_it()
    {
        await using var context = CreateContext();
        context.Organizations.Add(new Organization
        {
            Id = 11,
            ShortName = "Test",
            FullName = "Test organization",
            Inn = "309142275",
            RegionId = 1,
            StateId = StateIdConst.ACTIVE,
            TenantId = 1,
            SetupStatus = "COMPLETED",
            CreatedDate = Now
        });
        context.DocumentTypes.Add(new DocumentType
        {
            Id = DocumentTypeIdConst.PURCHASE,
            Code = "PURCHASE",
            Name = "Purchase",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = Now
        });
        context.Set<DocumentNumberSequence>().AddRange(
            new DocumentNumberSequence
            {
                OrganizationId = 11,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                DocumentYear = 2010,
                LastNumber = 4,
                LastDocumentDate = new DateTime(2010, 12, 31),
                CreatedAt = Now,
                UpdatedAt = Now
            },
            new DocumentNumberSequence
            {
                OrganizationId = 11,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                DocumentYear = 2026,
                LastNumber = 100,
                LastDocumentDate = new DateTime(2026, 8, 12),
                CreatedAt = Now,
                UpdatedAt = Now
            });
        await context.SaveChangesAsync();
        var service = new DocumentNumberService(
            new TestUserContext(),
            new QueryBuilder(null!),
            new NoOpDocumentPostingLock(),
            new QueryRepository<Organization>(context),
            new QueryRepository<DocumentType>(context),
            new QueryRepository<DocumentNumberSequence>(context),
            new CommandRepository<DocumentNumberSequence>(context));
        var providerDate = new DateTime(2010, 1, 15);

        var regular = await service.GetNextAsync(
            11, DocumentTypeIdConst.PURCHASE, providerDate);
        var historical = await service.GetNextHistoricalAsync(
            11, DocumentTypeIdConst.PURCHASE, providerDate);

        Assert.False(regular.IsSuccess);
        Assert.Equal("DocumentNumber.EarlierDocumentDate", regular.Error.Code);
        Assert.True(historical.IsSuccess);
        Assert.Equal("H-2010-5", historical.Value.DocumentNumber);
        Assert.Equal(providerDate, historical.Value.DocumentDate);
        Assert.Equal(new DateTime(2010, 12, 31), context.Set<DocumentNumberSequence>()
            .Single(sequence => sequence.DocumentYear == 2010).LastDocumentDate);
    }

    [Fact]
    public async Task Historical_numbering_uses_only_active_tenant_organization_from_background_scope()
    {
        var backgroundScope = new BackgroundOrganizationScope();
        await using var context = CreateContext(backgroundScope, setUserContext: false);
        context.Organizations.AddRange(
            new Organization
            {
                Id = 11, ShortName = "Scoped", FullName = "Scoped organization", Inn = "309142275",
                RegionId = 1, StateId = StateIdConst.ACTIVE, TenantId = 1,
                SetupStatus = "COMPLETED", CreatedDate = Now
            },
            new Organization
            {
                Id = 22, ShortName = "Inactive", FullName = "Inactive organization", Inn = "309142276",
                RegionId = 1, StateId = StateIdConst.PASSIVE, TenantId = 1,
                SetupStatus = "COMPLETED", CreatedDate = Now
            });
        context.DocumentTypes.Add(new DocumentType
        {
            Id = DocumentTypeIdConst.PURCHASE,
            Code = "PURCHASE",
            Name = "Purchase",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = Now
        });
        await context.SaveChangesAsync();

        var userContext = new UserContext(
            new Microsoft.AspNetCore.Http.HttpContextAccessor(), backgroundScope);
        var service = new DocumentNumberService(
            userContext,
            new QueryBuilder(null!),
            new NoOpDocumentPostingLock(),
            new QueryRepository<Organization>(context),
            new QueryRepository<DocumentType>(context),
            new QueryRepository<DocumentNumberSequence>(context),
            new CommandRepository<DocumentNumberSequence>(context));

        using (backgroundScope.Enter(11, "TEST"))
        {
            var scoped = await service.GetNextHistoricalAsync(
                11, DocumentTypeIdConst.PURCHASE, new DateTime(2010, 1, 15));
            var crossOrganization = await service.GetNextHistoricalAsync(
                22, DocumentTypeIdConst.PURCHASE, new DateTime(2010, 1, 15));

            Assert.True(scoped.IsSuccess);
            Assert.Equal("H-2010-1", scoped.Value.DocumentNumber);
            Assert.False(crossOrganization.IsSuccess);
            Assert.Equal("DocumentNumber.InvalidOrganization", crossOrganization.Error.Code);
        }

        using (backgroundScope.Enter(22, "TEST"))
        {
            var inactive = await service.GetNextHistoricalAsync(
                22, DocumentTypeIdConst.PURCHASE, new DateTime(2010, 1, 15));

            Assert.False(inactive.IsSuccess);
            Assert.Equal("DocumentNumber.InvalidOrganization", inactive.Error.Code);
        }
        Assert.False(backgroundScope.IsActive);
    }

    [Fact]
    public async Task Historical_numbering_reaches_background_scope_through_real_dependency_injection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor,
            Microsoft.AspNetCore.Http.HttpContextAccessor>();
        services.AddSingleton<IBackgroundOrganizationScope, BackgroundOrganizationScope>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase($"edo-numbering-di-{Guid.NewGuid():N}"));
        services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
        services.AddScoped<IQueryBuilder>(_ => new QueryBuilder(null!));
        services.AddScoped<IDocumentPostingLock, NoOpDocumentPostingLock>();
        services.AddScoped<IDocumentNumberService, DocumentNumberService>();

        await using var provider = services.BuildServiceProvider();
        await using var serviceScope = provider.CreateAsyncScope();
        var scopedProvider = serviceScope.ServiceProvider;
        var context = scopedProvider.GetRequiredService<AppDbContext>();
        context.Organizations.Add(new Organization
        {
            Id = 11, ShortName = "DI", FullName = "DI organization", Inn = "309142275",
            RegionId = 1, StateId = StateIdConst.ACTIVE, TenantId = 1,
            SetupStatus = "COMPLETED", CreatedDate = Now
        });
        context.DocumentTypes.Add(new DocumentType
        {
            Id = DocumentTypeIdConst.PURCHASE,
            Code = "PURCHASE",
            Name = "Purchase",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = Now
        });
        await context.SaveChangesAsync();

        var backgroundScope = scopedProvider.GetRequiredService<IBackgroundOrganizationScope>();
        var numbering = scopedProvider.GetRequiredService<IDocumentNumberService>();
        using (backgroundScope.Enter(11, "TEST"))
        {
            var result = await numbering.GetNextHistoricalAsync(
                11, DocumentTypeIdConst.PURCHASE, new DateTime(2010, 1, 15));

            Assert.True(result.IsSuccess);
            Assert.Equal("H-2010-1", result.Value.DocumentNumber);
        }
        Assert.False(backgroundScope.IsActive);
    }

    [Fact]
    public void Controller_exposes_only_the_phase_3a_preflight_routes()
    {
        var controllerRoute = Assert.Single(typeof(EdoImportController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), false)
            .Cast<Microsoft.AspNetCore.Mvc.RouteAttribute>());
        Assert.Equal("api/purchase-docs/edo-imports", controllerRoute.Template);

        var routes = typeof(EdoImportController).GetMethods()
            .SelectMany(method => method.GetCustomAttributes(false)
                .OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
                .Select(attribute => $"{attribute.HttpMethods.Single()} {attribute.Template}"))
            .Order()
            .ToArray();

        Assert.Equal([
            "GET {jobId:long}",
            "GET {jobId:long}/bulk-import/status",
            "GET {jobId:long}/documents",
            "GET {jobId:long}/documents/{candidateId:long}",
            "GET {jobId:long}/draft-import-failures",
            "GET {jobId:long}/import-plan",
            "GET {jobId:long}/mapping-summary",
            "GET {jobId:long}/marking-conflicts",
            "GET {jobId:long}/master-data-plan",
            "GET {jobId:long}/piece-tracking-plan",
            "GET {jobId:long}/product-conflicts",
            "POST {jobId:long}/bulk-import/cancel",
            "POST {jobId:long}/bulk-import/start",
            "POST {jobId:long}/cancel",
            "POST {jobId:long}/documents/{candidateId:long}/requeue-draft-import",
            "POST {jobId:long}/draft-import-failures/apply",
            "POST {jobId:long}/import-drafts",
            "POST {jobId:long}/marking-conflicts/apply",
            "POST {jobId:long}/master-data/apply",
            "POST {jobId:long}/master-data/products/apply-defaults",
            "POST {jobId:long}/piece-tracking/apply",
            "POST {jobId:long}/product-conflicts/apply",
            "POST {jobId:long}/resolve-mappings",
            "POST preflight",
            "PUT {jobId:long}/documents/{candidateId:long}/mapping"
        ], routes);
    }

    [Theory]
    [InlineData(EdoProviderCode.EDOCS)]
    [InlineData(EdoProviderCode.DIDOX)]
    public async Task Start_uses_accounting_start_date_snapshots_only_active_provider_and_schedules_quartz(
        EdoProviderCode activeProvider)
    {
        await using var context = CreateContext();
        context.OrganizationConfigs.Add(new OrganizationConfig
        {
            OrganizationId = 11,
            InventoryValuationMethod = "FIFO",
            FiscalYearStartMonth = 1,
            AccountingStartDate = new DateOnly(2026, 1, 1)
        });
        await context.SaveChangesAsync();
        var scheduler = new RecordingScheduler();
        var service = CreateService(context, scheduler, activeProvider);

        var result = await service.StartAsync(new EdoImportPreflightRequestDto
        {
            DateTo = new DateOnly(2026, 8, 12)
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 1, 1), result.Value.DateFrom);
        Assert.Equal(activeProvider.ToString(), Assert.Single(result.Value.Providers).ProviderCode);
        Assert.Equal(result.Value.Id, Assert.Single(scheduler.JobIds));
    }

    [Fact]
    public async Task Start_request_does_not_accept_provider_and_job_keeps_active_provider_snapshot()
    {
        Assert.Null(typeof(EdoImportPreflightRequestDto).GetProperty("ProviderCode"));
        await using var context = CreateContext();
        context.OrganizationConfigs.Add(new OrganizationConfig
        {
            OrganizationId = 11,
            InventoryValuationMethod = "FIFO",
            FiscalYearStartMonth = 1,
            AccountingStartDate = new DateOnly(2026, 1, 1)
        });
        await context.SaveChangesAsync();
        var resolver = new MutableActiveProviderResolver(EdoProviderCode.EDOCS);
        var service = CreateService(context, new RecordingScheduler(), resolver);

        var result = await service.StartAsync(new EdoImportPreflightRequestDto
        {
            DateTo = new DateOnly(2026, 8, 12)
        });
        resolver.ProviderCode = EdoProviderCode.DIDOX;

        Assert.True(result.IsSuccess);
        Assert.Equal("EDOCS", Assert.Single(result.Value.Providers).ProviderCode);
        Assert.Equal("EDOCS", Assert.Single(context.EdoImportJobProviders).ProviderCode);
    }

    [Fact]
    public async Task Missing_accounting_start_date_is_controlled_business_error_and_writes_no_job()
    {
        await using var context = CreateContext();
        var service = CreateService(context, new RecordingScheduler());

        var result = await service.StartAsync(new EdoImportPreflightRequestDto
        {
            DateTo = new DateOnly(2026, 8, 12)
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("EdoImport.AccountingStartDateRequired", result.Error.Code);
        Assert.Empty(context.EdoImportJobs);
    }

    [Fact]
    public async Task Second_active_job_for_same_organization_is_rejected_before_schedule()
    {
        await using var context = CreateContext();
        context.OrganizationConfigs.Add(new OrganizationConfig
        {
            OrganizationId = 11,
            InventoryValuationMethod = "FIFO",
            FiscalYearStartMonth = 1,
            AccountingStartDate = new DateOnly(2026, 1, 1)
        });
        await context.SaveChangesAsync();
        var scheduler = new RecordingScheduler();
        var service = CreateService(context, scheduler);
        var request = new EdoImportPreflightRequestDto { DateTo = new DateOnly(2026, 8, 12) };

        Assert.True((await service.StartAsync(request)).IsSuccess);
        var second = await service.StartAsync(request);

        Assert.False(second.IsSuccess);
        Assert.Equal("EdoImport.ActiveJobExists", second.Error.Code);
        Assert.Single(scheduler.JobIds);
    }

    [Fact]
    public async Task Processor_persists_ready_candidate_lines_markings_checkpoint_and_no_purchase()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var detail = ValidDetail("E-1", "MARK-1", "MARK-2");
        var processor = CreateProcessor(store,
            new StubHistoricalSource(EdoProviderCode.DIDOX, EmptyPage(EdoProviderCode.DIDOX)),
            new StubHistoricalSource(EdoProviderCode.EDOCS, OnePage(EdoProviderCode.EDOCS, "E-1"), detail));

        await processor.ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates
            .Include(item => item.Lines).ThenInclude(line => line.Markings)
            .SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Equal(EdoImportMappingStatus.Resolved, candidate.MappingStatus);
        Assert.Equal(2, Assert.Single(candidate.Lines).Markings.Count);
        Assert.Equal(1, job.DiscoveredCount);
        Assert.Equal(1, job.ReadyCount);
        Assert.Equal(EdoImportJobStatus.PreflightReady, job.Status);
        Assert.Null(job.LeaseOwner);
        Assert.Empty(context.PurchaseDocs);
        Assert.Empty(context.AccountingRegisterEntries);
        Assert.Empty(context.WarehouseProductMovements);
    }

    [Fact]
    public async Task Didox_one_day_preflight_flows_from_list_to_encoded_detail_to_candidate()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var day = new DateOnly(2026, 7, 31);
        var job = new EdoImportJob(11, 7, day, day, Now);
        await store.AddJobAsync(job);
        await store.AddProviderAsync(11, Provider(job.Id, "DIDOX"));

        var httpFactory = new TestHttpClientFactory(request =>
        {
            return request.RequestUri!.AbsolutePath.EndsWith("/v2/documents", StringComparison.Ordinal)
                ? JsonResponse("""
                    { "data": [{ "doc_id": "D-INTEGRATION", "doc_status": "3", "doctype": "002" }],
                      "total": 1, "page": 1, "limit": 100, "next_page_url": null }
                    """)
                : JsonResponse("""
                    { "data": { "document_json": "{\"doc_id\":\"D-INTEGRATION\",\"doc_status\":3,\"doctype\":\"002\",\"documentNumber\":\"42\",\"documentDate\":\"2026-07-31\",\"Seller\":{\"Name\":\"Supplier\"},\"SellerTin\":\"300000001\",\"Buyer\":{\"Name\":\"Buyer\"},\"BuyerTin\":\"309142275\",\"productlist\":{\"products\":[{\"ordno\":1,\"catalogcode\":\"123\",\"catalogname\":\"Product\",\"count\":1,\"summa\":100,\"deliverysumwithvat\":100}]}}" } }
                    """);
        });
        var tokenRoot = Path.Combine(Path.GetTempPath(), $"didox-preflight-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tokenRoot);
        try
        {
            var operations = new DidoxEdoOperations(
                new TestUserContext(),
                httpFactory,
                new DidoxTimestampClient(httpFactory));
            var tokenCache = new DidoxTokenCache(
                new EphemeralDataProtectionProvider(),
                Options.Create(new DidoxTokenStorageOptions { RootPath = "didox" }),
                new TestHostEnvironment(tokenRoot));
            tokenCache.Set(11, "test-session", TimeSpan.FromMinutes(5));
            var source = new DidoxHistoricalDocumentSource(
                operations,
                tokenCache,
                new TestOrganizationSourceReader(new Dictionary<int, string> { [11] = "309142275" }));

            await CreateProcessor(store, source).ProcessAsync(job.Id, "test-worker");
        }
        finally
        {
            Directory.Delete(tokenRoot, recursive: true);
        }

        var candidate = await context.EdoImportCandidates.Include(item => item.Lines).SingleAsync();
        Assert.Equal("D-INTEGRATION", candidate.ProviderDocumentId);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Single(candidate.Lines);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Processor_persists_normalized_historical_seller_tin_on_candidate()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var processor = CreateProcessor(store,
            new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-SELLER-TIN"),
                ValidDetailWithSellerTin("E-SELLER-TIN", "207164728")));

        await processor.ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal("207164728", candidate.SellerTin);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Processor_persists_unicode_provider_product_name_snapshot_and_returns_it_in_detail()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var detail = ValidDetailWithProductName("E-PRODUCT-NAME", "Кирпич силикатный");
        var processor = CreateProcessor(store,
            new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-PRODUCT-NAME"),
                detail));

        await processor.ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates
            .Include(item => item.Lines)
            .SingleAsync();
        var line = Assert.Single(candidate.Lines);
        Assert.Equal("Кирпич силикатный", line.ProviderProductName);
        Assert.NotEqual(line.PackageName, line.ProviderProductName);

        var response = await CreateService(context, new RecordingScheduler())
            .GetCandidateAsync(job.Id, candidate.Id);
        Assert.True(response.IsSuccess);
        Assert.Equal("Кирпич силикатный", Assert.Single(response.Value.Lines).ProviderProductName);
    }

    [Fact]
    public async Task Missing_or_used_marking_makes_candidate_mapping_required()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        context.ProductTables.Add(new ProductTable
        {
            Id = 901,
            ProductId = 300,
            MarkingNumber = "MARK-USED",
            CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var processor = CreateProcessor(store,
            new StubHistoricalSource(EdoProviderCode.DIDOX, EmptyPage(EdoProviderCode.DIDOX)),
            new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-2"),
                ValidDetail("E-2", "MARK-USED", "MARK-NEW")));

        await processor.ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal(EdoImportMarkingPolicy.AlreadyUsed, candidate.SafeErrorCode);
        Assert.Empty(context.PurchaseDocs);
    }

    [Theory]
    [InlineData("2", "", EdoImportMarkingPolicy.ProviderDataRequired)]
    [InlineData("2", "DUP|DUP", EdoImportMarkingPolicy.Duplicate)]
    [InlineData("1.5", "MARK-1", EdoImportMarkingPolicy.QuantityInvalid)]
    public async Task Processor_returns_specific_structural_marking_failure(
        string quantity,
        string markingsValue,
        string expectedCode)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var markings = markingsValue.Length == 0 ? [] : markingsValue.Split('|');
        var detail = DetailWithMarkingLine(
            "E-MARKING-STRUCTURE",
            decimal.Parse(quantity, System.Globalization.CultureInfo.InvariantCulture),
            isService: false,
            markings);

        await CreateProcessor(store, new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, detail.ProviderDocumentId),
                detail))
            .ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal(expectedCode, candidate.SafeErrorCode);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task All_markings_owned_by_one_purchase_confirm_duplicate()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        SeedPurchaseMarkingOwner(context, 182, 801, [(901, "MARK-A"), (902, "MARK-B")], 11, 300);
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        await CreateProcessor(store, new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-MARKING-DUPLICATE"),
                ValidDetail("E-MARKING-DUPLICATE", "MARK-A", "MARK-B")))
            .ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.Duplicate, candidate.Status);
        Assert.Equal(EdoImportDuplicateState.Confirmed, candidate.DuplicateState);
        Assert.Equal(182, candidate.ExistingPurchaseId);
        Assert.Equal(1, job.DuplicateCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Partial_or_multiple_purchase_marking_usage_remains_controlled_conflict(
        bool multiplePurchases)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        SeedPurchaseMarkingOwner(context, 182, 801, [(901, "MARK-A")], 11, 300);
        if (multiplePurchases)
            SeedPurchaseMarkingOwner(context, 183, 802, [(902, "MARK-B")], 11, 300);
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        await CreateProcessor(store, new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-MARKING-CONFLICT"),
                ValidDetail("E-MARKING-CONFLICT", "MARK-A", "MARK-B")))
            .ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal(EdoImportMarkingPolicy.AlreadyUsed, candidate.SafeErrorCode);
        Assert.Null(candidate.ExistingPurchaseId);
        Assert.NotEqual(EdoImportDuplicateState.Confirmed, candidate.DuplicateState);
    }

    [Fact]
    public async Task Marking_conflict_plan_is_scoped_and_explicit_skip_is_stale_safe_and_idempotent()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        SeedPurchaseMarkingOwner(context, 182, 801, [(901, "MARK-A")], 11, 300);
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        await CreateProcessor(store, new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-MARKING-SKIP"),
                ValidDetail("E-MARKING-SKIP", "MARK-A", "MARK-B")))
            .ProcessAsync(job.Id, "test-worker");
        var candidate = await context.EdoImportCandidates.SingleAsync();
        var purchaseCount = await context.PurchaseDocs.CountAsync();
        var productCount = await context.Products.CountAsync();
        var markingCount = await context.ProductTables.CountAsync();

        var service = CreateService(context, new RecordingScheduler());
        var planResult = await service.GetMarkingConflictsAsync(job.Id);

        Assert.True(planResult.IsSuccess);
        Assert.Equal(64, planResult.Value.ConflictHash.Length);
        var item = Assert.Single(planResult.Value.Items);
        Assert.Equal(candidate.Id, item.CandidateId);
        Assert.Equal("42", item.DocumentNumber);
        Assert.Equal(new DateOnly(2026, 7, 31), item.DocumentDate);
        Assert.Equal(EdoImportMarkingPolicy.AlreadyUsed, item.SafeErrorCode);
        Assert.Equal(2m, item.ExpectedQuantity);
        Assert.Equal(2, item.ActualMarkingCount);
        Assert.Equal(1, item.ConflictCount);
        Assert.Equal([182L], item.ExistingPurchaseIds);
        Assert.DoesNotContain(typeof(EdoImportMarkingConflictItemDto).GetProperties(), property =>
            property.Name.Contains("MarkingCode", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(purchaseCount, await context.PurchaseDocs.CountAsync());
        Assert.Equal(markingCount, await context.ProductTables.CountAsync());

        var foreignService = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new ScopedTestUserContext(22));
        var foreignPlan = await foreignService.GetMarkingConflictsAsync(job.Id);
        Assert.False(foreignPlan.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", foreignPlan.Error.Code);

        var request = new EdoImportMarkingConflictApplyRequestDto
        {
            Confirm = true,
            ExpectedConflictHash = planResult.Value.ConflictHash,
            Items = [new EdoImportMarkingConflictApplyItemDto
            {
                CandidateId = candidate.Id,
                Action = "SKIP"
            }]
        };
        var unconfirmed = await service.ApplyMarkingConflictsAsync(job.Id,
            new EdoImportMarkingConflictApplyRequestDto
            {
                Confirm = false,
                ExpectedConflictHash = request.ExpectedConflictHash,
                Items = request.Items
            });
        Assert.False(unconfirmed.IsSuccess);
        Assert.Equal("MARKING_CONFLICT_CONFIRMATION_REQUIRED", unconfirmed.Error.Code);
        var stale = await service.ApplyMarkingConflictsAsync(job.Id, new EdoImportMarkingConflictApplyRequestDto
        {
            Confirm = true,
            ExpectedConflictHash = new string('0', 64),
            Items = request.Items
        });
        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_MARKING_CONFLICT_PLAN", stale.Error.Code);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);

        var applied = await service.ApplyMarkingConflictsAsync(job.Id, request);
        Assert.True(applied.IsSuccess);
        Assert.Equal(1, applied.Value.SkippedCandidateCount);
        Assert.Equal(0, applied.Value.MappingRequiredCount);
        Assert.Equal(1, applied.Value.SkippedCount);
        Assert.Equal(EdoImportCandidateStatus.Skipped, candidate.Status);
        Assert.Equal(EdoImportMarkingPolicy.AlreadyUsedSkipped, candidate.SafeErrorCode);

        var retry = await service.ApplyMarkingConflictsAsync(job.Id, request);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.SkippedCandidateCount);
        Assert.Equal(1, retry.Value.SkippedCount);
        Assert.Equal(purchaseCount, await context.PurchaseDocs.CountAsync());
        Assert.Equal(productCount, await context.Products.CountAsync());
        Assert.Equal(markingCount, await context.ProductTables.CountAsync());
    }

    [Fact]
    public async Task Structural_marking_conflicts_are_visible_and_explicit_skip_preserves_snapshot()
    {
        var cases = new[]
        {
            new { Code = EdoImportMarkingPolicy.CountMismatch, Quantity = 2m, MarkingCount = 1 },
            new { Code = EdoImportMarkingPolicy.ProviderDataRequired, Quantity = 2m, MarkingCount = 0 },
            new { Code = EdoImportMarkingPolicy.Duplicate, Quantity = 2m, MarkingCount = 2 },
            new { Code = EdoImportMarkingPolicy.QuantityInvalid, Quantity = 1.5m, MarkingCount = 1 }
        };

        foreach (var testCase in cases)
        {
            await using var context = CreateContext();
            var store = new EdoImportStore(context);
            var markings = Enumerable.Range(1, testCase.MarkingCount)
                .Select(index => $"SAFE-MARK-{index}")
                .ToArray();
            var (job, candidate) = await SeedMappingCandidateAsync(
                store,
                quantity: testCase.Quantity,
                markings: markings);
            candidate.SafeErrorCode = testCase.Code;
            await store.SaveChangesAsync();
            var originalQuantity = Assert.Single(candidate.Lines).Quantity;
            var originalMarkingCount = Assert.Single(candidate.Lines).Markings.Count;
            var service = CreateService(context, new RecordingScheduler());

            var plan = await service.GetMarkingConflictsAsync(job.Id);

            Assert.True(plan.IsSuccess);
            var item = Assert.Single(plan.Value.Items);
            Assert.Equal(testCase.Code, item.SafeErrorCode);
            Assert.Equal(testCase.Quantity, item.ExpectedQuantity);
            Assert.Equal(testCase.MarkingCount, item.ActualMarkingCount);
            Assert.Equal(0, item.ConflictCount);
            Assert.Empty(item.ExistingPurchaseIds);

            var applied = await service.ApplyMarkingConflictsAsync(job.Id,
                new EdoImportMarkingConflictApplyRequestDto
                {
                    Confirm = true,
                    ExpectedConflictHash = plan.Value.ConflictHash,
                    Items = [new EdoImportMarkingConflictApplyItemDto
                    {
                        CandidateId = candidate.Id,
                        Action = "SKIP"
                    }]
                });

            Assert.True(applied.IsSuccess);
            Assert.Equal(EdoImportCandidateStatus.Skipped, candidate.Status);
            Assert.Equal(testCase.Code + "_SKIPPED", candidate.SafeErrorCode);
            Assert.Equal(originalQuantity, Assert.Single(candidate.Lines).Quantity);
            Assert.Equal(originalMarkingCount, Assert.Single(candidate.Lines).Markings.Count);
        }
    }

    [Fact]
    public async Task Marking_conflict_apply_rejects_non_supported_candidate()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store);
        var service = CreateService(context, new RecordingScheduler());
        var plan = await service.GetMarkingConflictsAsync(job.Id);

        var result = await service.ApplyMarkingConflictsAsync(job.Id,
            new EdoImportMarkingConflictApplyRequestDto
            {
                Confirm = true,
                ExpectedConflictHash = plan.Value.ConflictHash,
                Items = [new EdoImportMarkingConflictApplyItemDto
                {
                    CandidateId = candidate.Id,
                    Action = "SKIP"
                }]
            });

        Assert.False(result.IsSuccess);
        Assert.Equal("MARKING_CONFLICT_SELECTION_INVALID", result.Error.Code);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
    }

    [Fact]
    public async Task Import_plan_is_scoped_read_only_and_batches_ready_candidates_in_id_order()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var first = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-2", "MARK-DRAFT-2", totalAmount: 112m);
        var second = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-1", "MARK-DRAFT-1", totalAmount: 112m);
        var factory = new RecordingHistoricalDraftFactory(context);
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);

        var plan = await service.GetImportPlanAsync(job.Id);

        Assert.True(plan.IsSuccess);
        Assert.Equal(2, plan.Value.ReadyCount);
        Assert.Equal(224m, plan.Value.ReadyTotalAmount);
        Assert.Equal(2, plan.Value.MarkedCandidateCount);
        Assert.Equal(64, plan.Value.ImportPlanHash.Length);
        Assert.Empty(context.PurchaseDocs);
        var foreign = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new ScopedTestUserContext(22));
        var foreignPlan = await foreign.GetImportPlanAsync(job.Id);
        Assert.False(foreignPlan.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", foreignPlan.Error.Code);

        var stale = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = new string('0', 64),
            BatchSize = 1
        });
        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_IMPORT_PLAN", stale.Error.Code);
        Assert.Empty(context.PurchaseDocs);

        var firstBatch = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 1
        });
        Assert.True(firstBatch.IsSuccess);
        Assert.True(firstBatch.Value.CreatedDraftCount == 1,
            string.Join(",", firstBatch.Value.Failures.Select(failure => failure.SafeErrorCode)));
        Assert.Equal(1, firstBatch.Value.RemainingReadyCount);
        Assert.Equal(EdoImportJobStatus.Partial, firstBatch.Value.JobStatus);
        Assert.Equal(first.Id, Assert.Single(factory.CandidateOrder));
        var importedFirst = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(candidate => candidate.Id == first.Id);
        Assert.Equal(EdoImportCandidateStatus.Imported, importedFirst.Status);
        Assert.Equal(importedFirst.ImportedPurchaseId, importedFirst.ExistingPurchaseId);
        Assert.NotNull(importedFirst.ImportedPurchaseId);
        var firstPurchase = await context.PurchaseDocs.IgnoreQueryFilters()
            .SingleAsync(purchase => purchase.Id == importedFirst.ImportedPurchaseId);
        Assert.Equal(DocumentStatusIdConst.DRAFT, firstPurchase.StatusId);

        var nextPlan = await service.GetImportPlanAsync(job.Id);
        Assert.NotEqual(plan.Value.ImportPlanHash, nextPlan.Value.ImportPlanHash);
        var secondBatch = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = nextPlan.Value.ImportPlanHash,
            BatchSize = 50
        });
        Assert.True(secondBatch.IsSuccess);
        Assert.Equal(1, secondBatch.Value.CreatedDraftCount);
        Assert.Equal(0, secondBatch.Value.RemainingReadyCount);
        Assert.Equal(EdoImportJobStatus.Completed, secondBatch.Value.JobStatus);
        Assert.Equal([first.Id, second.Id], factory.CandidateOrder);
        Assert.Equal(2, await context.PurchaseDocs.CountAsync());
        Assert.Equal(2, await context.ProductTables.CountAsync(table =>
            table.MarkingNumber != null && table.MarkingNumber.StartsWith("MARK-DRAFT-")));
        Assert.Empty(context.AccountingRegisterEntries);
        Assert.Empty(context.WarehouseProductMovements);
    }

    [Fact]
    public async Task Mapping_resolution_replaces_stale_marking_mismatch_before_ready_draft_import()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(
            store,
            quantity: 2,
            markings: ["MARK-STATE-A", "MARK-STATE-B"]);
        foreach (var marking in candidate.Lines.SelectMany(line => line.Markings))
            marking.ProviderVerificationState = EdoImportMarkingVerificationState.Mismatch;
        await store.SaveChangesAsync();

        var factory = new RecordingHistoricalDraftFactory(context);
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);

        var resolved = await service.ResolveMappingsAsync(job.Id);
        Assert.True(resolved.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.All(candidate.Lines.SelectMany(line => line.Markings), marking =>
            Assert.Equal(EdoImportMarkingVerificationState.Verified, marking.ProviderVerificationState));

        var plan = await service.GetImportPlanAsync(job.Id);
        var imported = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 1
        });

        Assert.True(imported.IsSuccess);
        Assert.Equal(1, imported.Value.CreatedDraftCount);
        Assert.Equal(EdoImportCandidateStatus.Imported, candidate.Status);
        Assert.All(candidate.Lines.SelectMany(line => line.Markings), marking =>
            Assert.Equal(EdoImportMarkingVerificationState.Verified, marking.ProviderVerificationState));
        var purchase = await context.PurchaseDocs.SingleAsync(purchase =>
            purchase.Id == candidate.ImportedPurchaseId);
        Assert.Equal(DocumentStatusIdConst.DRAFT, purchase.StatusId);
        Assert.Equal(2, await context.ProductTables.CountAsync(table =>
            table.MarkingNumber != null && table.MarkingNumber.StartsWith("MARK-STATE")));
        Assert.Empty(context.AccountingRegisterEntries);
        Assert.Empty(context.WarehouseProductMovements);
    }

    [Fact]
    public async Task Draft_import_candidate_failure_is_controlled_and_does_not_rollback_next_candidate()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var failedCandidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-FAIL", marking: null, totalAmount: 112m);
        var validCandidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-VALID", marking: null, totalAmount: 112m);
        var factory = new RecordingHistoricalDraftFactory(
            context, failDocumentIdentity: "E-DRAFT-FAIL");
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);
        var plan = await service.GetImportPlanAsync(job.Id);

        var result = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 2
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CreatedDraftCount == 1,
            string.Join(",", result.Value.Failures.Select(failure => failure.SafeErrorCode)));
        Assert.Equal(1, result.Value.FailedCandidateCount);
        Assert.Equal(EdoImportJobStatus.Partial, result.Value.JobStatus);
        var failed = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(candidate => candidate.Id == failedCandidate.Id);
        var imported = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(candidate => candidate.Id == validCandidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Failed, failed.Status);
        Assert.Equal("DRAFT_IMPORT_TEST_VALIDATION_FAILURE", failed.SafeErrorCode);
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Single(context.PurchaseDocs);
    }

    [Fact]
    public async Task Failed_service_override_candidate_can_be_safely_requeued_once()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var product = context.Products.Single(item => item.Id == 300);
        product.IsService = true;
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-SERVICE-REQUEUE", "SERVICE-SNAPSHOT-MARK", totalAmount: 112m);
        var line = Assert.Single(candidate.Lines);
        line.ProviderProductName = "Provider service snapshot";
        line.PackageCode = "SERVICE-PACKAGE";
        var identityHash = EdoProviderProductIdentity.Create(
            candidate.ProviderCode,
            line.CatalogCode,
            line.PackageCode,
            line.ProviderProductName,
            line.IsService);
        Assert.NotNull(identityHash);
        context.EdoProviderProductMappings.Add(new EdoProviderProductMapping
        {
            OrganizationId = 11,
            ProviderCode = candidate.ProviderCode,
            CatalogCode = line.CatalogCode!,
            PackageCode = line.PackageCode,
            ProviderProductName = line.ProviderProductName,
            ProviderProductNameHash = EdoProviderProductIdentity.HashName(line.ProviderProductName),
            IdentityHash = identityHash,
            IsService = false,
            ProductId = product.Id,
            CreatedDate = Now
        });
        candidate.SafeErrorCode = "DRAFT_IMPORT_PURCHASEDOC_SERVICEITEMSNOTALLOWED";
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        job.ReadyCount = 0;
        job.FailedCount = 1;
        job.TransitionTo(EdoImportJobStatus.Importing, Now);
        job.TransitionTo(EdoImportJobStatus.Partial, Now);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var request = new EdoImportDraftRequeueRequestDto { Confirm = true };

        var first = await service.RequeueDraftCandidateAsync(job.Id, candidate.Id, request);
        var retry = await service.RequeueDraftCandidateAsync(job.Id, candidate.Id, request);

        Assert.True(first.IsSuccess);
        Assert.True(first.Value.Requeued);
        Assert.True(retry.IsSuccess);
        Assert.False(retry.Value.Requeued);
        var requeued = await context.EdoImportCandidates.IgnoreQueryFilters()
            .Include(item => item.Lines).ThenInclude(item => item.Markings)
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Ready, requeued.Status);
        Assert.Null(requeued.SafeErrorCode);
        Assert.Equal(1, job.ReadyCount);
        Assert.Equal(0, job.FailedCount);
        Assert.Single(Assert.Single(requeued.Lines).Markings);
        Assert.Empty(context.PurchaseDocs);
    }

    [Theory]
    [InlineData("DRAFT_IMPORT_DOCUMENTNUMBER_EARLIERDOCUMENTDATE")]
    [InlineData("DRAFT_IMPORT_PURCHASEFROMEDO_VALIDATIONFAILED")]
    public async Task Corrected_historical_validation_failure_is_listed_and_requeued_safely(
        string safeErrorCode)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, $"E-REQUEUE-{safeErrorCode}", marking: null, totalAmount: 112m);
        candidate.SafeErrorCode = safeErrorCode;
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        job.ReadyCount = 0;
        job.FailedCount = 1;
        job.TransitionTo(EdoImportJobStatus.Importing, Now);
        job.TransitionTo(EdoImportJobStatus.Partial, Now);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var failures = await service.GetDraftImportFailuresAsync(job.Id);
        var foreignFailures = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .GetDraftImportFailuresAsync(job.Id);
        var first = await service.RequeueDraftCandidateAsync(
            job.Id, candidate.Id, new EdoImportDraftRequeueRequestDto { Confirm = true });
        var retry = await service.RequeueDraftCandidateAsync(
            job.Id, candidate.Id, new EdoImportDraftRequeueRequestDto { Confirm = true });

        Assert.True(failures.IsSuccess);
        var item = Assert.Single(failures.Value.Items);
        Assert.Equal(candidate.Id, item.CandidateId);
        Assert.Equal(candidate.DocumentNumber, item.DocumentNumber);
        Assert.Equal(candidate.DocumentDate, item.DocumentDate);
        Assert.Equal(safeErrorCode, item.SafeErrorCode);
        Assert.DoesNotContain(typeof(EdoImportDraftFailureItemDto).GetProperties(), property =>
            property.Name.Contains("MarkingCode", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("MarkingNumber", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("ProviderDocument", StringComparison.OrdinalIgnoreCase));
        Assert.False(foreignFailures.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", foreignFailures.Error.Code);
        Assert.True(first.IsSuccess);
        Assert.True(first.Value.Requeued);
        Assert.True(retry.IsSuccess);
        Assert.False(retry.Value.Requeued);
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Null(candidate.SafeErrorCode);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Historical_generic_validation_error_gets_controlled_draft_failure_code()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-HISTORICAL-VALIDATION", marking: null, totalAmount: 112m);
        var factory = new RecordingHistoricalDraftFactory(
            context,
            failDocumentIdentity: candidate.ProviderDocumentId,
            failErrorCode: "PurchaseFromEdo.ValidationFailed");
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);
        var plan = await service.GetImportPlanAsync(job.Id);

        var result = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 1
        });

        Assert.True(result.IsSuccess);
        var failure = Assert.Single(result.Value.Failures);
        Assert.Equal("DRAFT_IMPORT_PURCHASEFROMEDO_VALIDATIONFAILED", failure.SafeErrorCode);
        var failedCandidate = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Failed, failedCandidate.Status);
        Assert.Equal(failure.SafeErrorCode, failedCandidate.SafeErrorCode);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Non_retryable_draft_failure_skip_is_scoped_stale_safe_and_idempotent()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var nonRetryable = await SeedReadyImportCandidateAsync(
            store, job, "E-NON-RETRYABLE", marking: null, totalAmount: 112m);
        var infrastructure = await SeedReadyImportCandidateAsync(
            store, job, "E-INFRASTRUCTURE", marking: null, totalAmount: 112m);
        var sourceLine = Assert.Single(nonRetryable.Lines);
        sourceLine.Quantity = 0;
        sourceLine.UnitPrice = 0;
        sourceLine.NetAmount = 100;
        sourceLine.TotalAmount = 112;
        nonRetryable.SafeErrorCode = EdoImportDraftFailurePolicy.LineValuesInvalid;
        infrastructure.SafeErrorCode = "DRAFT_IMPORT_PROCESSING_FAILURE";
        nonRetryable.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        infrastructure.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        job.ReadyCount = 0;
        job.FailedCount = 2;
        job.SkippedCount = 7;
        job.TransitionTo(EdoImportJobStatus.Importing, Now);
        job.TransitionTo(EdoImportJobStatus.Partial, Now);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var plan = await service.GetDraftImportFailuresAsync(job.Id);
        var repeatedPlan = await service.GetDraftImportFailuresAsync(job.Id);

        Assert.True(plan.IsSuccess);
        Assert.True(repeatedPlan.IsSuccess);
        Assert.Equal(64, plan.Value.FailureHash.Length);
        Assert.Equal(plan.Value.FailureHash, repeatedPlan.Value.FailureHash);
        Assert.Equal([nonRetryable.Id, infrastructure.Id],
            plan.Value.Items.Select(item => item.CandidateId).ToArray());
        Assert.DoesNotContain(typeof(EdoImportDraftFailureItemDto).GetProperties(), property =>
            property.Name.Contains("MarkingCode", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("MarkingNumber", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Quantity", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Amount", StringComparison.OrdinalIgnoreCase));

        var request = new EdoImportDraftFailureApplyRequestDto
        {
            Confirm = true,
            ExpectedFailureHash = plan.Value.FailureHash,
            Items = [new EdoImportDraftFailureApplyItemDto
            {
                CandidateId = nonRetryable.Id,
                Action = "SKIP"
            }]
        };
        var unconfirmed = await service.ApplyDraftImportFailuresAsync(job.Id,
            new EdoImportDraftFailureApplyRequestDto
            {
                Confirm = false,
                ExpectedFailureHash = request.ExpectedFailureHash,
                Items = request.Items
            });
        Assert.False(unconfirmed.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_FAILURE_CONFIRMATION_REQUIRED", unconfirmed.Error.Code);

        var stale = await service.ApplyDraftImportFailuresAsync(job.Id,
            new EdoImportDraftFailureApplyRequestDto
            {
                Confirm = true,
                ExpectedFailureHash = new string('0', 64),
                Items = request.Items
            });
        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_DRAFT_IMPORT_FAILURE_PLAN", stale.Error.Code);

        var infrastructureSkip = await service.ApplyDraftImportFailuresAsync(job.Id,
            new EdoImportDraftFailureApplyRequestDto
            {
                Confirm = true,
                ExpectedFailureHash = request.ExpectedFailureHash,
                Items = [new EdoImportDraftFailureApplyItemDto
                {
                    CandidateId = infrastructure.Id,
                    Action = "SKIP"
                }]
            });
        Assert.False(infrastructureSkip.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_FAILURE_SELECTION_INVALID", infrastructureSkip.Error.Code);

        var foreignApply = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .ApplyDraftImportFailuresAsync(job.Id, request);
        Assert.False(foreignApply.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", foreignApply.Error.Code);

        var requeue = await service.RequeueDraftCandidateAsync(
            job.Id,
            nonRetryable.Id,
            new EdoImportDraftRequeueRequestDto { Confirm = true });
        Assert.False(requeue.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_REQUEUE_NOT_ALLOWED", requeue.Error.Code);

        var originalQuantity = sourceLine.Quantity;
        var originalUnitPrice = sourceLine.UnitPrice;
        var originalNetAmount = sourceLine.NetAmount;
        var originalTotalAmount = sourceLine.TotalAmount;
        var originalMarkingCount = sourceLine.Markings.Count;
        var applied = await service.ApplyDraftImportFailuresAsync(job.Id, request);
        var retry = await service.ApplyDraftImportFailuresAsync(job.Id, request);
        var remainingPlan = await service.GetDraftImportFailuresAsync(job.Id);

        Assert.True(applied.IsSuccess);
        Assert.Equal(1, applied.Value.SkippedCandidateCount);
        Assert.Equal(1, applied.Value.FailedCount);
        Assert.Equal(8, applied.Value.SkippedCount);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.SkippedCandidateCount);
        Assert.NotEqual(plan.Value.FailureHash, remainingPlan.Value.FailureHash);
        Assert.Equal(infrastructure.Id, Assert.Single(remainingPlan.Value.Items).CandidateId);
        Assert.Equal(EdoImportCandidateStatus.Skipped, nonRetryable.Status);
        Assert.Equal(EdoImportDraftFailurePolicy.LineValuesInvalid + "_SKIPPED",
            nonRetryable.SafeErrorCode);
        Assert.Equal(EdoImportCandidateStatus.Failed, infrastructure.Status);
        Assert.Equal(originalQuantity, sourceLine.Quantity);
        Assert.Equal(originalUnitPrice, sourceLine.UnitPrice);
        Assert.Equal(originalNetAmount, sourceLine.NetAmount);
        Assert.Equal(originalTotalAmount, sourceLine.TotalAmount);
        Assert.Equal(originalMarkingCount, sourceLine.Markings.Count);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Purchase_linked_non_retryable_draft_failure_cannot_be_skipped()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-LINKED-NON-RETRYABLE", marking: null, totalAmount: 112m);
        candidate.SafeErrorCode = EdoImportDraftFailurePolicy.LineValuesInvalid;
        candidate.ExistingPurchaseId = 180;
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        job.ReadyCount = 0;
        job.FailedCount = 1;
        job.TransitionTo(EdoImportJobStatus.Importing, Now);
        job.TransitionTo(EdoImportJobStatus.Partial, Now);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = await service.GetDraftImportFailuresAsync(job.Id);

        var result = await service.ApplyDraftImportFailuresAsync(job.Id,
            new EdoImportDraftFailureApplyRequestDto
            {
                Confirm = true,
                ExpectedFailureHash = plan.Value.FailureHash,
                Items = [new EdoImportDraftFailureApplyItemDto
                {
                    CandidateId = candidate.Id,
                    Action = "SKIP"
                }]
            });

        Assert.False(result.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_FAILURE_SELECTION_INVALID", result.Error.Code);
        Assert.Equal(EdoImportCandidateStatus.Failed, candidate.Status);
        Assert.Equal(1, job.FailedCount);
        Assert.Equal(0, job.SkippedCount);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Draft_marking_failure_reconciliation_is_safe_scoped_and_idempotent()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var exact = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-MARKING-EXACT", "EXACT-A", totalAmount: 112m);
        AddCandidateMarking(exact, "EXACT-B");
        var partial = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-MARKING-PARTIAL", "PARTIAL-A", totalAmount: 112m);
        AddCandidateMarking(partial, "PARTIAL-B");
        var multiple = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-MARKING-MULTIPLE", "MULTIPLE-A", totalAmount: 112m);
        AddCandidateMarking(multiple, "MULTIPLE-B");
        SeedPurchaseMarkingOwner(
            context, 990, 890, [(990, "EXACT-A"), (991, "EXACT-B")], 11, 300);
        SeedPurchaseMarkingOwner(
            context, 991, 891, [(992, "PARTIAL-A")], 11, 300);
        SeedPurchaseMarkingOwner(
            context, 992, 892, [(993, "MULTIPLE-A")], 11, 300);
        SeedPurchaseMarkingOwner(
            context, 993, 893, [(994, "MULTIPLE-B")], 11, 300);
        exact.SafeErrorCode = EdoImportDraftFailurePolicy.MarkingAlreadyUsed;
        partial.SafeErrorCode = EdoImportDraftFailurePolicy.MarkingAlreadyUsed;
        multiple.SafeErrorCode = EdoImportDraftFailurePolicy.MarkingAlreadyUsed;
        exact.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        partial.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        multiple.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        job.ReadyCount = 0;
        job.FailedCount = 3;
        job.SkippedCount = 4;
        job.TransitionTo(EdoImportJobStatus.Importing, Now);
        job.TransitionTo(EdoImportJobStatus.Partial, Now);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var plan = await service.GetDraftImportFailuresAsync(job.Id);

        Assert.True(plan.IsSuccess);
        Assert.Equal(3, plan.Value.Items.Count);
        var exactPlan = plan.Value.Items.Single(item => item.CandidateId == exact.Id);
        Assert.Equal(EdoImportDraftFailurePolicy.MarkingAlreadyUsed, exactPlan.SafeErrorCode);
        Assert.Equal(2, exactPlan.TotalMarkingCount);
        Assert.Equal(2, exactPlan.UsedMarkingCount);
        Assert.Equal([990L], exactPlan.ExistingPurchaseIds);
        var partialPlan = plan.Value.Items.Single(item => item.CandidateId == partial.Id);
        Assert.Equal(2, partialPlan.TotalMarkingCount);
        Assert.Equal(1, partialPlan.UsedMarkingCount);
        Assert.Equal([991L], partialPlan.ExistingPurchaseIds);
        var multiplePlan = plan.Value.Items.Single(item => item.CandidateId == multiple.Id);
        Assert.Equal(2, multiplePlan.TotalMarkingCount);
        Assert.Equal(2, multiplePlan.UsedMarkingCount);
        Assert.Equal([992L, 993L], multiplePlan.ExistingPurchaseIds);
        Assert.DoesNotContain(typeof(EdoImportDraftFailureItemDto).GetProperties(), property =>
            property.Name.Contains("MarkingCode", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("MarkingNumber", StringComparison.OrdinalIgnoreCase));

        var exactSkip = await service.ApplyDraftImportFailuresAsync(job.Id,
            DraftFailureRequest(plan.Value.FailureHash, exact.Id, "SKIP"));
        Assert.False(exactSkip.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_FAILURE_SELECTION_INVALID", exactSkip.Error.Code);
        var partialDuplicate = await service.ApplyDraftImportFailuresAsync(job.Id,
            DraftFailureRequest(plan.Value.FailureHash, partial.Id, "MARK_DUPLICATE"));
        Assert.False(partialDuplicate.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_FAILURE_SELECTION_INVALID", partialDuplicate.Error.Code);
        var atomicFailure = await service.ApplyDraftImportFailuresAsync(job.Id,
            new EdoImportDraftFailureApplyRequestDto
            {
                Confirm = true,
                ExpectedFailureHash = plan.Value.FailureHash,
                Items =
                [
                    new EdoImportDraftFailureApplyItemDto
                    {
                        CandidateId = exact.Id,
                        Action = "MARK_DUPLICATE"
                    },
                    new EdoImportDraftFailureApplyItemDto
                    {
                        CandidateId = partial.Id,
                        Action = "MARK_DUPLICATE"
                    }
                ]
            });
        Assert.False(atomicFailure.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.Failed, exact.Status);
        Assert.Equal(EdoImportCandidateStatus.Failed, partial.Status);
        Assert.Null(exact.ExistingPurchaseId);

        var request = new EdoImportDraftFailureApplyRequestDto
        {
            Confirm = true,
            ExpectedFailureHash = plan.Value.FailureHash,
            Items =
            [
                new EdoImportDraftFailureApplyItemDto
                {
                    CandidateId = exact.Id,
                    Action = "MARK_DUPLICATE"
                },
                new EdoImportDraftFailureApplyItemDto
                {
                    CandidateId = partial.Id,
                    Action = "SKIP"
                },
                new EdoImportDraftFailureApplyItemDto
                {
                    CandidateId = multiple.Id,
                    Action = "SKIP"
                }
            ]
        };
        var purchaseCount = context.PurchaseDocs.Count();
        var purchaseMarkingCount = context.ProductTables.Count();
        var candidateMarkingCount = context.EdoImportCandidateMarkings.Count();

        var applied = await service.ApplyDraftImportFailuresAsync(job.Id, request);
        var retry = await service.ApplyDraftImportFailuresAsync(job.Id, request);

        Assert.True(applied.IsSuccess);
        Assert.Equal(1, applied.Value.MarkedDuplicateCandidateCount);
        Assert.Equal(2, applied.Value.SkippedCandidateCount);
        Assert.Equal(0, applied.Value.FailedCount);
        Assert.Equal(1, applied.Value.DuplicateCount);
        Assert.Equal(6, applied.Value.SkippedCount);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.MarkedDuplicateCandidateCount);
        Assert.Equal(0, retry.Value.SkippedCandidateCount);
        Assert.Equal(EdoImportCandidateStatus.Duplicate, exact.Status);
        Assert.Equal(EdoImportDuplicateState.Confirmed, exact.DuplicateState);
        Assert.Equal(990, exact.ExistingPurchaseId);
        Assert.Null(exact.SafeErrorCode);
        Assert.Equal(EdoImportCandidateStatus.Skipped, partial.Status);
        Assert.Equal(EdoImportDraftFailurePolicy.MarkingAlreadyUsedSkipped,
            partial.SafeErrorCode);
        Assert.Equal(EdoImportCandidateStatus.Skipped, multiple.Status);
        Assert.Equal(EdoImportDraftFailurePolicy.MarkingAlreadyUsedSkipped,
            multiple.SafeErrorCode);
        Assert.Equal(purchaseCount, context.PurchaseDocs.Count());
        Assert.Equal(purchaseMarkingCount, context.ProductTables.Count());
        Assert.Equal(candidateMarkingCount, context.EdoImportCandidateMarkings.Count());
    }

    [Fact]
    public async Task Draft_marking_failure_ownership_is_organization_scoped()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        context.SetUserContext(new ScopedTestUserContext(22));
        context.Products.Add(new Product
        {
            Id = 320,
            OrganizationId = 22,
            UnitId = 5,
            Name = "Other organization marked product",
            Mxik = "12345678901234567",
            IsPieceTracked = true,
            IsPurchased = true,
            DefaultVatRateId = 2,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = Now
        });
        SeedPurchaseMarkingOwner(
            context, 220, 820, [(920, "FOREIGN-DRAFT-MARK")], 22, 320);
        await context.SaveChangesAsync();
        context.SetUserContext(new ScopedTestUserContext(11));
        var store = new EdoImportStore(context);

        var currentOrganization = await store.GetDraftMarkingUsageAsync(
            11, ["FOREIGN-DRAFT-MARK"]);
        var owningOrganization = await store.GetDraftMarkingUsageAsync(
            22, ["FOREIGN-DRAFT-MARK"]);

        Assert.Equal(1, currentOrganization.TotalMarkingCount);
        Assert.Equal(0, currentOrganization.UsedMarkingCount);
        Assert.Empty(currentOrganization.ExistingPurchaseIds);
        Assert.Null(currentOrganization.ExistingPurchaseIdForAllMarkings);
        Assert.Equal(1, owningOrganization.UsedMarkingCount);
        Assert.Equal([220L], owningOrganization.ExistingPurchaseIds);
        Assert.Equal(220, owningOrganization.ExistingPurchaseIdForAllMarkings);
    }

    [Fact]
    public async Task Linked_or_imported_candidate_cannot_be_requeued()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var linked = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-LINKED", marking: null, totalAmount: 112m);
        var imported = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-IMPORTED", marking: null, totalAmount: 112m);
        context.PurchaseDocs.AddRange(Purchase(880), Purchase(881));
        linked.SafeErrorCode = "DRAFT_IMPORT_PURCHASEDOC_SERVICEITEMSNOTALLOWED";
        linked.ExistingPurchaseId = 880;
        linked.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        imported.ImportedPurchaseId = 881;
        imported.ExistingPurchaseId = 881;
        imported.TransitionTo(EdoImportCandidateStatus.Importing, Now);
        imported.TransitionTo(EdoImportCandidateStatus.Imported, Now);
        job.ReadyCount = 0;
        job.FailedCount = 1;
        job.ImportedCount = 1;
        job.TransitionTo(EdoImportJobStatus.Importing, Now);
        job.TransitionTo(EdoImportJobStatus.Partial, Now);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var request = new EdoImportDraftRequeueRequestDto { Confirm = true };

        var linkedResult = await service.RequeueDraftCandidateAsync(job.Id, linked.Id, request);
        var importedResult = await service.RequeueDraftCandidateAsync(job.Id, imported.Id, request);

        Assert.False(linkedResult.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_REQUEUE_PURCHASE_LINKED", linkedResult.Error.Code);
        Assert.False(importedResult.IsSuccess);
        Assert.Equal("DRAFT_IMPORT_REQUEUE_PURCHASE_LINKED", importedResult.Error.Code);
        Assert.Equal(EdoImportCandidateStatus.Failed, linked.Status);
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Equal(2, await context.PurchaseDocs.CountAsync());
    }

    [Fact]
    public async Task Marked_goods_non_piece_product_is_not_ready_and_piece_tracking_apply_is_idempotent()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(
            store,
            quantity: 2,
            markings: ["PIECE-1", "PIECE-2"]);
        var service = CreateService(context, new RecordingScheduler());

        var resolved = await service.ResolveMappingsAsync(job.Id);
        var plan = await service.GetPieceTrackingPlanAsync(job.Id);
        var foreignPlan = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .GetPieceTrackingPlanAsync(job.Id);

        Assert.True(resolved.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal("PRODUCT_PIECE_TRACKING_REQUIRED", candidate.SafeErrorCode);
        Assert.True(plan.IsSuccess);
        var item = Assert.Single(plan.Value.Products);
        Assert.Equal(300, item.ProductId);
        Assert.Equal(1, item.AffectedCandidateCount);
        Assert.Equal(2, item.MarkingCount);
        Assert.False(item.IsService);
        Assert.False(item.IsPieceTracked);
        Assert.Equal("ENABLE_PIECE_TRACKING", item.SafeAction);
        Assert.Equal(64, plan.Value.PlanHash.Length);
        var planJson = System.Text.Json.JsonSerializer.Serialize(plan.Value);
        Assert.DoesNotContain("PIECE-1", planJson, StringComparison.Ordinal);
        Assert.DoesNotContain("PIECE-2", planJson, StringComparison.Ordinal);
        Assert.DoesNotContain("MarkingNumber", planJson, StringComparison.OrdinalIgnoreCase);
        Assert.False(foreignPlan.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", foreignPlan.Error.Code);

        var notConfirmed = await service.ApplyPieceTrackingAsync(job.Id,
            new EdoImportPieceTrackingApplyRequestDto
            {
                Confirm = false,
                ExpectedPlanHash = plan.Value.PlanHash,
                ProductIds = [300]
            });
        Assert.False(notConfirmed.IsSuccess);
        Assert.Equal("PIECE_TRACKING_CONFIRMATION_REQUIRED", notConfirmed.Error.Code);

        var stale = await service.ApplyPieceTrackingAsync(job.Id,
            new EdoImportPieceTrackingApplyRequestDto
            {
                Confirm = true,
                ExpectedPlanHash = new string('0', 64),
                ProductIds = [300]
            });
        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_PIECE_TRACKING_PLAN", stale.Error.Code);
        Assert.False(context.Products.Single(product => product.Id == 300).IsPieceTracked);

        var request = new EdoImportPieceTrackingApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.Value.PlanHash,
            ProductIds = [300]
        };
        var foreignApply = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .ApplyPieceTrackingAsync(job.Id, request);
        Assert.False(foreignApply.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", foreignApply.Error.Code);
        var first = await service.ApplyPieceTrackingAsync(job.Id, request);
        var retry = await service.ApplyPieceTrackingAsync(job.Id, request);

        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value.UpdatedProductCount);
        Assert.Equal(1, first.Value.ReadyCount);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.UpdatedProductCount);
        Assert.Equal(1, retry.Value.ReusedProductCount);
        Assert.True(context.Products.Single(product => product.Id == 300).IsPieceTracked);
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Null(candidate.SafeErrorCode);
        Assert.Equal(["PIECE-1", "PIECE-2"],
            Assert.Single(candidate.Lines).Markings.OrderBy(marking => marking.Id)
                .Select(marking => marking.MarkingNumber));
        Assert.Empty(context.PurchaseDocs);
        Assert.Empty(context.WarehouseProductMovements);
        Assert.Empty(context.AccountingRegisterEntries);

        var factory = new RecordingHistoricalDraftFactory(context);
        var importService = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);
        var importPlan = (await importService.GetImportPlanAsync(job.Id)).Value;
        var imported = await importService.ImportDraftsAsync(job.Id,
            new EdoImportDraftBatchRequestDto
            {
                Confirm = true,
                ExpectedImportPlanHash = importPlan.ImportPlanHash,
                BatchSize = 1
            });

        Assert.True(imported.IsSuccess);
        Assert.Equal(1, imported.Value.CreatedDraftCount);
        Assert.Equal(2, await context.ProductTables.CountAsync(table =>
            table.MarkingNumber == "PIECE-1" || table.MarkingNumber == "PIECE-2"));
        Assert.Empty(context.WarehouseProductMovements);
        Assert.Empty(context.AccountingRegisterEntries);
    }

    [Fact]
    public async Task Durable_mapping_cannot_bypass_marked_goods_piece_tracking_requirement()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(
            store,
            quantity: 1,
            markings: ["DURABLE-PIECE"]);
        var line = Assert.Single(candidate.Lines);
        line.ProviderProductName = "Durable marked goods";
        line.PackageCode = "BOX";
        var identityHash = EdoProviderProductIdentity.Create(
            candidate.ProviderCode,
            line.CatalogCode,
            line.PackageCode,
            line.ProviderProductName,
            line.IsService);
        Assert.NotNull(identityHash);
        context.EdoProviderProductMappings.Add(new EdoProviderProductMapping
        {
            OrganizationId = 11,
            ProviderCode = candidate.ProviderCode,
            CatalogCode = line.CatalogCode!,
            PackageCode = line.PackageCode,
            ProviderProductName = line.ProviderProductName,
            ProviderProductNameHash = EdoProviderProductIdentity.HashName(line.ProviderProductName),
            IdentityHash = identityHash,
            IsService = false,
            ProductId = 300,
            CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var result = await service.ResolveMappingsAsync(job.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(300, line.SelectedProductId);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal("PRODUCT_PIECE_TRACKING_REQUIRED", candidate.SafeErrorCode);
        Assert.Single(line.Markings);
    }

    [Fact]
    public async Task Piece_tracking_apply_rejects_a_service_product()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Products.Single(product => product.Id == 300).IsService = true;
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-SERVICE-PIECE-BLOCK", "SERVICE-PIECE", totalAmount: 112m);
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetPieceTrackingPlanAsync(job.Id)).Value;
        var item = Assert.Single(plan.Products);
        Assert.Equal("BLOCKED_SERVICE", item.SafeAction);

        var result = await service.ApplyPieceTrackingAsync(job.Id,
            new EdoImportPieceTrackingApplyRequestDto
            {
                Confirm = true,
                ExpectedPlanHash = plan.PlanHash,
                ProductIds = [300]
            });

        Assert.False(result.IsSuccess);
        Assert.Equal("PIECE_TRACKING_SELECTION_INVALID", result.Error.Code);
        Assert.False(context.Products.Single(product => product.Id == 300).IsPieceTracked);
        Assert.Single(Assert.Single(candidate.Lines).Markings);
    }

    [Fact]
    public async Task Old_failed_marked_goods_can_be_requeued_after_piece_tracking_correction()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-OLD-NON-PIECE", "OLD-PIECE", totalAmount: 112m);
        candidate.SafeErrorCode = "DRAFT_IMPORT_PURCHASEDOC_SERVICEITEMSNOTALLOWED";
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        job.ReadyCount = 0;
        job.FailedCount = 1;
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetPieceTrackingPlanAsync(job.Id)).Value;

        var applied = await service.ApplyPieceTrackingAsync(job.Id,
            new EdoImportPieceTrackingApplyRequestDto
            {
                Confirm = true,
                ExpectedPlanHash = plan.PlanHash,
                ProductIds = [300]
            });
        Assert.True(applied.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.Failed, candidate.Status);
        var requeued = await service.RequeueDraftCandidateAsync(
            job.Id,
            candidate.Id,
            new EdoImportDraftRequeueRequestDto { Confirm = true });

        Assert.True(requeued.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Single(Assert.Single(candidate.Lines).Markings);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Draft_import_rechecks_marked_goods_piece_tracking_before_factory()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-NON-PIECE", "NON-PIECE-MARK", totalAmount: 112m);
        var factory = new RecordingHistoricalDraftFactory(context);
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);
        var plan = (await service.GetImportPlanAsync(job.Id)).Value;

        var result = await service.ImportDraftsAsync(job.Id,
            new EdoImportDraftBatchRequestDto
            {
                Confirm = true,
                ExpectedImportPlanHash = plan.ImportPlanHash,
                BatchSize = 1
            });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.FailedCandidateCount);
        Assert.Equal("DRAFT_IMPORT_PRODUCT_PIECE_TRACKING_REQUIRED",
            Assert.Single(result.Value.Failures).SafeErrorCode);
        Assert.Empty(factory.CandidateOrder);
        var failed = await context.EdoImportCandidates.IgnoreQueryFilters()
            .Include(item => item.Lines).ThenInclude(line => line.Markings)
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Failed, failed.Status);
        Assert.Single(Assert.Single(failed.Lines).Markings);
        Assert.Empty(context.PurchaseDocs);
    }

    [Fact]
    public async Task Draft_import_reuses_existing_provider_purchase_and_never_calls_factory()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var existingPurchase = Purchase(880);
        context.PurchaseDocs.Add(existingPurchase);
        context.EdoDocuments.Add(new EdoDocument
        {
            OrganizationId = 11,
            Provider = "EDOCS",
            Direction = "INBOX",
            InternalDocumentType = "PURCHASE",
            InternalDocumentId = 880,
            ProviderDocumentId = "E-DRAFT-REUSE",
            DocumentType = "FACTURA",
            Status = "SIGNED",
            OperationType = "PURCHASE_FROM_EDO",
            CreatedAt = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-REUSE", marking: null, totalAmount: 112m);
        var factory = new RecordingHistoricalDraftFactory(context);
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);
        var plan = await service.GetImportPlanAsync(job.Id);

        var result = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.CreatedDraftCount);
        Assert.Equal(1, result.Value.ReusedDraftCount);
        Assert.Empty(factory.CandidateOrder);
        var imported = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Equal(880, imported.ImportedPurchaseId);
        Assert.Single(context.PurchaseDocs);
    }

    [Fact]
    public async Task Draft_import_rechecks_marking_usage_after_plan_and_fails_closed()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-DRAFT-LATE-MARK", "MARK-LATE-USED", totalAmount: 112m);
        var factory = new RecordingHistoricalDraftFactory(context);
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            draftFactory: factory);
        var plan = await service.GetImportPlanAsync(job.Id);
        SeedPurchaseMarkingOwner(
            context, 990, 890, [(990, "MARK-LATE-USED")], 11, 300);
        await context.SaveChangesAsync();

        var result = await service.ImportDraftsAsync(job.Id, new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.FailedCandidateCount);
        Assert.Equal("DRAFT_IMPORT_MARKING_ALREADY_USED",
            Assert.Single(result.Value.Failures).SafeErrorCode);
        Assert.Empty(factory.CandidateOrder);
        var failed = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Failed, failed.Status);
        Assert.Equal("DRAFT_IMPORT_MARKING_ALREADY_USED", failed.SafeErrorCode);
        Assert.Single(context.PurchaseDocs);
    }

    [Fact]
    public async Task Bulk_draft_import_continues_ready_candidates_reconciles_controlled_failures_and_completes()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var valid = await SeedReadyImportCandidateAsync(
            store, job, "E-BULK-VALID", marking: null, totalAmount: 112m);
        var lineValuesInvalid = await SeedReadyImportCandidateAsync(
            store, job, "E-BULK-LINE-INVALID", marking: null, totalAmount: 112m);
        var scheduler = new RecordingScheduler();
        var factory = new RecordingHistoricalDraftFactory(
            context,
            failDocumentIdentity: lineValuesInvalid.ProviderDocumentId,
            failErrorCode: EdoImportDraftFailurePolicy.LineValuesInvalid);
        var service = CreateService(
            context, scheduler, new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(), draftFactory: factory);
        var plan = await service.GetImportPlanAsync(job.Id);

        var started = await service.StartBulkImportAsync(job.Id, new EdoImportBulkDraftStartRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 50,
            LineValuesInvalidPolicy = "SKIP",
            MarkingAlreadyUsedPolicy = "MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP"
        });
        await service.ProcessBulkImportAsync(job.Id, "test-worker");
        var status = await service.GetBulkImportStatusAsync(job.Id);

        Assert.True(started.IsSuccess);
        Assert.Equal([job.Id], scheduler.BulkJobIds);
        Assert.True(status.IsSuccess);
        Assert.Equal(EdoImportBulkImportStatus.Completed, status.Value.Status);
        Assert.Equal(2, status.Value.ProcessedCount);
        Assert.Equal(1, status.Value.CreatedDraftCount);
        Assert.Equal(1, status.Value.SkippedCount);
        Assert.Equal(0, status.Value.FailedCount);
        Assert.Equal(0, status.Value.RemainingReadyCount);
        Assert.NotNull(status.Value.StartedAt);
        Assert.NotNull(status.Value.CompletedAt);
        var imported = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(candidate => candidate.Id == valid.Id);
        var skipped = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(candidate => candidate.Id == lineValuesInvalid.Id);
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Equal(EdoImportCandidateStatus.Skipped, skipped.Status);
        Assert.Equal(EdoImportDraftFailurePolicy.LineValuesInvalid + "_SKIPPED",
            skipped.SafeErrorCode);
        Assert.Single(context.PurchaseDocs);
    }

    [Fact]
    public async Task Bulk_draft_import_cancel_is_scoped_and_prevents_worker_processing()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        await SeedReadyImportCandidateAsync(store, job, "E-BULK-CANCEL", marking: null, totalAmount: 112m);
        var scheduler = new RecordingScheduler();
        var factory = new RecordingHistoricalDraftFactory(context);
        var service = CreateService(context, scheduler, new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(), draftFactory: factory);
        var plan = await service.GetImportPlanAsync(job.Id);
        Assert.True((await service.StartBulkImportAsync(job.Id, new EdoImportBulkDraftStartRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 50,
            LineValuesInvalidPolicy = "SKIP",
            MarkingAlreadyUsedPolicy = "MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP"
        })).IsSuccess);

        var cancelled = await service.CancelBulkImportAsync(job.Id);
        await service.ProcessBulkImportAsync(job.Id, "test-worker");

        Assert.True(cancelled.IsSuccess);
        Assert.Equal(EdoImportBulkImportStatus.Cancelled, cancelled.Value.Status);
        Assert.Equal(0, cancelled.Value.ProcessedCount);
        Assert.Empty(factory.CandidateOrder);
        Assert.Empty(context.PurchaseDocs);
    }

    [Theory]
    [InlineData("DRAFT_IMPORT_COMMON_USERHASNOORGANIZATION")]
    [InlineData("DRAFT_IMPORT_DOCUMENTNUMBER_INVALIDORGANIZATION")]
    public async Task Bulk_draft_import_uses_persisted_background_organization_scope_and_recovers_legacy_scope_failure(
        string failureCode)
    {
        var backgroundScope = new BackgroundOrganizationScope();
        await using var context = CreateContext(backgroundScope, setUserContext: false);
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-BULK-BACKGROUND-SCOPE", marking: null, totalAmount: 112m);
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        candidate.SafeErrorCode = failureCode;
        candidate.AttemptCount = 1;
        job.ReadyCount--;
        job.FailedCount++;
        job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
        job.BulkFailedCount = 1;
        job.BulkProcessedCount = 99;
        job.BulkLastSafeErrorCode = failureCode;
        await store.SaveChangesAsync();

        var scheduler = new RecordingScheduler();
        var factory = new RecordingHistoricalDraftFactory(
            context, backgroundOrganizationScope: backgroundScope);
        var starter = CreateService(context, scheduler,
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS), new TestUserContext(),
            draftFactory: factory, backgroundOrganizationScope: backgroundScope);
        var plan = await starter.GetImportPlanAsync(job.Id);
        Assert.True((await starter.StartBulkImportAsync(job.Id, new EdoImportBulkDraftStartRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = plan.Value.ImportPlanHash,
            BatchSize = 50,
            LineValuesInvalidPolicy = "SKIP",
            MarkingAlreadyUsedPolicy = "MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP"
        })).IsSuccess);

        var workerUserContext = new UserContext(
            new Microsoft.AspNetCore.Http.HttpContextAccessor(), backgroundScope);
        var worker = CreateService(context, scheduler,
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS), workerUserContext,
            draftFactory: factory, backgroundOrganizationScope: backgroundScope);
        await worker.ProcessBulkImportAsync(job.Id, "test-worker");

        var imported = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Equal(11, factory.ObservedOrganizationId);
        Assert.False(backgroundScope.IsActive);
        Assert.Null(workerUserContext.OrganizationId);
        Assert.Equal(11, Assert.Single(context.PurchaseDocs.IgnoreQueryFilters()).OrganizationId);
        var status = (await starter.GetBulkImportStatusAsync(job.Id)).Value;
        Assert.Equal(EdoImportBulkImportStatus.Completed, status.Status);
        Assert.Equal(status.ProcessedCount,
            status.CreatedDraftCount + status.ReusedDraftCount + status.DuplicateCount
            + status.SkippedCount + status.FailedCount);
    }

    [Fact]
    public async Task Bulk_draft_import_recovers_user_scope_then_document_number_failure_once()
    {
        var backgroundScope = new BackgroundOrganizationScope();
        await using var context = CreateContext(backgroundScope, setUserContext: false);
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-BULK-RECOVERY-CHAIN", marking: null, totalAmount: 112m);
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        candidate.SafeErrorCode = "DRAFT_IMPORT_COMMON_USERHASNOORGANIZATION";
        candidate.AttemptCount = 1;
        job.ReadyCount--;
        job.FailedCount++;
        job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
        job.BulkFailedCount = 1;
        job.BulkLastSafeErrorCode = candidate.SafeErrorCode;
        await store.SaveChangesAsync();

        var scheduler = new RecordingScheduler();
        var factory = new RecordingHistoricalDraftFactory(
            context,
            failDocumentIdentity: candidate.ProviderDocumentId,
            backgroundOrganizationScope: backgroundScope,
            failureCodes: ["DRAFT_IMPORT_DOCUMENTNUMBER_INVALIDORGANIZATION"]);
        var starter = CreateService(context, scheduler,
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS), new TestUserContext(),
            draftFactory: factory, backgroundOrganizationScope: backgroundScope);
        var worker = CreateService(context, scheduler,
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new UserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor(), backgroundScope),
            draftFactory: factory, backgroundOrganizationScope: backgroundScope);

        var firstPlan = await starter.GetImportPlanAsync(job.Id);
        Assert.True((await starter.StartBulkImportAsync(job.Id, BulkStart(firstPlan.Value.ImportPlanHash))).IsSuccess);
        await worker.ProcessBulkImportAsync(job.Id, "test-worker");

        var afterDocumentNumberFailure = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == candidate.Id);
        Assert.Equal(EdoImportCandidateStatus.Failed, afterDocumentNumberFailure.Status);
        Assert.Equal("DRAFT_IMPORT_DOCUMENTNUMBER_INVALIDORGANIZATION", afterDocumentNumberFailure.SafeErrorCode);
        Assert.Equal(2, afterDocumentNumberFailure.AttemptCount);

        var secondPlan = await starter.GetImportPlanAsync(job.Id);
        Assert.True((await starter.StartBulkImportAsync(job.Id, BulkStart(secondPlan.Value.ImportPlanHash))).IsSuccess);
        await worker.ProcessBulkImportAsync(job.Id, "test-worker");

        var imported = await context.EdoImportCandidates.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == candidate.Id);
        var status = (await starter.GetBulkImportStatusAsync(job.Id)).Value;
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Equal(EdoImportBulkImportStatus.Completed, status.Status);
        Assert.Equal(0, status.FailedCount);
        Assert.Equal(status.ProcessedCount,
            status.CreatedDraftCount + status.ReusedDraftCount + status.DuplicateCount
            + status.SkippedCount + status.FailedCount);
    }

    [Fact]
    public async Task Bulk_draft_import_pauses_with_exhausted_code_after_third_background_failure()
    {
        var backgroundScope = new BackgroundOrganizationScope();
        await using var context = CreateContext(backgroundScope, setUserContext: false);
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var candidate = await SeedReadyImportCandidateAsync(
            store, job, "E-BULK-RECOVERY-EXHAUSTED", marking: null, totalAmount: 112m);
        candidate.TransitionTo(EdoImportCandidateStatus.Failed, Now);
        candidate.SafeErrorCode = "DRAFT_IMPORT_DOCUMENTNUMBER_INVALIDORGANIZATION";
        candidate.AttemptCount = 3;
        job.ReadyCount--;
        job.FailedCount++;
        job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
        job.BulkFailedCount = 1;
        job.BulkLastSafeErrorCode = candidate.SafeErrorCode;
        await store.SaveChangesAsync();

        var scheduler = new RecordingScheduler();
        var factory = new RecordingHistoricalDraftFactory(context, backgroundOrganizationScope: backgroundScope);
        var starter = CreateService(context, scheduler,
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS), new TestUserContext(),
            draftFactory: factory, backgroundOrganizationScope: backgroundScope);
        var worker = CreateService(context, scheduler,
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new UserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor(), backgroundScope),
            draftFactory: factory, backgroundOrganizationScope: backgroundScope);

        var plan = await starter.GetImportPlanAsync(job.Id);
        Assert.True((await starter.StartBulkImportAsync(job.Id, BulkStart(plan.Value.ImportPlanHash))).IsSuccess);
        await worker.ProcessBulkImportAsync(job.Id, "test-worker");

        var status = (await starter.GetBulkImportStatusAsync(job.Id)).Value;
        Assert.Equal(EdoImportBulkImportStatus.Paused, status.Status);
        Assert.Equal("DRAFT_IMPORT_BACKGROUND_ORGANIZATION_RECOVERY_EXHAUSTED", status.LastSafeErrorCode);
        Assert.Empty(factory.CandidateOrder);
        Assert.Equal(status.ProcessedCount,
            status.CreatedDraftCount + status.ReusedDraftCount + status.DuplicateCount
            + status.SkippedCount + status.FailedCount);
    }

    [Fact]
    public async Task Marking_owner_detection_is_organization_scoped_and_service_markings_are_not_persisted()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Products.Single(product => product.Id == 300).IsService = true;
        await context.SaveChangesAsync();
        context.SetUserContext(new ScopedTestUserContext(22));
        context.Products.Add(new Product
        {
            Id = 320, OrganizationId = 22, UnitId = 5, Name = "Other organization product",
            Mxik = "12345678901234567", IsPieceTracked = true, IsPurchased = true,
            DefaultVatRateId = 2, StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        SeedPurchaseMarkingOwner(context, 220, 820, [(920, "FOREIGN-MARK")], 22, 320);
        await context.SaveChangesAsync();
        context.SetUserContext(new ScopedTestUserContext(11));
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var detail = DetailWithMarkingLine("E-SERVICE-MARK", 1, isService: true, ["FOREIGN-MARK"]);

        await CreateProcessor(store, new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, detail.ProviderDocumentId),
                detail))
            .ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates
            .Include(item => item.Lines).ThenInclude(line => line.Markings).SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Empty(Assert.Single(candidate.Lines).Markings);
        Assert.Null(candidate.ExistingPurchaseId);
    }

    [Fact]
    public async Task Linked_local_purchase_180_is_detected_as_duplicate_and_not_recreated()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.PurchaseDocs.Add(Purchase(180));
        context.EdoDocuments.Add(new EdoDocument
        {
            Id = 77,
            OrganizationId = 11,
            Provider = "EDOCS",
            Direction = "INBOX",
            InternalDocumentType = "PURCHASE",
            InternalDocumentId = 180,
            ProviderDocumentId = "E-180",
            DocumentType = "FACTURA",
            Status = "SIGNED",
            OperationType = "IMPORT",
            CreatedAt = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        var processor = CreateProcessor(store,
            new StubHistoricalSource(EdoProviderCode.DIDOX, EmptyPage(EdoProviderCode.DIDOX)),
            new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-180"),
                ValidDetail("E-180")));

        await processor.ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.Duplicate, candidate.Status);
        Assert.Equal(EdoImportDuplicateState.Confirmed, candidate.DuplicateState);
        Assert.Equal(180, candidate.ExistingPurchaseId);
        Assert.Single(context.PurchaseDocs);
        Assert.Equal(1, job.DuplicateCount);
    }

    [Fact]
    public async Task Edo_document_purchase_reference_without_purchase_row_is_not_confirmed_duplicate()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.EdoDocuments.Add(new EdoDocument
        {
            Id = 78,
            OrganizationId = 11,
            Provider = "EDOCS",
            Direction = "INBOX",
            InternalDocumentType = "PURCHASE",
            InternalDocumentId = 999,
            ProviderDocumentId = "E-STALE-PURCHASE",
            DocumentType = "FACTURA",
            Status = "SIGNED",
            OperationType = "IMPORT",
            CreatedAt = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        await CreateProcessor(
                store,
                new StubHistoricalSource(
                    EdoProviderCode.EDOCS,
                    OnePage(EdoProviderCode.EDOCS, "E-STALE-PURCHASE"),
                    ValidDetail("E-STALE-PURCHASE")))
            .ProcessAsync(job.Id, "test-worker");

        var candidate = await context.EdoImportCandidates.SingleAsync();
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Equal(EdoImportDuplicateState.None, candidate.DuplicateState);
        Assert.Null(candidate.ExistingPurchaseId);
        Assert.Equal(0, job.DuplicateCount);
    }

    [Fact]
    public async Task Failed_prior_job_unimported_candidate_is_reanalyzed_without_duplicate_count()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var priorJob = await SeedJobAsync(store, "EDOCS");
        var priorCandidate = new EdoImportCandidate(
            priorJob.Id,
            11,
            "EDOCS",
            "E-RETRY",
            Now);
        priorCandidate.TransitionTo(EdoImportCandidateStatus.MappingRequired, Now);
        await store.AddCandidateAsync(priorCandidate);
        priorJob.TransitionTo(EdoImportJobStatus.Failed, Now);
        await store.SaveChangesAsync();

        var retryJob = await SeedJobAsync(store, "EDOCS");
        await CreateProcessor(
                store,
                new StubHistoricalSource(
                    EdoProviderCode.EDOCS,
                    OnePage(EdoProviderCode.EDOCS, "E-RETRY"),
                    ValidDetail("E-RETRY")))
            .ProcessAsync(retryJob.Id, "retry-worker");

        var current = await context.EdoImportCandidates.SingleAsync(candidate =>
            candidate.JobId == retryJob.Id);
        Assert.Equal(EdoImportCandidateStatus.Ready, current.Status);
        Assert.Equal(EdoImportDuplicateState.None, current.DuplicateState);
        Assert.Null(current.ExistingPurchaseId);
        Assert.Equal(1, retryJob.DiscoveredCount);
        Assert.Equal(1, retryJob.ReadyCount);
        Assert.Equal(0, retryJob.MappingRequiredCount);
        Assert.Equal(0, retryJob.DuplicateCount);
    }

    [Fact]
    public async Task Prior_imported_candidate_is_confirmed_duplicate_with_purchase_id()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.PurchaseDocs.Add(Purchase(181));
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var priorJob = await SeedJobAsync(store, "EDOCS");
        var priorCandidate = new EdoImportCandidate(
            priorJob.Id,
            11,
            "EDOCS",
            "E-IMPORTED",
            Now)
        {
            ImportedPurchaseId = 181
        };
        priorCandidate.TransitionTo(EdoImportCandidateStatus.Ready, Now);
        priorCandidate.TransitionTo(EdoImportCandidateStatus.Importing, Now);
        priorCandidate.TransitionTo(EdoImportCandidateStatus.Imported, Now);
        await store.AddCandidateAsync(priorCandidate);
        priorJob.TransitionTo(EdoImportJobStatus.Failed, Now);
        await store.SaveChangesAsync();

        var retryJob = await SeedJobAsync(store, "EDOCS");
        await CreateProcessor(
                store,
                new StubHistoricalSource(
                    EdoProviderCode.EDOCS,
                    OnePage(EdoProviderCode.EDOCS, "E-IMPORTED"),
                    ValidDetail("E-IMPORTED")))
            .ProcessAsync(retryJob.Id, "retry-worker");

        var current = await context.EdoImportCandidates.SingleAsync(candidate =>
            candidate.JobId == retryJob.Id);
        Assert.Equal(EdoImportCandidateStatus.Duplicate, current.Status);
        Assert.Equal(EdoImportDuplicateState.Confirmed, current.DuplicateState);
        Assert.Equal(181, current.ExistingPurchaseId);
        Assert.Equal(1, retryJob.DiscoveredCount);
        Assert.Equal(0, retryJob.ReadyCount);
        Assert.Equal(0, retryJob.MappingRequiredCount);
        Assert.Equal(1, retryJob.DuplicateCount);
    }

    [Fact]
    public async Task Job_api_dto_returns_persisted_skipped_count()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "EDOCS");
        job.SkippedCount = 7;
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var result = await service.GetJobAsync(job.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.SkippedCount);
    }

    [Fact]
    public async Task Mapping_summary_is_organization_scoped_and_returns_job_not_found()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        var crossOrganization = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .GetMappingSummaryAsync(job.Id);
        var missing = await CreateService(context, new RecordingScheduler())
            .GetMappingSummaryAsync(job.Id + 999);
        var crossOrganizationPlan = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .GetMasterDataPlanAsync(job.Id);

        Assert.False(crossOrganization.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", crossOrganization.Error.Code);
        Assert.False(missing.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", missing.Error.Code);
        Assert.False(crossOrganizationPlan.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", crossOrganizationPlan.Error.Code);
    }

    [Fact]
    public async Task Master_data_plan_deduplicates_snapshots_blocks_incomplete_products_and_is_read_only()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        for (var index = 1; index <= 3; index++)
        {
            var candidate = SummaryCandidate(
                job.Id,
                $"E-CREATE-{index}",
                "207164728",
                index == 3 ? "Alpha LLC" : "Alpha",
                EdoImportCandidateStatus.MappingRequired,
                "PRODUCT_MAPPING_REQUIRED");
            candidate.ProviderContractNumber = "NEW-1";
            candidate.ProviderContractDate = new DateOnly(2026, 7, 1);
            candidate.Lines.Add(SummaryLine(
                "11111111111111111",
                providerProductName: "Кирпич силикатный",
                isService: false,
                packageCode: "PKG",
                packageName: "штука",
                unitId: 5,
                vatRateId: 2));
            await store.AddCandidateGraphAsync(candidate);
        }

        var noName = SummaryCandidate(
            job.Id,
            "E-NO-NAME",
            "207164729",
            "Beta",
            EdoImportCandidateStatus.MappingRequired,
            "PRODUCT_MAPPING_REQUIRED");
        noName.Lines.Add(SummaryLine(
            "22222222222222222",
            providerProductName: null,
            isService: false,
            unitId: 5,
            vatRateId: 2));
        await store.AddCandidateGraphAsync(noName);

        foreach (var variant in new[]
                 {
                     (ProviderId: "E-CONFLICT-1", IsService: false, UnitId: (short)5, VatId: (short)2),
                     (ProviderId: "E-CONFLICT-2", IsService: true, UnitId: (short)6, VatId: (short)3)
                 })
        {
            var candidate = SummaryCandidate(
                job.Id,
                variant.ProviderId,
                "207164730",
                "Gamma",
                EdoImportCandidateStatus.MappingRequired,
                "PRODUCT_MAPPING_REQUIRED");
            candidate.Lines.Add(SummaryLine(
                "33333333333333333",
                providerProductName: "Conflicting product",
                isService: variant.IsService,
                unitId: variant.UnitId,
                vatRateId: variant.VatId));
            await store.AddCandidateGraphAsync(candidate);
        }

        var existing = SummaryCandidate(
            job.Id,
            "E-EXISTING",
            "300000001",
            "Supplier",
            EdoImportCandidateStatus.MappingRequired,
            "MARKING_MAPPING_REQUIRED",
            counterpartyId: 200,
            contractId: 201,
            currencyId: 1,
            warehouseId: 12);
        existing.ProviderContractNumber = "C-1";
        existing.ProviderContractDate = DateOnly.FromDateTime(Now);
        existing.Lines.Add(SummaryLine(
            "12345678901234567",
            providerProductName: "Existing product",
            isService: false,
            productId: 300,
            unitId: 5,
            vatRateId: 2,
            mappingStatus: EdoImportMappingStatus.Unresolved));
        await store.AddCandidateGraphAsync(existing);

        foreach (var terminal in new[]
                 {
                     (ProviderId: "E-READY-NO-CREATE", Status: EdoImportCandidateStatus.Ready),
                     (ProviderId: "E-DUPLICATE-NO-CREATE", Status: EdoImportCandidateStatus.Duplicate)
                 })
        {
            var candidate = SummaryCandidate(
                job.Id,
                terminal.ProviderId,
                "207199999",
                "Excluded",
                terminal.Status,
                safeErrorCode: null);
            candidate.Lines.Add(SummaryLine(
                "99999999999999999",
                providerProductName: "Must not be planned",
                isService: false,
                unitId: 5,
                vatRateId: 2));
            await store.AddCandidateGraphAsync(candidate);
        }

        var candidateCountBefore = await context.EdoImportCandidates.CountAsync();
        var lineCountBefore = await context.EdoImportCandidateLines.CountAsync();
        Assert.All(context.ChangeTracker.Entries(), entry =>
            Assert.Equal(EntityState.Unchanged, entry.State));

        var result = await CreateService(context, new RecordingScheduler())
            .GetMasterDataPlanAsync(job.Id);

        Assert.True(result.IsSuccess);
        var plan = result.Value;
        var seller = plan.Counterparties.Single(item => item.SellerTin == "207164728");
        Assert.Equal("Alpha", seller.CanonicalSellerName);
        Assert.Equal(["Alpha LLC"], seller.NameAliases);
        Assert.Equal(3, seller.CandidateCount);
        Assert.Equal("CREATE", seller.Action);
        Assert.DoesNotContain(plan.Counterparties, item => item.SellerTin == "207199999");

        var createProduct = plan.Products.Single(item => item.CatalogCode == "11111111111111111");
        Assert.Equal("Кирпич силикатный", createProduct.ProviderProductName);
        Assert.Equal(3, createProduct.CandidateCount);
        Assert.Equal((short)5, createProduct.ResolvedUnitId);
        Assert.Equal((short)2, createProduct.ResolvedVatRateId);
        Assert.Equal("CREATE", createProduct.Action);

        var blockedProduct = plan.Products.Single(item => item.CatalogCode == "22222222222222222");
        Assert.Equal("BLOCKED", blockedProduct.Action);
        Assert.Contains("PRODUCT_NAME_REQUIRED", blockedProduct.BlockedReasonCodes);

        var conflictProduct = plan.Products.Single(item => item.CatalogCode == "33333333333333333");
        Assert.Equal("CONFLICT", conflictProduct.Action);
        Assert.Contains("PRODUCT_ITEM_TYPE_CONFLICT", conflictProduct.BlockedReasonCodes);
        Assert.Contains("PRODUCT_UNIT_CONFLICT", conflictProduct.BlockedReasonCodes);
        Assert.Contains("PRODUCT_VAT_RATE_CONFLICT", conflictProduct.BlockedReasonCodes);

        var existingProduct = plan.Products.Single(item => item.CatalogCode == "12345678901234567");
        Assert.Equal("USE_EXISTING", existingProduct.Action);
        Assert.Equal(300, existingProduct.ExistingProductId);
        Assert.Equal(1, plan.MarkingRequiredCount);
        Assert.DoesNotContain(plan.Products, item => item.CatalogCode == "99999999999999999");
        Assert.True(plan.CreateCount > 0);
        Assert.True(plan.UseExistingCount > 0);
        Assert.True(plan.ConflictCount > 0);
        Assert.True(plan.BlockedCount > 0);

        var json = System.Text.Json.JsonSerializer.Serialize(plan);
        Assert.DoesNotContain("ProviderDocumentId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MarkingNumber", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RawProvider", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Pkcs7", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(candidateCountBefore, await context.EdoImportCandidates.CountAsync());
        Assert.Equal(lineCountBefore, await context.EdoImportCandidateLines.CountAsync());
        Assert.All(context.ChangeTracker.Entries(), entry =>
            Assert.Equal(EntityState.Unchanged, entry.State));
    }

    [Fact]
    public async Task Master_data_apply_requires_confirmation_current_hash_and_organization_scope()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "DIDOX");
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;
        var secondPlan = (await service.GetMasterDataPlanAsync(job.Id)).Value;

        Assert.Equal(64, plan.PlanHash.Length);
        Assert.Equal(plan.PlanHash, secondPlan.PlanHash);
        var notConfirmed = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = false,
            ExpectedPlanHash = plan.PlanHash
        });
        var stale = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = new string('0', 64)
        });
        var crossOrganization = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
            {
                Confirm = true,
                ExpectedPlanHash = plan.PlanHash
            });

        Assert.False(notConfirmed.IsSuccess);
        Assert.Equal("MASTER_DATA_CONFIRMATION_REQUIRED", notConfirmed.Error.Code);
        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_MASTER_DATA_PLAN", stale.Error.Code);
        Assert.False(crossOrganization.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", crossOrganization.Error.Code);
        Assert.Empty(context.CounterpartyCards);
        Assert.Empty(context.Products);
        Assert.Empty(context.Contracts);
    }

    [Fact]
    public async Task Master_data_apply_creates_reuses_maps_contract_and_is_idempotent_without_purchase_or_provider_write()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);

        var create = SummaryCandidate(job.Id, "E-APPLY-CREATE", "207164728", "New Supplier",
            EdoImportCandidateStatus.MappingRequired, "COUNTERPARTY_MAPPING_REQUIRED");
        create.DocumentDate = new DateOnly(2026, 8, 1);
        create.Lines.Add(SummaryLine(
            "11111111111111111", "Кирпич", false, vatRate: 12, vatRateId: 2));
        await store.AddCandidateGraphAsync(create);

        var existing = SummaryCandidate(job.Id, "E-APPLY-REUSE", "300000001", "Supplier",
            EdoImportCandidateStatus.MappingRequired, "CONTRACT_MAPPING_REQUIRED");
        existing.DocumentDate = new DateOnly(2026, 8, 2);
        existing.ProviderContractNumber = "C-NEW";
        existing.ProviderContractDate = new DateOnly(2026, 7, 1);
        existing.Lines.Add(SummaryLine(
            "12345678901234567", "Product", false, vatRate: 12, vatRateId: 2));
        await store.AddCandidateGraphAsync(existing);
        job.DiscoveredCount = 2;
        job.MappingRequiredCount = 2;
        await store.SaveChangesAsync();

        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;
        var request = new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Counterparties =
            [
                new() { SellerTin = "207164728", Action = "CREATE" },
                new() { SellerTin = "300000001", Action = "USE_EXISTING", ExistingCounterpartyId = 200 }
            ],
            Contracts =
            [
                new()
                {
                    SellerTin = "300000001", ProviderContractNumber = "C-NEW",
                    ProviderContractDate = new DateOnly(2026, 7, 1), Action = "CREATE"
                }
            ],
            Products =
            [
                new()
                {
                    CatalogCode = "11111111111111111", Action = "CREATE", IsService = false,
                    IsPieceTracked = false, UnitId = 5, VatRateId = 2
                },
                new()
                {
                    CatalogCode = "12345678901234567", Action = "USE_EXISTING", ExistingProductId = 300,
                    IsService = false, IsPieceTracked = false, UnitId = 5, VatRateId = 2
                }
            ]
        };
        var providerCheckpointBefore = Assert.Single(context.EdoImportJobProviders).UpdatedDate;
        var purchaseCountBefore = await context.PurchaseDocs.CountAsync();

        var first = await service.ApplyMasterDataAsync(job.Id, request);
        var planAfterFirst = (await service.GetMasterDataPlanAsync(job.Id)).Value;
        var second = await service.ApplyMasterDataAsync(job.Id, request);

        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value.CreatedCounterpartyCount);
        Assert.Equal(1, first.Value.ReusedCounterpartyCount);
        Assert.Equal(1, first.Value.CreatedContractCount);
        Assert.Equal(1, first.Value.CreatedProductCount);
        Assert.Equal(1, first.Value.ReusedProductCount);
        Assert.NotEqual(plan.PlanHash, planAfterFirst.PlanHash);
        Assert.True(second.IsSuccess);
        Assert.Equal(0, second.Value.CreatedCounterpartyCount);
        Assert.Equal(2, second.Value.ReusedCounterpartyCount);
        Assert.Equal(0, second.Value.CreatedContractCount);
        Assert.Equal(1, second.Value.ReusedContractCount);
        Assert.Equal(0, second.Value.CreatedProductCount);
        Assert.Equal(2, second.Value.ReusedProductCount);
        Assert.Single(context.CounterpartyCards.Where(item => item.Inn == "207164728"));
        Assert.Single(context.Products.Where(item => item.Mxik == "11111111111111111"));
        var contract = Assert.Single(context.Contracts.Where(item =>
            item.ProviderCode == "EDOCS"
            && item.ProviderContractNumber == "C-NEW"
            && item.ProviderContractDate == new DateOnly(2026, 7, 1)));
        Assert.NotEqual("C-NEW", contract.ContractNumber);
        Assert.Equal(contract.Id, existing.SelectedContractId);
        Assert.Equal(purchaseCountBefore, await context.PurchaseDocs.CountAsync());
        Assert.Empty(context.AccountingRegisterEntries);
        Assert.Empty(context.WarehouseProductMovements);
        Assert.Equal(providerCheckpointBefore, Assert.Single(context.EdoImportJobProviders).UpdatedDate);
    }

    [Fact]
    public async Task Contract_apply_persists_selected_id_while_product_is_partial_and_plan_becomes_use_existing()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        var candidate = SummaryCandidate(job.Id, "E-PARTIAL-CONTRACT", "300000001", "Supplier",
            EdoImportCandidateStatus.MappingRequired, "CONTRACT_MAPPING_REQUIRED", counterpartyId: 200);
        candidate.DocumentDate = new DateOnly(2026, 8, 2);
        candidate.ProviderContractNumber = "52";
        candidate.ProviderContractDate = new DateOnly(2026, 7, 1);
        candidate.Lines.Add(SummaryLine(
            "99999999999999999", providerProductName: null, isService: false, vatRate: 12, vatRateId: 2));
        await store.AddCandidateGraphAsync(candidate);
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var before = (await service.GetMasterDataPlanAsync(job.Id)).Value;

        var applied = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = before.PlanHash,
            Contracts =
            [
                new()
                {
                    SellerTin = "300000001", ProviderContractNumber = "52",
                    ProviderContractDate = new DateOnly(2026, 7, 1), Action = "CREATE"
                }
            ]
        });
        var after = (await service.GetMasterDataPlanAsync(job.Id)).Value;

        Assert.True(applied.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.NotNull(candidate.SelectedContractId);
        var contractPlan = Assert.Single(after.Contracts);
        Assert.Equal("USE_EXISTING", contractPlan.Action);
        Assert.Equal(candidate.SelectedContractId, contractPlan.ExistingContractId);
        Assert.NotEqual(before.PlanHash, after.PlanHash);
        var contract = await context.Contracts.SingleAsync(item => item.Id == candidate.SelectedContractId);
        Assert.NotEqual("52", contract.ContractNumber);
        Assert.Equal("52", contract.ProviderContractNumber);
    }

    [Fact]
    public async Task Didox_contract_without_provider_identity_requires_candidate_scoped_existing_contract_and_is_idempotent()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.SetUserContext(new ScopedTestUserContext(22));
        context.CounterpartyCards.Add(new CounterpartyCard
        {
            Id = 220, OrganizationId = 22, CounterpartyTypeId = 1, ShortName = "Other supplier",
            Inn = "300000001", StateId = StateIdConst.ACTIVE, IsSupplier = true, CreatedDate = Now
        });
        context.Contracts.Add(new Contract
        {
            Id = 220, OrganizationId = 22, CounterpartyId = 220, ContractTypeId = 1,
            ContractNumber = "OTHER", ContractDate = Now, StartDate = new DateTime(2026, 1, 1),
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        context.SetUserContext(new TestUserContext());

        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "DIDOX");
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        var candidate = new EdoImportCandidate(job.Id, 11, "DIDOX", "D-NO-CONTRACT-METADATA", Now)
        {
            SellerTin = "300000001",
            SellerName = "Supplier",
            SelectedCounterpartyId = 200,
            SafeErrorCode = "CONTRACT_MAPPING_REQUIRED"
        };
        candidate.TransitionTo(EdoImportCandidateStatus.MappingRequired, Now);
        candidate.DocumentDate = new DateOnly(2026, 8, 2);
        candidate.Lines.Add(SummaryLine("99999999999999999", providerProductName: null, isService: false));
        await store.AddCandidateGraphAsync(candidate);
        await store.SaveChangesAsync();

        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.DIDOX));
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;
        var contractPlan = Assert.Single(plan.Contracts);
        Assert.Null(contractPlan.ProviderContractNumber);
        Assert.Null(contractPlan.ProviderContractDate);
        Assert.Equal("REQUIRES_SELECTION", contractPlan.Action);
        Assert.Equal([candidate.Id], contractPlan.CandidateIds);
        Assert.Contains(201, contractPlan.ReconciliationContractIds);
        Assert.DoesNotContain(220, contractPlan.ReconciliationContractIds);

        var crossOrganization = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Contracts =
            [
                new()
                {
                    SellerTin = "300000001", Action = "USE_EXISTING", ExistingContractId = 220,
                    CandidateIds = [candidate.Id]
                }
            ]
        });
        Assert.False(crossOrganization.IsSuccess);
        Assert.Equal("MASTER_DATA_SELECTION_INVALID", crossOrganization.Error.Code);

        var request = new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Contracts =
            [
                new()
                {
                    SellerTin = "300000001", Action = "USE_EXISTING", ExistingContractId = 201,
                    CandidateIds = [candidate.Id]
                }
            ]
        };
        var first = await service.ApplyMasterDataAsync(job.Id, request);
        var after = (await service.GetMasterDataPlanAsync(job.Id)).Value;
        var retry = await service.ApplyMasterDataAsync(job.Id, request);

        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value.ReusedContractCount);
        Assert.Equal(201, candidate.SelectedContractId);
        Assert.Null((await context.Contracts.SingleAsync(item => item.Id == 201)).ProviderCode);
        Assert.Null((await context.Contracts.SingleAsync(item => item.Id == 201)).ProviderContractNumber);
        Assert.Equal("USE_EXISTING", Assert.Single(after.Contracts).Action);
        Assert.NotEqual(plan.PlanHash, after.PlanHash);
        Assert.True(retry.IsSuccess);
        Assert.Equal(1, retry.Value.ReusedContractCount);
        Assert.Equal(2, await context.Contracts.CountAsync());
    }

    [Fact]
    public async Task Contract_plan_keeps_case_and_number_prefix_aliases_separate_and_requires_explicit_reconciliation()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        context.Contracts.Add(new Contract
        {
            Id = 990, OrganizationId = 11, CounterpartyId = 200, ContractTypeId = 1,
            ContractNumber = "100000990", ContractDate = new DateTime(2026, 7, 1),
            StartDate = new DateTime(2026, 7, 1), StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        foreach (var number in new[] { "\u0442\u0430\u0440\u0438\u0444", "\u0422\u0430\u0440\u0438\u0444", "52", "\u211652" })
        {
            var candidate = SummaryCandidate(job.Id, $"E-ALIAS-{number}", "300000001", "Supplier",
                EdoImportCandidateStatus.MappingRequired, "CONTRACT_MAPPING_REQUIRED", counterpartyId: 200);
            candidate.ProviderContractNumber = number;
            candidate.ProviderContractDate = new DateOnly(2026, 7, 1);
            candidate.Lines.Add(SummaryLine("99999999999999999"));
            await store.AddCandidateGraphAsync(candidate);
        }
        await store.SaveChangesAsync();

        var plan = (await CreateService(context, new RecordingScheduler())
            .GetMasterDataPlanAsync(job.Id)).Value;

        Assert.Equal(4, plan.Contracts.Count);
        Assert.True(new HashSet<string>(plan.Contracts.Select(item => item.ProviderContractNumber!), StringComparer.Ordinal)
            .SetEquals(["\u0442\u0430\u0440\u0438\u0444", "\u0422\u0430\u0440\u0438\u0444", "52", "\u211652"]));
        Assert.All(plan.Contracts, item =>
        {
            Assert.Equal("REQUIRES_SELECTION", item.Action);
            Assert.Contains(990, item.ReconciliationContractIds);
        });
    }

    [Fact]
    public async Task Contract_reconciliation_rejects_cross_counterparty_selection()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        context.CounterpartyCards.Add(new CounterpartyCard
        {
            Id = 991, OrganizationId = 11, CounterpartyTypeId = 2, ShortName = "Other",
            Inn = "300000002", IsSupplier = true, StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Contracts.Add(new Contract
        {
            Id = 992, OrganizationId = 11, CounterpartyId = 991, ContractTypeId = 1,
            ContractNumber = "100000992", ContractDate = new DateTime(2026, 7, 1),
            StartDate = new DateTime(2026, 7, 1), StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        var candidate = SummaryCandidate(job.Id, "E-WRONG-CP", "300000001", "Supplier",
            EdoImportCandidateStatus.MappingRequired, "CONTRACT_MAPPING_REQUIRED", counterpartyId: 200);
        candidate.ProviderContractNumber = "52";
        candidate.ProviderContractDate = new DateOnly(2026, 7, 1);
        candidate.Lines.Add(SummaryLine("99999999999999999"));
        await store.AddCandidateGraphAsync(candidate);
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;

        var result = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Contracts =
            [
                new()
                {
                    SellerTin = "300000001", ProviderContractNumber = "52",
                    ProviderContractDate = new DateOnly(2026, 7, 1), Action = "USE_EXISTING",
                    ExistingContractId = 992
                }
            ]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("MASTER_DATA_SELECTION_INVALID", result.Error.Code);
        Assert.Null(candidate.SelectedContractId);
        Assert.Null((await context.Contracts.SingleAsync(item => item.Id == 992)).ProviderCode);
    }

    [Fact]
    public async Task Master_data_apply_rejects_missing_unit_before_any_write_and_rolls_back()
    {
        await using var context = CreateContext();
        context.VatRates.Add(new VatRate
        {
            Id = 2, Code = "12", Name = "12%", Rate = 12,
            StateId = StateIdConst.ACTIVE, EffectiveFrom = new DateOnly(2020, 1, 1), CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        var candidate = SummaryCandidate(job.Id, "E-ATOMIC", "207164728", "Atomic Supplier",
            EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED");
        candidate.Lines.Add(SummaryLine(
            "11111111111111111", "Atomic product", false, vatRate: 12, vatRateId: 2));
        await store.AddCandidateGraphAsync(candidate);
        await store.SaveChangesAsync();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new TestUserContext(),
            unitOfWork);
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;

        var result = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Counterparties = [new() { SellerTin = "207164728", Action = "CREATE" }],
            Products =
            [
                new()
                {
                    CatalogCode = "11111111111111111", Action = "CREATE", IsService = false,
                    IsPieceTracked = false, UnitId = null, VatRateId = 2
                }
            ]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("PRODUCT_MASTER_DATA_RESOLUTION_REQUIRED", result.Error.Code);
        Assert.Equal(1, unitOfWork.RollbackCount);
        Assert.DoesNotContain(context.CounterpartyCards, item => item.Inn == "207164728");
        Assert.DoesNotContain(context.Products, item => item.Mxik == "11111111111111111");
    }

    [Fact]
    public async Task Master_data_apply_does_not_create_conflicting_product()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        foreach (var isService in new[] { false, true })
        {
            var candidate = SummaryCandidate(job.Id, $"E-CONFLICT-{isService}", "207164728", "Supplier",
                EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED");
            candidate.Lines.Add(SummaryLine(
                "11111111111111111", "Conflict", isService, vatRate: 12, unitId: 5, vatRateId: 2));
            await store.AddCandidateGraphAsync(candidate);
        }
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;

        var result = await service.ApplyMasterDataAsync(job.Id, new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Products =
            [
                new()
                {
                    CatalogCode = "11111111111111111", Action = "CREATE", IsService = false,
                    IsPieceTracked = false, UnitId = 5, VatRateId = 2
                }
            ]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("PRODUCT_MASTER_DATA_RESOLUTION_REQUIRED", result.Error.Code);
        Assert.DoesNotContain(context.Products, item => item.Mxik == "11111111111111111");
    }

    [Fact]
    public async Task Product_defaults_apply_maps_exact_packages_skips_unknown_and_conflict_and_preserves_marking_policy()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Units.AddRange(
            new Unit { Id = 1, Code = "PCE", Name = "Piece", StateId = StateIdConst.ACTIVE },
            new Unit { Id = 4, Code = "MTR", Name = "Meter", StateId = StateIdConst.ACTIVE });
        context.Products.AddRange(
            new Product
            {
                Id = 401, OrganizationId = 11, UnitId = 1, Name = "Compatible",
                Mxik = "44444444444444444", IsService = false, IsPurchased = true,
                DefaultVatRateId = 2, StateId = StateIdConst.ACTIVE, CreatedDate = Now
            },
            new Product
            {
                Id = 402, OrganizationId = 11, UnitId = 4, Name = "Incompatible",
                Mxik = "55555555555555555", IsService = false, IsPurchased = true,
                DefaultVatRateId = 2, StateId = StateIdConst.ACTIVE, CreatedDate = Now
            });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);

        async Task AddProductCandidate(
            string providerId,
            string code,
            string packageName,
            bool isService = false,
            bool marking = false)
        {
            var candidate = SummaryCandidate(job.Id, providerId, "300000001", "Supplier",
                EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED",
                counterpartyId: 200, contractId: 201, currencyId: 1, warehouseId: 12);
            candidate.DocumentDate = new DateOnly(2026, 8, 1);
            var line = SummaryLine(code, $"Product {code}", isService,
                packageName: packageName, vatRate: 12, vatRateId: 2);
            if (marking)
                line.Markings.Add(new EdoImportCandidateMarking
                {
                    MarkingNumber = $"MARK-{providerId}",
                    ProviderVerificationState = EdoImportMarkingVerificationState.Unverified,
                    CreatedDate = Now
                });
            candidate.Lines.Add(line);
            await store.AddCandidateGraphAsync(candidate);
        }

        await AddProductCandidate("E-PIECE", "11111111111111111", "шт.");
        await AddProductCandidate("E-METER", "22222222222222222", "метр", isService: true);
        await AddProductCandidate("E-UNKNOWN", "33333333333333333", "ШТ.");
        await AddProductCandidate("E-COMPATIBLE", "44444444444444444", "шт.");
        await AddProductCandidate("E-INCOMPATIBLE", "55555555555555555", "шт.");
        await AddProductCandidate("E-MARKING", "66666666666666666", "шт.", marking: true);
        await AddProductCandidate("E-CONFLICT-A", "77777777777777777", "шт.");
        await AddProductCandidate("E-CONFLICT-B", "77777777777777777", "шт.", isService: true);
        await store.SaveChangesAsync();

        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetMasterDataPlanAsync(job.Id)).Value;
        Assert.Equal("CONFLICT", plan.Products.Single(item =>
            item.CatalogCode == "77777777777777777").Action);
        Assert.Equal("BLOCKED", plan.Products.Single(item =>
            item.CatalogCode == "11111111111111111").Action);
        Assert.Equal("USE_EXISTING", plan.Products.Single(item =>
            item.CatalogCode == "44444444444444444").Action);
        var request = new EdoImportProductDefaultsApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            MarkingPolicy = "PIECE_TRACKED_WHEN_REQUIRED",
            PackageUnitMappings =
            [
                new() { PackageName = "шт.", UnitId = 1 },
                new() { PackageName = "метр", UnitId = 4 }
            ]
        };
        var eligibleCodes = plan.Products.Where(item =>
                item.Action == "BLOCKED"
                && item.BlockedReasonCodes.SequenceEqual(["PRODUCT_UNIT_REQUIRED"])
                && request.PackageUnitMappings.Any(mapping => mapping.PackageName == item.PackageName))
            .Select(item => item.CatalogCode!)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            ["11111111111111111", "22222222222222222", "66666666666666666"],
            eligibleCodes);

        var first = await service.ApplyProductDefaultsAsync(job.Id, request);
        var retry = await service.ApplyProductDefaultsAsync(job.Id, request);

        Assert.True(first.IsSuccess);
        Assert.Equal(3, first.Value.CreatedProductCount);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.CreatedProductCount);
        Assert.Equal(3, retry.Value.ReusedProductCount);
        Assert.Equal((short)1, (await context.Products.SingleAsync(item => item.Mxik == "11111111111111111")).UnitId);
        var meter = await context.Products.SingleAsync(item => item.Mxik == "22222222222222222");
        Assert.Equal((short)4, meter.UnitId);
        Assert.True(meter.IsService);
        Assert.True((await context.Products.SingleAsync(item => item.Mxik == "66666666666666666")).IsPieceTracked);
        Assert.DoesNotContain(context.Products, item => item.Mxik == "33333333333333333");
        Assert.DoesNotContain(context.Products, item => item.Mxik == "77777777777777777");
        Assert.Single(context.Products.Where(item => item.Mxik == "44444444444444444"));
        Assert.Single(context.Products.Where(item => item.Mxik == "55555555555555555"));
        Assert.Equal((short)4, (await context.Products.SingleAsync(item => item.Mxik == "55555555555555555")).UnitId);
        Assert.Equal(200, (await context.EdoImportCandidates.SingleAsync(item => item.ProviderDocumentId == "E-PIECE")).SelectedCounterpartyId);
        Assert.Equal(201, (await context.EdoImportCandidates.SingleAsync(item => item.ProviderDocumentId == "E-PIECE")).SelectedContractId);
    }

    [Fact]
    public async Task Product_defaults_apply_rejects_stale_hash_and_cross_organization_without_writes()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        await store.SaveChangesAsync();
        var request = new EdoImportProductDefaultsApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = new string('0', 64),
            MarkingPolicy = "PIECE_TRACKED_WHEN_REQUIRED",
            PackageUnitMappings = [new() { PackageName = "шт.", UnitId = 1 }]
        };

        var stale = await CreateService(context, new RecordingScheduler())
            .ApplyProductDefaultsAsync(job.Id, request);
        var crossOrganization = await CreateService(
                context,
                new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .ApplyProductDefaultsAsync(job.Id, request);

        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_MASTER_DATA_PLAN", stale.Error.Code);
        Assert.False(crossOrganization.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", crossOrganization.Error.Code);
        Assert.Empty(context.Products);
    }

    [Fact]
    public async Task Product_conflicts_keep_exact_identities_apply_aliases_and_reuse_mapping_in_future()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        context.Products.Add(new Product
        {
            Id = 301, OrganizationId = 11, UnitId = 5, Name = "Other model",
            Mxik = "12345678901234567", IsPieceTracked = true, IsPurchased = true,
            DefaultVatRateId = 2, StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);

        async Task<EdoImportCandidate> AddIdentity(
            string id, string name, string packageCode, bool hasProviderMarking)
        {
            var candidate = SummaryCandidate(job.Id, id, "300000001", "Supplier",
                EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED",
                counterpartyId: 200, contractId: 201, currencyId: 1, warehouseId: 12);
            candidate.DocumentDate = new DateOnly(2026, 7, 31);
            var line = SummaryLine("12345678901234567", name, false,
                packageCode, "Package", vatRate: 12, vatRateId: 2);
            line.Quantity = 2;
            if (hasProviderMarking)
                line.Markings.Add(new EdoImportCandidateMarking
                {
                    MarkingNumber = $"MARK-{id}",
                    ProviderVerificationState = EdoImportMarkingVerificationState.Mismatch,
                    CreatedDate = Now
                });
            candidate.Lines.Add(line);
            await store.AddCandidateGraphAsync(candidate);
            return candidate;
        }

        var uz = await AddIdentity("E-UZ", "G'isht", "P-1", hasProviderMarking: true);
        var ru = await AddIdentity("E-RU", "Кирпич", "P-1", hasProviderMarking: false);
        var other = await AddIdentity(
            "E-MODEL", "Кирпич модель B", "P-2", hasProviderMarking: false);
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetProductConflictsAsync(job.Id)).Value;

        Assert.Equal(3, plan.Items.Count);
        Assert.Equal(3, plan.Items.Select(item => item.IdentityKey).Distinct().Count());
        var selected = plan.Items.Where(item => item.PackageCode == "P-1").ToArray();
        Assert.Equal(2, selected.Length);
        Assert.All(selected, item => Assert.Equal(2, item.CompatibleProducts.Count));
        Assert.Single(selected, item => item.MarkingRequired);
        Assert.Equal(1, selected.Sum(item => item.MarkedCandidateCount));
        var invalidMarkingRequest = new EdoImportProductConflictApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Items =
            [
                new EdoImportProductConflictApplyItemDto
                {
                    IdentityKeys = selected.Select(item => item.IdentityKey!).ToArray(),
                    Action = "USE_EXISTING", ExistingProductId = 300, IsService = false,
                    UnitId = 5, VatRateId = 2, IsPieceTracked = false
                }
            ]
        };
        var request = new EdoImportProductConflictApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Items =
            [
                new EdoImportProductConflictApplyItemDto
                {
                    IdentityKeys = selected.Select(item => item.IdentityKey!).ToArray(),
                    Action = "USE_EXISTING", ExistingProductId = 300, IsService = false,
                    UnitId = 5, VatRateId = 2, IsPieceTracked = true
                }
            ]
        };

        var invalidMarking = await service.ApplyProductConflictsAsync(job.Id, invalidMarkingRequest);
        var first = await service.ApplyProductConflictsAsync(job.Id, request);
        var retry = await service.ApplyProductConflictsAsync(job.Id, request);

        Assert.False(invalidMarking.IsSuccess);
        Assert.Equal("PRODUCT_MARKING_SELECTION_INVALID", invalidMarking.Error.Code);
        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(2, first.Value.CreatedMappingCount);
        Assert.Equal(2, retry.Value.ReusedMappingCount);
        Assert.Equal(2, await context.EdoProviderProductMappings.CountAsync());
        Assert.Equal(300, Assert.Single(uz.Lines).SelectedProductId);
        Assert.Equal(300, Assert.Single(ru.Lines).SelectedProductId);
        Assert.Null(Assert.Single(other.Lines).SelectedProductId);
        Assert.Equal(201, uz.SelectedContractId);
        Assert.All(uz.Lines.SelectMany(line => line.Markings), marking =>
            Assert.Equal(EdoImportMarkingVerificationState.Mismatch, marking.ProviderVerificationState));

        var future = await store.ResolveMappingAsync(
            11, "EDOCS", "300000001", new DateOnly(2026, 7, 31),
            [new EdoHistoricalDocumentLineDto
            {
                Number = 1, CatalogCode = "12345678901234567", CatalogName = "G'isht",
                PackageCode = "P-1", IsService = false, VatRate = 12
            }]);
        var unmappedModel = await store.ResolveMappingAsync(
            11, "EDOCS", "300000001", new DateOnly(2026, 7, 31),
            [new EdoHistoricalDocumentLineDto
            {
                Number = 1, CatalogCode = "12345678901234567", CatalogName = "Кирпич модель B",
                PackageCode = "P-2", IsService = false, VatRate = 12
            }]);
        Assert.Equal(300, future.Lines[1].ProductId);
        Assert.Null(unmappedModel.Lines[1].ProductId);
    }

    [Fact]
    public async Task Product_conflict_create_is_idempotent_and_rejects_stale_or_cross_organization_requests()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Products.Add(new Product
        {
            Id = 302, OrganizationId = 11, UnitId = 5, Name = "Incompatible service",
            Mxik = "88888888888888888", IsService = true, IsPurchased = true,
            DefaultVatRateId = 2, StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        var candidate = SummaryCandidate(job.Id, "E-CREATE-CONFLICT", "300000001", "Supplier",
            EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED",
            counterpartyId: 200, contractId: 201, currencyId: 1, warehouseId: 12);
        candidate.DocumentDate = new DateOnly(2026, 7, 31);
        candidate.Lines.Add(SummaryLine("88888888888888888", "Exact provider model", false,
            "BOX-8", "Box", vatRate: 12, vatRateId: 2));
        await store.AddCandidateGraphAsync(candidate);
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetProductConflictsAsync(job.Id)).Value;
        var item = Assert.Single(plan.Items);
        var mismatchedProduct = Assert.Single(item.CompatibleProducts);
        Assert.Equal(302, mismatchedProduct.ProductId);
        Assert.Contains("ITEM_TYPE_MISMATCH", mismatchedProduct.CompatibilityCodes);
        Assert.DoesNotContain("STRICT_COMPATIBLE", mismatchedProduct.CompatibilityCodes);
        var request = new EdoImportProductConflictApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = plan.PlanHash,
            Items =
            [
                new()
                {
                    IdentityKeys = [item.IdentityKey!], Action = "CREATE", IsService = false,
                    UnitId = 5, VatRateId = 2, IsPieceTracked = false
                }
            ]
        };

        var incompatible = await service.ApplyProductConflictsAsync(job.Id,
            new EdoImportProductConflictApplyRequestDto
            {
                Confirm = true,
                ExpectedPlanHash = plan.PlanHash,
                Items =
                [
                    new()
                    {
                        IdentityKeys = [item.IdentityKey!], Action = "USE_EXISTING",
                        ExistingProductId = 302, IsService = true, UnitId = 5,
                        VatRateId = 2, IsPieceTracked = false
                    }
                ]
            });

        var first = await service.ApplyProductConflictsAsync(job.Id, request);
        var retry = await service.ApplyProductConflictsAsync(job.Id, request);
        var stale = await service.ApplyProductConflictsAsync(job.Id,
            new EdoImportProductConflictApplyRequestDto
            {
                Confirm = true,
                ExpectedPlanHash = new string('0', 64),
                Items = [new() { IdentityKeys = [new string('1', 64)], Action = "USE_EXISTING", ExistingProductId = 300 }]
            });
        var crossOrganization = await CreateService(context, new RecordingScheduler(),
                new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
                new ScopedTestUserContext(22))
            .ApplyProductConflictsAsync(job.Id, request);

        Assert.False(incompatible.IsSuccess);
        Assert.Equal("PRODUCT_CONFLICT_SELECTION_INVALID", incompatible.Error.Code);
        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value.CreatedProductCount);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.CreatedProductCount);
        Assert.Equal(1, retry.Value.ReusedMappingCount);
        Assert.Single(context.Products.Where(product =>
            product.Mxik == "88888888888888888" && !product.IsService));
        Assert.Single(context.EdoProviderProductMappings);
        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_PRODUCT_CONFLICT_PLAN", stale.Error.Code);
        Assert.False(crossOrganization.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", crossOrganization.Error.Code);
    }

    [Fact]
    public async Task Product_conflict_group_create_requires_explicit_item_type_override_and_creates_one_product()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);

        foreach (var variant in new[]
                 {
                     (Id: "SERVICE-RU", Name: "Электронный документооборот", Package: "RU"),
                     (Id: "SERVICE-UZ", Name: "Elektron hujjat aylanishi", Package: "UZ")
                 })
        {
            var candidate = SummaryCandidate(job.Id, variant.Id, "300000001", "Supplier",
                EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED",
                counterpartyId: 200, contractId: 201, currencyId: 1, warehouseId: 12);
            candidate.DocumentDate = new DateOnly(2026, 7, 31);
            candidate.Lines.Add(SummaryLine("10306002005000000", variant.Name, false,
                variant.Package, "Package", vatRate: 12, vatRateId: 2));
            await store.AddCandidateGraphAsync(candidate);
        }
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var plan = (await service.GetProductConflictsAsync(job.Id)).Value;
        Assert.Equal(2, plan.Items.Count);
        var selection = new EdoImportProductConflictApplyItemDto
        {
            IdentityKeys = plan.Items.Select(item => item.IdentityKey!).ToArray(),
            Action = "CREATE", IsService = true, UnitId = 5, VatRateId = 2,
            IsPieceTracked = false
        };
        var withoutOverride = await service.ApplyProductConflictsAsync(job.Id,
            new EdoImportProductConflictApplyRequestDto
            {
                Confirm = true, ExpectedPlanHash = plan.PlanHash, Items = [selection]
            });

        selection = new EdoImportProductConflictApplyItemDto
        {
            IdentityKeys = selection.IdentityKeys, Action = selection.Action,
            IsService = selection.IsService, UnitId = selection.UnitId,
            VatRateId = selection.VatRateId, IsPieceTracked = selection.IsPieceTracked,
            ConfirmItemTypeOverride = true
        };
        var request = new EdoImportProductConflictApplyRequestDto
        {
            Confirm = true, ExpectedPlanHash = plan.PlanHash, Items = [selection]
        };
        var first = await service.ApplyProductConflictsAsync(job.Id, request);
        var retry = await service.ApplyProductConflictsAsync(job.Id, request);

        Assert.False(withoutOverride.IsSuccess);
        Assert.Equal("PRODUCT_CONFLICT_SELECTION_INVALID", withoutOverride.Error.Code);
        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value.CreatedProductCount);
        Assert.Equal(2, first.Value.CreatedMappingCount);
        Assert.True(retry.IsSuccess);
        Assert.Equal(0, retry.Value.CreatedProductCount);
        Assert.Equal(1, retry.Value.ReusedProductCount);
        Assert.Equal(2, retry.Value.ReusedMappingCount);
        var product = Assert.Single(context.Products.Where(item => item.Mxik == "10306002005000000"));
        Assert.True(product.IsService);
        var mappings = await context.EdoProviderProductMappings
            .Where(mapping => mapping.CatalogCode == "10306002005000000").ToArrayAsync();
        Assert.Equal(2, mappings.Length);
        Assert.All(mappings, mapping =>
        {
            Assert.Equal(product.Id, mapping.ProductId);
            Assert.False(mapping.IsService);
        });
        Assert.All(context.EdoImportCandidateLines.Where(line =>
            line.CatalogCode == "10306002005000000"), line => Assert.Equal(product.Id, line.SelectedProductId));
    }

    [Fact]
    public async Task Marked_service_override_is_rejected_and_marking_changes_plan_hash()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        var candidate = SummaryCandidate(job.Id, "MARKED-SERVICE", "300000001", "Supplier",
            EdoImportCandidateStatus.MappingRequired, "PRODUCT_MAPPING_REQUIRED",
            counterpartyId: 200, contractId: 201, currencyId: 1, warehouseId: 12);
        candidate.DocumentDate = new DateOnly(2026, 7, 31);
        var line = SummaryLine("10304008002000000", "Telecommunication service", false,
            "SERVICE", "Package", vatRate: 12, vatRateId: 2);
        candidate.Lines.Add(line);
        await store.AddCandidateGraphAsync(candidate);
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());
        var beforeMarking = (await service.GetProductConflictsAsync(job.Id)).Value;
        Assert.False(Assert.Single(beforeMarking.Items).MarkingRequired);

        line.Markings.Add(new EdoImportCandidateMarking
        {
            MarkingNumber = "MARKED-SERVICE-CODE",
            ProviderVerificationState = EdoImportMarkingVerificationState.Verified,
            CreatedDate = Now
        });
        await store.SaveChangesAsync();
        var afterMarking = (await service.GetProductConflictsAsync(job.Id)).Value;
        var item = Assert.Single(afterMarking.Items);
        Assert.True(item.MarkingRequired);
        Assert.Equal(1, item.MarkedCandidateCount);
        Assert.NotEqual(beforeMarking.PlanHash, afterMarking.PlanHash);
        var selection = new EdoImportProductConflictApplyItemDto
        {
            IdentityKeys = [item.IdentityKey!], Action = "CREATE", IsService = true,
            UnitId = 5, VatRateId = 2, IsPieceTracked = false,
            ConfirmItemTypeOverride = true
        };
        var stale = await service.ApplyProductConflictsAsync(job.Id,
            new EdoImportProductConflictApplyRequestDto
            {
                Confirm = true, ExpectedPlanHash = beforeMarking.PlanHash, Items = [selection]
            });
        var markedService = await service.ApplyProductConflictsAsync(job.Id,
            new EdoImportProductConflictApplyRequestDto
            {
                Confirm = true, ExpectedPlanHash = afterMarking.PlanHash, Items = [selection]
            });

        Assert.False(stale.IsSuccess);
        Assert.Equal("STALE_PRODUCT_CONFLICT_PLAN", stale.Error.Code);
        Assert.False(markedService.IsSuccess);
        Assert.Equal("PRODUCT_MARKING_SELECTION_INVALID", markedService.Error.Code);
        Assert.DoesNotContain(context.Products, product => product.Mxik == "10304008002000000");
        Assert.DoesNotContain(context.EdoProviderProductMappings,
            mapping => mapping.CatalogCode == "10304008002000000");
    }

    [Theory]
    [InlineData("10306002005000000")]
    [InlineData("10304008002000000")]
    [InlineData("10704008002000000")]
    public async Task Special_service_conflict_catalogs_require_explicit_mapping(string catalogCode)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Products.Single(product => product.Id == 300).Mxik = catalogCode;
        await context.SaveChangesAsync();

        var mapping = await new EdoImportStore(context).ResolveMappingAsync(
            11, "EDOCS", "300000001", new DateOnly(2026, 7, 31),
            [new EdoHistoricalDocumentLineDto
            {
                Number = 1, CatalogCode = catalogCode, CatalogName = "Provider description",
                PackageCode = "P-1", IsService = false, VatRate = 12
            }]);

        Assert.Null(mapping.Lines[1].ProductId);
    }

    [Fact]
    public async Task Mapping_summary_aggregates_only_mapping_required_candidates_without_writes_or_sensitive_fields()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        foreach (var providerId in new[] { "E-PRODUCT-1", "E-PRODUCT-2" })
        {
            var candidate = SummaryCandidate(
                job.Id,
                providerId,
                "207164728",
                "Supplier A",
                EdoImportCandidateStatus.MappingRequired,
                "PRODUCT_MAPPING_REQUIRED");
            candidate.Lines.Add(SummaryLine(
                "12345678901234567",
                isService: false,
                packageCode: "P-1",
                packageName: "Piece",
                vatRate: 12));
            await store.AddCandidateGraphAsync(candidate);
        }

        var missingContract = SummaryCandidate(
            job.Id,
            "E-CONTRACT",
            "207164729",
            "Supplier B",
            EdoImportCandidateStatus.MappingRequired,
            "CONTRACT_MAPPING_REQUIRED",
            counterpartyId: 200,
            currencyId: 1,
            warehouseId: 12);
        missingContract.ProviderContractNumber = "C-77";
        missingContract.ProviderContractDate = new DateOnly(2026, 7, 1);
        missingContract.Lines.Add(SummaryLine(
            "22345678901234567",
            productId: 300,
            unitId: 5,
            vatRateId: 2,
            mappingStatus: EdoImportMappingStatus.Resolved));
        await store.AddCandidateGraphAsync(missingContract);

        var marking = SummaryCandidate(
            job.Id,
            "E-MARKING",
            "207164730",
            "Supplier C",
            EdoImportCandidateStatus.MappingRequired,
            "MARKING_MAPPING_REQUIRED",
            counterpartyId: 200,
            contractId: 201,
            currencyId: 1,
            warehouseId: 12);
        marking.Lines.Add(SummaryLine(
            "32345678901234567",
            productId: 300,
            unitId: 5,
            vatRateId: 2,
            mappingStatus: EdoImportMappingStatus.Unresolved));
        await store.AddCandidateGraphAsync(marking);

        var ready = SummaryCandidate(
            job.Id,
            "E-READY-EXCLUDED",
            "207164728",
            "Supplier A",
            EdoImportCandidateStatus.Ready,
            safeErrorCode: null);
        ready.Lines.Add(SummaryLine("12345678901234567"));
        await store.AddCandidateGraphAsync(ready);

        var duplicate = SummaryCandidate(
            job.Id,
            "E-DUPLICATE-EXCLUDED",
            "207164728",
            "Supplier A",
            EdoImportCandidateStatus.Duplicate,
            safeErrorCode: null);
        duplicate.Lines.Add(SummaryLine("12345678901234567"));
        await store.AddCandidateGraphAsync(duplicate);

        var candidateCountBefore = await context.EdoImportCandidates.CountAsync();
        var lineCountBefore = await context.EdoImportCandidateLines.CountAsync();
        Assert.All(context.ChangeTracker.Entries(), entry =>
            Assert.Equal(EntityState.Unchanged, entry.State));

        var result = await CreateService(context, new RecordingScheduler())
            .GetMappingSummaryAsync(job.Id);

        Assert.True(result.IsSuccess);
        var summary = result.Value;
        Assert.Equal(job.Id, summary.JobId);
        Assert.Equal(6, summary.TotalCandidates);
        Assert.Equal(1, summary.ReadyCount);
        Assert.Equal(1, summary.DuplicateCount);
        Assert.Equal(4, summary.MappingRequiredCount);
        Assert.Equal(2, summary.SafeErrorCodeCounts["PRODUCT_MAPPING_REQUIRED"]);
        Assert.Equal(1, summary.SafeErrorCodeCounts["CONTRACT_MAPPING_REQUIRED"]);
        Assert.Equal(1, summary.SafeErrorCodeCounts["MARKING_MAPPING_REQUIRED"]);

        var seller = Assert.Single(summary.MissingSellers);
        Assert.Equal("207164728", seller.SellerTin);
        Assert.Equal("Supplier A", seller.SellerName);
        Assert.Equal(2, seller.CandidateCount);

        var contract = Assert.Single(summary.MissingContracts);
        Assert.Equal("207164729", contract.SellerTin);
        Assert.Equal(200, contract.CounterpartyId);
        Assert.Equal("C-77", contract.ProviderContractNumber);
        Assert.Equal(new DateOnly(2026, 7, 1), contract.ProviderContractDate);
        Assert.Equal(1, contract.CandidateCount);

        var product = Assert.Single(summary.MissingProducts);
        Assert.Equal("12345678901234567", product.CatalogCode);
        Assert.Null(product.ProviderProductName);
        Assert.Equal("GOODS", product.ItemType);
        Assert.False(product.IsService);
        Assert.Equal("P-1", product.PackageCode);
        Assert.Equal("Piece", product.PackageName);
        Assert.Equal(12, product.VatRate);
        Assert.Equal(2, product.CandidateCount);

        Assert.Equal(2, summary.IssueCounts.Counterparty);
        Assert.Equal(1, summary.IssueCounts.Contract);
        Assert.Equal(2, summary.IssueCounts.Product);
        Assert.Equal(2, summary.IssueCounts.Currency);
        Assert.Equal(2, summary.IssueCounts.Warehouse);
        Assert.Equal(0, summary.IssueCounts.Unit);
        Assert.Equal(0, summary.IssueCounts.VatRate);
        Assert.Equal(1, summary.IssueCounts.Marking);
        Assert.Contains(summary.MissingMasterData, item =>
            item.Type == "PRODUCT" && item.CandidateCount == 2);
        Assert.Contains(summary.MissingMasterData, item =>
            item.Type == "MARKING_VALIDATION" && item.CandidateCount == 1);

        var json = System.Text.Json.JsonSerializer.Serialize(summary);
        Assert.DoesNotContain("ProviderDocumentId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CandidateId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MarkingNumber", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RawProvider", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Pkcs7", json, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(candidateCountBefore, await context.EdoImportCandidates.CountAsync());
        Assert.Equal(lineCountBefore, await context.EdoImportCandidateLines.CountAsync());
        Assert.All(context.ChangeTracker.Entries(), entry =>
            Assert.Equal(EntityState.Unchanged, entry.State));
    }

    [Fact]
    public async Task Candidate_mapping_is_organization_scoped()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store);
        var service = CreateService(
            context,
            new RecordingScheduler(),
            new MutableActiveProviderResolver(EdoProviderCode.EDOCS),
            new ScopedTestUserContext(22));

        var result = await service.UpdateCandidateMappingAsync(
            job.Id,
            candidate.Id,
            ValidMappingRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("EdoImport.JobNotFound", result.Error.Code);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
    }

    [Theory]
    [InlineData("seller", "COUNTERPARTY_MAPPING_REQUIRED")]
    [InlineData("contract", "CONTRACT_MAPPING_REQUIRED")]
    [InlineData("product", "PRODUCT_MAPPING_REQUIRED")]
    [InlineData("not-purchased", "PRODUCT_MAPPING_REQUIRED")]
    [InlineData("unit", "UNIT_MAPPING_REQUIRED")]
    [InlineData("vat", "VAT_MAPPING_REQUIRED")]
    [InlineData("currency", "CURRENCY_MAPPING_REQUIRED")]
    [InlineData("warehouse", "WAREHOUSE_MAPPING_REQUIRED")]
    public async Task Candidate_mapping_keeps_precise_safe_code_for_invalid_master_data(
        string scenario,
        string expectedSafeCode)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.CounterpartyCards.Add(new CounterpartyCard
        {
            Id = 201, OrganizationId = 11, CounterpartyTypeId = 1, ShortName = "Wrong supplier",
            Inn = "399999999", StateId = StateIdConst.ACTIVE, IsSupplier = true, CreatedDate = Now
        });
        context.Contracts.Add(new Contract
        {
            Id = 202, OrganizationId = 11, CounterpartyId = 200, ContractTypeId = 1,
            ContractNumber = "EXPIRED", ContractDate = Now, StartDate = new DateTime(2020, 1, 1),
            EndDate = new DateTime(2020, 12, 31), StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Products.AddRange(
            new Product
            {
                Id = 301, OrganizationId = 11, UnitId = 5, Name = "Wrong MXIK",
                Mxik = "99999999999999999", IsPurchased = true,
                StateId = StateIdConst.ACTIVE, CreatedDate = Now
            },
            new Product
            {
                Id = 302, OrganizationId = 11, UnitId = 5, Name = "Not purchased",
                Mxik = "12345678901234567", IsPurchased = false,
                StateId = StateIdConst.ACTIVE, CreatedDate = Now
            });
        context.VatRates.Add(new VatRate
        {
            Id = 3, Code = "15", Name = "15%", Rate = 15,
            StateId = StateIdConst.ACTIVE, EffectiveFrom = new DateOnly(2020, 1, 1), CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store);
        var request = scenario switch
        {
            "seller" => ValidMappingRequest(counterpartyId: 201),
            "contract" => ValidMappingRequest(contractId: 202),
            "product" => ValidMappingRequest(productId: 301),
            "not-purchased" => ValidMappingRequest(productId: 302),
            "unit" => ValidMappingRequest(unitId: 6),
            "vat" => ValidMappingRequest(vatRateId: 3),
            "currency" => ValidMappingRequest(currencyId: null),
            "warehouse" => ValidMappingRequest(warehouseId: null),
            _ => throw new InvalidOperationException()
        };

        var result = await CreateService(context, new RecordingScheduler())
            .UpdateCandidateMappingAsync(job.Id, candidate.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal(expectedSafeCode, candidate.SafeErrorCode);
        Assert.Equal(0, job.ReadyCount);
        Assert.Equal(1, job.MappingRequiredCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Valid_goods_and_service_mapping_transition_to_ready_and_are_idempotent(
        bool isService)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Products.Single(product => product.Id == 300).IsService = isService;
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store, isService: isService);
        var service = CreateService(context, new RecordingScheduler());
        var request = ValidMappingRequest();

        var first = await service.UpdateCandidateMappingAsync(job.Id, candidate.Id, request);
        var second = await service.UpdateCandidateMappingAsync(job.Id, candidate.Id, request);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
        Assert.Equal(EdoImportMappingStatus.Resolved, candidate.MappingStatus);
        Assert.Null(candidate.SafeErrorCode);
        Assert.Equal(1, job.ReadyCount);
        Assert.Equal(0, job.MappingRequiredCount);
        Assert.Equal(0, job.DuplicateCount);
        Assert.Single(context.EdoImportCandidates);
    }

    [Theory]
    [InlineData(2, 2, true)]
    [InlineData(2, 1, false)]
    public async Task Piece_tracked_mapping_requires_matching_provider_markings(
        int quantity,
        int markingCount,
        bool expectedReady)
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        var store = new EdoImportStore(context);
        var markings = Enumerable.Range(1, markingCount).Select(index => $"MARK-{index}").ToArray();
        var (job, candidate) = await SeedMappingCandidateAsync(
            store,
            quantity: quantity,
            markings: markings);

        var result = await CreateService(context, new RecordingScheduler())
            .UpdateCandidateMappingAsync(job.Id, candidate.Id, ValidMappingRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(
            expectedReady ? EdoImportCandidateStatus.Ready : EdoImportCandidateStatus.MappingRequired,
            candidate.Status);
        Assert.Equal(expectedReady ? null : EdoImportMarkingPolicy.CountMismatch, candidate.SafeErrorCode);
    }

    [Fact]
    public async Task Bulk_resolve_maps_partial_job_recalculates_counts_and_keeps_terminal_candidates_unchanged()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.PurchaseDocs.Add(Purchase(190));
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var (job, mappingCandidate) = await SeedMappingCandidateAsync(
            store,
            jobStatus: EdoImportJobStatus.Partial);
        var duplicate = Candidate(job.Id, "E-DUPLICATE");
        duplicate.ExistingPurchaseId = 190;
        duplicate.DuplicateState = EdoImportDuplicateState.Confirmed;
        duplicate.TransitionTo(EdoImportCandidateStatus.Duplicate, Now);
        await store.AddCandidateAsync(duplicate);
        var imported = Candidate(job.Id, "E-IMPORTED-UNCHANGED");
        imported.ImportedPurchaseId = 190;
        imported.TransitionTo(EdoImportCandidateStatus.Ready, Now);
        imported.TransitionTo(EdoImportCandidateStatus.Importing, Now);
        imported.TransitionTo(EdoImportCandidateStatus.Imported, Now);
        await store.AddCandidateAsync(imported);
        var skipped = Candidate(job.Id, "E-SKIPPED-UNCHANGED");
        skipped.SafeErrorCode = "DOCUMENT_EXCLUDED";
        skipped.TransitionTo(EdoImportCandidateStatus.Skipped, Now);
        await store.AddCandidateAsync(skipped);
        job.DiscoveredCount = 4;
        job.DuplicateCount = 1;
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var first = await service.ResolveMappingsAsync(job.Id);
        var second = await service.ResolveMappingsAsync(job.Id);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(EdoImportCandidateStatus.Ready, mappingCandidate.Status);
        Assert.Equal(EdoImportCandidateStatus.Duplicate, duplicate.Status);
        Assert.Equal(190, duplicate.ExistingPurchaseId);
        Assert.Equal(EdoImportCandidateStatus.Imported, imported.Status);
        Assert.Equal(190, imported.ImportedPurchaseId);
        Assert.Equal(EdoImportCandidateStatus.Skipped, skipped.Status);
        Assert.Equal("DOCUMENT_EXCLUDED", skipped.SafeErrorCode);
        Assert.Equal(1, job.ReadyCount);
        Assert.Equal(0, job.MappingRequiredCount);
        Assert.Equal(1, job.DuplicateCount);
        Assert.Equal(4, job.DiscoveredCount);
    }

    [Fact]
    public async Task Bulk_resolve_prefers_exact_provider_contract_identity_over_overlapping_contracts()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var exact = context.Contracts.Single(item => item.Id == 201);
        exact.ContractNumber = "LOCAL-GENERATED-201";
        exact.ProviderCode = EdoProviderCode.EDOCS.ToString();
        exact.ProviderContractNumber = "52";
        exact.ProviderContractDate = new DateOnly(2026, 7, 31);
        context.Contracts.Add(new Contract
        {
            Id = 203, OrganizationId = 11, CounterpartyId = 200, ContractTypeId = 1,
            ContractNumber = "OVERLAPPING", ContractDate = Now,
            StartDate = new DateTime(2026, 1, 1), StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store);
        candidate.ProviderContractNumber = "52";
        candidate.ProviderContractDate = new DateOnly(2026, 7, 31);
        await store.SaveChangesAsync();

        var result = await CreateService(context, new RecordingScheduler()).ResolveMappingsAsync(job.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, candidate.SelectedContractId);
        Assert.Equal(EdoImportCandidateStatus.Ready, candidate.Status);
    }

    [Fact]
    public async Task Bulk_resolve_preserves_valid_explicit_contract_product_and_accounts_when_marking_remains()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: true);
        context.Contracts.Add(new Contract
        {
            Id = 203, OrganizationId = 11, CounterpartyId = 200, ContractTypeId = 1,
            ContractNumber = "OVERLAPPING", ContractDate = Now,
            StartDate = new DateTime(2026, 1, 1), StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Products.Add(new Product
        {
            Id = 303, OrganizationId = 11, UnitId = 5, Name = "Same MXIK",
            Mxik = "12345678901234567", IsPurchased = true, DefaultVatRateId = 2,
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.ChartAccounts.AddRange(
            new ChartAccount
            {
                Id = 501, OrganizationId = 11, Number = "1010", Name = "Debit",
                StateId = StateIdConst.ACTIVE, CreatedDate = Now
            },
            new ChartAccount
            {
                Id = 502, OrganizationId = 11, Number = "6410", Name = "VAT",
                StateId = StateIdConst.ACTIVE, CreatedDate = Now
            });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(
            store, quantity: 2, markings: ["ONLY-ONE-MARK"]);
        candidate.SelectedCounterpartyId = 200;
        candidate.SelectedContractId = 201;
        candidate.SelectedCurrencyId = 1;
        candidate.SelectedWarehouseId = 12;
        var line = Assert.Single(candidate.Lines);
        line.SelectedProductId = 300;
        line.SelectedUnitId = 5;
        line.SelectedVatRateId = 2;
        line.SelectedDebitAccountId = 501;
        line.SelectedVatAccountId = 502;
        await store.SaveChangesAsync();
        var service = CreateService(context, new RecordingScheduler());

        var first = await service.ResolveMappingsAsync(job.Id);
        var second = await service.ResolveMappingsAsync(job.Id);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(201, candidate.SelectedContractId);
        Assert.Equal(300, line.SelectedProductId);
        Assert.Equal((short)5, line.SelectedUnitId);
        Assert.Equal((short)2, line.SelectedVatRateId);
        Assert.Equal(501, line.SelectedDebitAccountId);
        Assert.Equal(502, line.SelectedVatAccountId);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal(EdoImportMarkingPolicy.CountMismatch, candidate.SafeErrorCode);
    }

    [Fact]
    public async Task Bulk_resolve_rejects_cross_organization_persisted_selections()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.SetUserContext(new ScopedTestUserContext(22));
        context.CounterpartyCards.Add(new CounterpartyCard
        {
            Id = 220, OrganizationId = 22, CounterpartyTypeId = 1, ShortName = "Other supplier",
            Inn = "300000001", StateId = StateIdConst.ACTIVE, IsSupplier = true, CreatedDate = Now
        });
        context.Contracts.Add(new Contract
        {
            Id = 220, OrganizationId = 22, CounterpartyId = 220, ContractTypeId = 1,
            ContractNumber = "OTHER", ContractDate = Now, StartDate = new DateTime(2026, 1, 1),
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Products.Add(new Product
        {
            Id = 320, OrganizationId = 22, UnitId = 5, Name = "Other product",
            Mxik = "12345678901234567", IsPurchased = true, DefaultVatRateId = 2,
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        context.SetUserContext(new TestUserContext());
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store);
        candidate.SelectedCounterpartyId = 220;
        candidate.SelectedContractId = 220;
        var line = Assert.Single(candidate.Lines);
        line.SelectedProductId = 320;
        line.SelectedUnitId = 5;
        line.SelectedVatRateId = 2;
        await store.SaveChangesAsync();

        var result = await CreateService(context, new RecordingScheduler()).ResolveMappingsAsync(job.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(candidate.SelectedCounterpartyId);
        Assert.Null(candidate.SelectedContractId);
        Assert.Null(line.SelectedProductId);
        Assert.Null(line.SelectedUnitId);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal("COUNTERPARTY_MAPPING_REQUIRED", candidate.SafeErrorCode);
    }

    [Fact]
    public async Task Bulk_resolve_keeps_ambiguous_unselected_product_unresolved_and_ready_candidate_unchanged()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        context.Products.Add(new Product
        {
            Id = 303, OrganizationId = 11, UnitId = 5, Name = "Same MXIK",
            Mxik = "12345678901234567", IsPurchased = true, DefaultVatRateId = 2,
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        await context.SaveChangesAsync();
        var store = new EdoImportStore(context);
        var (job, candidate) = await SeedMappingCandidateAsync(store);
        var ready = Candidate(job.Id, "E-READY-PRESERVED");
        ready.SelectedCounterpartyId = 999;
        ready.TransitionTo(EdoImportCandidateStatus.Ready, Now);
        await store.AddCandidateAsync(ready);

        var result = await CreateService(context, new RecordingScheduler()).ResolveMappingsAsync(job.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(candidate.Lines).SelectedProductId);
        Assert.Equal("PRODUCT_MAPPING_REQUIRED", candidate.SafeErrorCode);
        Assert.Equal(EdoImportCandidateStatus.MappingRequired, candidate.Status);
        Assert.Equal(EdoImportCandidateStatus.Ready, ready.Status);
        Assert.Equal(999, ready.SelectedCounterpartyId);
    }

    [Fact]
    public async Task Cross_provider_fingerprint_is_not_used_inside_single_provider_job()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var previousJob = await SeedJobAsync(store, "DIDOX");
        await CreateProcessor(store,
                new StubHistoricalSource(
                    EdoProviderCode.DIDOX,
                    OnePage(EdoProviderCode.DIDOX, "D-PREVIOUS"),
                    ValidDetail("D-PREVIOUS")))
            .ProcessAsync(previousJob.Id, "didox-worker");
        previousJob.TransitionTo(EdoImportJobStatus.Completed, Now.AddMinutes(1));
        await store.SaveChangesAsync();

        var job = await SeedJobAsync(store, "EDOCS");
        var processor = CreateProcessor(store,
            new StubHistoricalSource(
                EdoProviderCode.EDOCS,
                OnePage(EdoProviderCode.EDOCS, "E-CURRENT"),
                ValidDetail("E-CURRENT")));

        await processor.ProcessAsync(job.Id, "test-worker");

        var current = await context.EdoImportCandidates.SingleAsync(item => item.JobId == job.Id);
        Assert.Equal(EdoImportCandidateStatus.Ready, current.Status);
        Assert.Equal(EdoImportDuplicateState.None, current.DuplicateState);
    }

    [Fact]
    public async Task Waiting_auth_provider_maps_job_to_waiting_auth()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "DIDOX");
        var processor = CreateProcessor(store,
            new StubHistoricalSource(EdoProviderCode.DIDOX, FailurePage(
                EdoProviderCode.DIDOX,
                EdoHistoricalReadState.WAITING_AUTH,
                "AUTHENTICATION_REQUIRED")));

        await processor.ProcessAsync(job.Id, "test-worker");

        Assert.Empty(context.EdoImportCandidates);
        Assert.Equal(EdoImportJobStatus.WaitingAuth, job.Status);
        Assert.Equal(EdoImportProviderCheckpointStatus.WaitingAuth, Assert.Single(job.Providers).Status);
    }

    [Fact]
    public async Task Partial_provider_maps_job_to_partial()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "DIDOX");
        var processor = CreateProcessor(store,
            new StubHistoricalSource(EdoProviderCode.DIDOX, FailurePage(
                EdoProviderCode.DIDOX,
                EdoHistoricalReadState.TRANSIENT_FAILURE,
                "TRANSIENT_PROVIDER_FAILURE")));

        await processor.ProcessAsync(job.Id, "test-worker");

        var didox = job.Providers.Single(item => item.ProviderCode == "DIDOX");
        Assert.Equal(EdoImportProviderCheckpointStatus.Partial, didox.Status);
        Assert.NotNull(didox.NextRetryAt);
        Assert.Equal(EdoImportJobStatus.Partial, job.Status);
        Assert.Null(job.LeaseOwner);
    }

    [Fact]
    public async Task Failed_provider_maps_job_to_failed()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "EDOCS");
        var processor = CreateProcessor(store,
            new StubHistoricalSource(EdoProviderCode.EDOCS, FailurePage(
                EdoProviderCode.EDOCS,
                EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE,
                "TERMINAL_PROVIDER_FAILURE")));

        await processor.ProcessAsync(job.Id, "test-worker");

        Assert.Equal(EdoImportProviderCheckpointStatus.Failed, Assert.Single(job.Providers).Status);
        Assert.Equal(EdoImportJobStatus.Failed, job.Status);
    }

    [Fact]
    public async Task Edocs_list_failure_after_progress_is_partial_and_keeps_detail_exclusion_diagnostic()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "EDOCS");
        var firstPage = OnePage(EdoProviderCode.EDOCS, "E-OUTSIDE");
        firstPage = new EdoHistoricalPageResultDto
        {
            ProviderCode = firstPage.ProviderCode,
            Page = 1,
            PageSize = 100,
            ProviderTotal = 2,
            HasNextPage = true,
            NextPage = 2,
            IsCompletenessConfirmed = true,
            State = EdoHistoricalReadState.COMPLETE,
            Items = firstPage.Items
        };
        var source = new SequenceHistoricalSource(
            EdoProviderCode.EDOCS,
            [
                firstPage,
                FailurePage(
                    EdoProviderCode.EDOCS,
                    EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE,
                    "EDOCS_HISTORICAL_LIST_HTTP_422")
            ],
            new EdoHistoricalDetailResultDto
            {
                ProviderCode = EdoProviderCode.EDOCS,
                State = EdoHistoricalReadState.VALIDATION_FAILURE,
                SafeFailureCode = "DOCUMENT_OUTSIDE_DATE_RANGE"
            });

        await CreateProcessor(store, source).ProcessAsync(job.Id, "test-worker");

        var checkpoint = Assert.Single(job.Providers);
        Assert.Equal(1, checkpoint.ScannedCount);
        Assert.Equal(2, checkpoint.CurrentPage);
        Assert.Equal(EdoImportProviderCheckpointStatus.Partial, checkpoint.Status);
        Assert.Equal(EdoImportJobStatus.Partial, job.Status);
        Assert.Equal(
            "EDOCS_HISTORICAL_LIST_HTTP_422_INCOMPLETE_LAST_DETAIL_DOCUMENT_OUTSIDE_DATE_RANGE",
            checkpoint.SafeErrorCode);
        Assert.Equal(0, job.DiscoveredCount);
        Assert.Empty(context.EdoImportCandidates);
    }

    [Fact]
    public async Task Edocs_detail_integrity_failure_is_not_silently_skipped()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "EDOCS");
        var source = new SequenceHistoricalSource(
            EdoProviderCode.EDOCS,
            [OnePage(EdoProviderCode.EDOCS, "E-MISMATCH")],
            new EdoHistoricalDetailResultDto
            {
                ProviderCode = EdoProviderCode.EDOCS,
                State = EdoHistoricalReadState.VALIDATION_FAILURE,
                SafeFailureCode = "PROVIDER_DOCUMENT_IDENTITY_MISMATCH"
            });

        await CreateProcessor(store, source).ProcessAsync(job.Id, "test-worker");

        var checkpoint = Assert.Single(job.Providers);
        Assert.Equal(EdoImportProviderCheckpointStatus.Failed, checkpoint.Status);
        Assert.Equal(EdoImportJobStatus.Failed, job.Status);
        Assert.Equal(
            "EDOCS_DETAIL_VALIDATION_PROVIDER_DOCUMENT_IDENTITY_MISMATCH",
            checkpoint.SafeErrorCode);
        Assert.Empty(context.EdoImportCandidates);
    }

    [Fact]
    public async Task Invalid_detail_line_is_skipped_and_scan_continues_to_valid_item_and_next_page()
    {
        await using var context = CreateContext();
        await SeedMappingsAsync(context, pieceTracked: false);
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "EDOCS");
        var firstPage = new EdoHistoricalPageResultDto
        {
            ProviderCode = EdoProviderCode.EDOCS,
            Page = 1,
            PageSize = 100,
            ProviderTotal = 2,
            HasNextPage = true,
            NextPage = 2,
            IsCompletenessConfirmed = true,
            State = EdoHistoricalReadState.COMPLETE,
            Items =
            [
                SignedInboxItem("E-DUPLICATE-LINE"),
                SignedInboxItem("E-VALID")
            ]
        };
        var secondPage = new EdoHistoricalPageResultDto
        {
            ProviderCode = EdoProviderCode.EDOCS,
            Page = 2,
            PageSize = 100,
            ProviderTotal = 2,
            HasNextPage = false,
            IsCompletenessConfirmed = true,
            State = EdoHistoricalReadState.COMPLETE
        };
        var source = new PerDocumentHistoricalSource(
            EdoProviderCode.EDOCS,
            [firstPage, secondPage],
            request => request.Item.ProviderDocumentId == "E-DUPLICATE-LINE"
                ? ValidationFailure(EdoProviderCode.EDOCS, "PROVIDER_LINE_NUMBER_DUPLICATE")
                : CompleteDetail(EdoProviderCode.EDOCS, ValidDetail("E-VALID")));

        await CreateProcessor(store, source).ProcessAsync(job.Id, "test-worker");

        Assert.Equal([1, 2], source.RequestedPages);
        Assert.Equal(2, Assert.Single(job.Providers).ScannedCount);
        Assert.Equal(1, job.SkippedCount);
        Assert.Equal(1, job.DiscoveredCount);
        Assert.Equal(EdoImportJobStatus.PreflightReady, job.Status);
        Assert.Equal("E-VALID", Assert.Single(context.EdoImportCandidates).ProviderDocumentId);
    }

    [Fact]
    public async Task Resolve_mapping_rejects_duplicate_lines_without_raw_argument_exception()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var lines = ValidDetail("E-DUPLICATE").Lines.ToArray();

        var exception = await Record.ExceptionAsync(() => store.ResolveMappingAsync(
            11,
            "EDOCS",
            "300000001",
            new DateOnly(2026, 7, 31),
            [lines[0], lines[0]]));

        Assert.NotNull(exception);
        Assert.IsNotType<ArgumentException>(exception);
        Assert.Equal("PROVIDER_LINE_NUMBER_DUPLICATE", exception.Message);
    }

    [Fact]
    public async Task Provider_total_larger_than_scanned_count_cannot_complete_preflight()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "EDOCS");
        var truncatedPage = EmptyPage(EdoProviderCode.EDOCS);
        truncatedPage = new EdoHistoricalPageResultDto
        {
            ProviderCode = EdoProviderCode.EDOCS,
            Page = 16,
            PageSize = 100,
            ProviderTotal = 4168,
            HasNextPage = false,
            IsCompletenessConfirmed = true,
            State = EdoHistoricalReadState.COMPLETE
        };

        await CreateProcessor(store, new StubHistoricalSource(EdoProviderCode.EDOCS, truncatedPage))
            .ProcessAsync(job.Id, "test-worker");

        var checkpoint = Assert.Single(job.Providers);
        Assert.Equal(EdoImportProviderCheckpointStatus.Partial, checkpoint.Status);
        Assert.Equal(EdoImportJobStatus.Partial, job.Status);
        Assert.Equal("EDOCS_PAGINATION_INCOMPLETE", checkpoint.SafeErrorCode);
    }

    [Fact]
    public async Task Checkpoint_page_is_used_when_a_crashed_scan_resumes()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "DIDOX");
        var didoxCheckpoint = job.Providers.Single(item => item.ProviderCode == "DIDOX");
        didoxCheckpoint.CurrentPage = 4;
        didoxCheckpoint.Status = EdoImportProviderCheckpointStatus.Partial;
        await store.SaveChangesAsync();
        var didox = new StubHistoricalSource(EdoProviderCode.DIDOX, EmptyPage(EdoProviderCode.DIDOX, 4));

        await CreateProcessor(
            store,
            didox,
            new StubHistoricalSource(EdoProviderCode.EDOCS, EmptyPage(EdoProviderCode.EDOCS)))
            .ProcessAsync(job.Id, "recovery-worker");

        Assert.Equal(4, Assert.Single(didox.RequestedPages));
        Assert.Equal(4, didoxCheckpoint.LastSuccessfulPage);
    }

    [Fact]
    public async Task Didox_unstable_order_runs_one_overlap_rescan_and_keeps_checkpoint()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store, "DIDOX");
        var didoxPage = EmptyPage(EdoProviderCode.DIDOX);
        didoxPage = new EdoHistoricalPageResultDto
        {
            ProviderCode = didoxPage.ProviderCode,
            Page = didoxPage.Page,
            PageSize = didoxPage.PageSize,
            ProviderTotal = 0,
            HasNextPage = false,
            IsCompletenessConfirmed = false,
            RequiresOverlapRescan = true,
            State = EdoHistoricalReadState.PARTIAL
        };
        var didox = new StubHistoricalSource(EdoProviderCode.DIDOX, didoxPage);

        await CreateProcessor(
            store,
            didox,
            new StubHistoricalSource(EdoProviderCode.EDOCS, EmptyPage(EdoProviderCode.EDOCS)))
            .ProcessAsync(job.Id, "test-worker");

        Assert.Equal([1, 1], didox.RequestedPages);
        var checkpoint = job.Providers.Single(item => item.ProviderCode == "DIDOX");
        Assert.Equal(EdoImportProviderCheckpointStatus.Partial, checkpoint.Status);
        Assert.Equal("OVERLAP_RESCAN_APPLIED", checkpoint.SafeErrorCode);
        Assert.Equal(EdoImportJobStatus.Partial, job.Status);
    }

    [Fact]
    public async Task Cancellation_is_persisted_and_recovery_completes_it_without_provider_read()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        job.TransitionTo(EdoImportJobStatus.CancelRequested, Now);
        await store.SaveChangesAsync();
        var didox = new StubHistoricalSource(EdoProviderCode.DIDOX, EmptyPage(EdoProviderCode.DIDOX));
        var edocs = new StubHistoricalSource(EdoProviderCode.EDOCS, EmptyPage(EdoProviderCode.EDOCS));

        await CreateProcessor(store, didox, edocs).ProcessAsync(job.Id, "test-worker");

        Assert.Equal(EdoImportJobStatus.Cancelled, job.Status);
        Assert.Equal(0, didox.PageReadCount + edocs.PageReadCount);
    }

    [Fact]
    public async Task Candidate_pagination_and_organization_scope_are_enforced()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);
        for (var number = 1; number <= 3; number++)
            await store.AddCandidateAsync(new EdoImportCandidate(job.Id, 11, "EDOCS", $"E-{number}", Now));

        var page = await store.GetCandidatesAsync(11, job.Id, 2, 2);

        Assert.Equal(3, page.TotalCount);
        Assert.Single(page.Items);
        Assert.Empty((await store.GetCandidatesAsync(12, job.Id, 1, 20)).Items);
    }

    [Fact]
    public async Task Lease_is_exclusive_until_expiry_and_can_then_be_recovered()
    {
        await using var context = CreateContext();
        var store = new EdoImportStore(context);
        var job = await SeedJobAsync(store);

        Assert.True(await store.TryAcquireLeaseAsync(11, job.Id, "worker-a", Now, Now.AddMinutes(3)));
        Assert.False(await store.TryAcquireLeaseAsync(11, job.Id, "worker-b", Now.AddMinutes(1), Now.AddMinutes(4)));
        Assert.True(await store.TryAcquireLeaseAsync(11, job.Id, "worker-b", Now.AddMinutes(4), Now.AddMinutes(7)));
    }

    private static EdoImportPreflightProcessor CreateProcessor(
        EdoImportStore store,
        params IEdoHistoricalDocumentSource[] sources) => new(
        store,
        new StubRegistry(sources),
        new NoOpUnitOfWork(),
        new NoOpAuditLogService(),
        new FixedTimeProvider(Now),
        NullLogger<EdoImportPreflightProcessor>.Instance);

    private static EdoImportPreflightService CreateService(
        AppDbContext context,
        IEdoImportPreflightScheduler scheduler,
        EdoProviderCode activeProvider = EdoProviderCode.EDOCS) =>
        CreateService(context, scheduler, new MutableActiveProviderResolver(activeProvider));

    private static EdoImportBulkDraftStartRequestDto BulkStart(string expectedImportPlanHash) => new()
    {
        Confirm = true,
        ExpectedImportPlanHash = expectedImportPlanHash,
        BatchSize = 50,
        LineValuesInvalidPolicy = "SKIP",
        MarkingAlreadyUsedPolicy = "MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP"
    };

    private static EdoImportPreflightService CreateService(
        AppDbContext context,
        IEdoImportPreflightScheduler scheduler,
        IActiveEdoProviderResolver activeProviderResolver) =>
        CreateService(context, scheduler, activeProviderResolver, new TestUserContext());

    private static EdoImportPreflightService CreateService(
        AppDbContext context,
        IEdoImportPreflightScheduler scheduler,
        IActiveEdoProviderResolver activeProviderResolver,
        IUserContext userContext,
        IUnitOfWork? unitOfWork = null,
        IEdoHistoricalPurchaseDraftFactory? draftFactory = null,
        IBackgroundOrganizationScope? backgroundOrganizationScope = null) => new(
        NullLogger<EdoImportPreflightService>.Instance,
        unitOfWork ?? new NoOpUnitOfWork(),
        userContext,
        new QueryRepository<OrganizationConfig>(context),
        activeProviderResolver,
        new EdoImportStore(context),
        scheduler,
        new NoOpAuditLogService(),
        new FixedTimeProvider(Now),
        draftFactory,
        backgroundOrganizationScope);

    private static async Task<EdoImportJob> SeedJobAsync(
        EdoImportStore store,
        string providerCode = "EDOCS")
    {
        var job = new EdoImportJob(11, 7, new DateOnly(2026, 1, 1), new DateOnly(2026, 8, 12), Now);
        await store.AddJobAsync(job);
        await store.AddProviderAsync(11, Provider(job.Id, providerCode));
        return job;
    }

    private static async Task<(EdoImportJob Job, EdoImportCandidate Candidate)> SeedMappingCandidateAsync(
        EdoImportStore store,
        bool isService = false,
        decimal quantity = 2,
        IReadOnlyCollection<string>? markings = null,
        string jobStatus = EdoImportJobStatus.PreflightReady)
    {
        var job = await SeedJobAsync(store, "EDOCS");
        var candidate = Candidate(job.Id, "E-MAPPING");
        candidate.Direction = EdoDirection.INBOX.ToString();
        candidate.NormalizedStatus = EdoDocumentStatusCode.SIGNED.ToString();
        candidate.DocumentType = "FACTURA";
        candidate.DocumentNumber = "42";
        candidate.DocumentDate = new DateOnly(2026, 7, 31);
        candidate.SellerTin = "300000001";
        candidate.BuyerTin = "309142275";
        candidate.SellerName = "Supplier";
        candidate.MappingStatus = EdoImportMappingStatus.Partial;
        candidate.SafeErrorCode = "PRODUCT_MAPPING_REQUIRED";
        var line = new EdoImportCandidateLine
        {
            ProviderLineNumber = 1,
            CatalogCode = "12345678901234567",
            IsService = isService,
            Quantity = quantity,
            UnitPrice = 100,
            NetAmount = quantity * 100,
            VatRate = 12,
            VatAmount = quantity * 12,
            TotalAmount = quantity * 112,
            CreatedDate = Now
        };
        foreach (var marking in markings ?? [])
        {
            line.Markings.Add(new EdoImportCandidateMarking
            {
                MarkingNumber = marking,
                ProviderVerificationState = EdoImportMarkingVerificationState.Unverified,
                CreatedDate = Now
            });
        }
        candidate.Lines.Add(line);
        candidate.TransitionTo(EdoImportCandidateStatus.MappingRequired, Now);
        await store.AddCandidateGraphAsync(candidate);

        job.DiscoveredCount = 1;
        job.MappingRequiredCount = 1;
        job.TransitionTo(EdoImportJobStatus.Scanning, Now);
        job.TransitionTo(jobStatus, Now);
        await store.SaveChangesAsync();
        return (job, candidate);
    }

    private static async Task<EdoImportCandidate> SeedReadyImportCandidateAsync(
        EdoImportStore store,
        EdoImportJob job,
        string providerDocumentId,
        string? marking,
        decimal totalAmount)
    {
        var candidate = Candidate(job.Id, providerDocumentId);
        candidate.Direction = EdoDirection.INBOX.ToString();
        candidate.NormalizedStatus = EdoDocumentStatusCode.SIGNED.ToString();
        candidate.DocumentType = "FACTURA";
        candidate.DocumentNumber = $"DOC-{job.DiscoveredCount + 1}";
        candidate.DocumentDate = new DateOnly(2026, 7, 31);
        candidate.SellerTin = "300000001";
        candidate.SellerName = "Supplier";
        candidate.ProviderContractNumber = "C-1";
        candidate.ProviderContractDate = new DateOnly(2026, 1, 1);
        candidate.NetAmount = 100m;
        candidate.VatAmount = totalAmount - 100m;
        candidate.TotalAmount = totalAmount;
        candidate.SelectedCounterpartyId = 200;
        candidate.SelectedContractId = 201;
        candidate.SelectedCurrencyId = 1;
        candidate.SelectedWarehouseId = 12;
        candidate.MappingStatus = EdoImportMappingStatus.Resolved;
        var line = new EdoImportCandidateLine
        {
            ProviderLineNumber = 1,
            CatalogCode = "12345678901234567",
            IsService = false,
            Quantity = 1m,
            UnitPrice = 100m,
            NetAmount = 100m,
            VatRate = 12m,
            VatAmount = totalAmount - 100m,
            TotalAmount = totalAmount,
            SelectedProductId = 300,
            SelectedUnitId = 5,
            SelectedVatRateId = 2,
            MappingStatus = EdoImportMappingStatus.Resolved,
            CreatedDate = Now
        };
        if (marking is not null)
        {
            line.Markings.Add(new EdoImportCandidateMarking
            {
                MarkingNumber = marking,
                ProviderVerificationState = EdoImportMarkingVerificationState.Verified,
                CreatedDate = Now
            });
        }
        candidate.Lines.Add(line);
        candidate.TransitionTo(EdoImportCandidateStatus.Ready, Now);
        await store.AddCandidateGraphAsync(candidate);
        job.DiscoveredCount++;
        job.ReadyCount++;
        if (job.Status == EdoImportJobStatus.Queued)
        {
            job.TransitionTo(EdoImportJobStatus.Scanning, Now);
            job.TransitionTo(EdoImportJobStatus.PreflightReady, Now);
        }
        await store.SaveChangesAsync();
        return candidate;
    }

    private static void AddCandidateMarking(EdoImportCandidate candidate, string marking)
    {
        var line = Assert.Single(candidate.Lines);
        line.Quantity = line.Markings.Count + 1;
        line.Markings.Add(new EdoImportCandidateMarking
        {
            MarkingNumber = marking,
            ProviderVerificationState = EdoImportMarkingVerificationState.Verified,
            CreatedDate = Now
        });
    }

    private static EdoImportDraftFailureApplyRequestDto DraftFailureRequest(
        string failureHash,
        long candidateId,
        string action) => new()
        {
            Confirm = true,
            ExpectedFailureHash = failureHash,
            Items = [new EdoImportDraftFailureApplyItemDto
            {
                CandidateId = candidateId,
                Action = action
            }]
        };

    private static EdoImportCandidate Candidate(long jobId, string providerDocumentId) =>
        new(jobId, 11, "EDOCS", providerDocumentId, Now);

    private static EdoImportCandidate SummaryCandidate(
        long jobId,
        string providerDocumentId,
        string sellerTin,
        string sellerName,
        string status,
        string? safeErrorCode,
        int? counterpartyId = null,
        long? contractId = null,
        short? currencyId = null,
        int? warehouseId = null)
    {
        var candidate = Candidate(jobId, providerDocumentId);
        candidate.SellerTin = sellerTin;
        candidate.SellerName = sellerName;
        candidate.SelectedCounterpartyId = counterpartyId;
        candidate.SelectedContractId = contractId;
        candidate.SelectedCurrencyId = currencyId;
        candidate.SelectedWarehouseId = warehouseId;
        candidate.SafeErrorCode = safeErrorCode;
        candidate.TransitionTo(status, Now);
        return candidate;
    }

    private static EdoImportCandidateLine SummaryLine(
        string catalogCode,
        string? providerProductName = null,
        bool? isService = null,
        string? packageCode = null,
        string? packageName = null,
        decimal? vatRate = null,
        int? productId = null,
        short? unitId = null,
        short? vatRateId = null,
        string mappingStatus = EdoImportMappingStatus.Unresolved) => new()
    {
        ProviderLineNumber = 1,
        CatalogCode = catalogCode,
        ProviderProductName = providerProductName,
        IsService = isService,
        PackageCode = packageCode,
        PackageName = packageName,
        VatRate = vatRate,
        SelectedProductId = productId,
        SelectedUnitId = unitId,
        SelectedVatRateId = vatRateId,
        MappingStatus = mappingStatus,
        CreatedDate = Now
    };

    private static EdoImportCandidateMappingRequestDto ValidMappingRequest(
        int? counterpartyId = 200,
        long? contractId = 201,
        short? currencyId = 1,
        int? warehouseId = 12,
        int? productId = 300,
        short? unitId = 5,
        short? vatRateId = 2) => new()
    {
        CounterpartyId = counterpartyId,
        ContractId = contractId,
        CurrencyId = currencyId,
        WarehouseId = warehouseId,
        Lines =
        [
            new EdoImportCandidateLineMappingRequestDto
            {
                LineNumber = 1,
                ProductId = productId,
                UnitId = unitId,
                VatRateId = vatRateId
            }
        ]
    };

    private static EdoImportJobProvider Provider(long jobId, string code) => new()
    {
        JobId = jobId,
        ProviderCode = code,
        Status = EdoImportProviderCheckpointStatus.Queued,
        CurrentPage = 1,
        PageSize = 100,
        CreatedDate = Now
    };

    private static EdoHistoricalPageResultDto OnePage(EdoProviderCode provider, string id) => new()
    {
        ProviderCode = provider,
        Page = 1,
        PageSize = 100,
        ProviderTotal = 1,
        HasNextPage = false,
        IsCompletenessConfirmed = true,
        State = EdoHistoricalReadState.COMPLETE,
        Items = [new EdoHistoricalDocumentSummaryDto
        {
            ProviderDocumentId = id,
            Direction = EdoDirection.INBOX,
            Status = EdoDocumentStatusCode.SIGNED,
            DocumentType = "FACTURA"
        }]
    };

    private static EdoHistoricalDocumentSummaryDto SignedInboxItem(string id) => new()
    {
        ProviderDocumentId = id,
        Direction = EdoDirection.INBOX,
        Status = EdoDocumentStatusCode.SIGNED,
        DocumentType = "FACTURA"
    };

    private static EdoHistoricalDetailResultDto ValidationFailure(
        EdoProviderCode provider,
        string safeCode) => new()
    {
        ProviderCode = provider,
        State = EdoHistoricalReadState.VALIDATION_FAILURE,
        SafeFailureCode = safeCode
    };

    private static EdoHistoricalDetailResultDto CompleteDetail(
        EdoProviderCode provider,
        EdoHistoricalDocumentDetailDto detail) => new()
    {
        ProviderCode = provider,
        State = EdoHistoricalReadState.COMPLETE,
        IsImportReady = true,
        Document = detail
    };

    private static EdoHistoricalPageResultDto EmptyPage(EdoProviderCode provider, int page = 1) => new()
    {
        ProviderCode = provider,
        Page = page,
        PageSize = 100,
        ProviderTotal = 0,
        HasNextPage = false,
        IsCompletenessConfirmed = true,
        State = EdoHistoricalReadState.COMPLETE
    };

    private static EdoHistoricalPageResultDto FailurePage(
        EdoProviderCode provider,
        EdoHistoricalReadState state,
        string code) => new()
    {
        ProviderCode = provider,
        Page = 1,
        PageSize = 100,
        State = state,
        SafeFailureCode = code,
        RequiresOverlapRescan = true
    };

    private static EdoHistoricalDocumentDetailDto ValidDetail(string id, params string[] markings) => new()
    {
        ProviderDocumentId = id,
        Direction = EdoDirection.INBOX,
        Status = EdoDocumentStatusCode.SIGNED,
        DocumentType = "FACTURA",
        DocumentNumber = "42",
        DocumentDate = new DateOnly(2026, 7, 31),
        Seller = new EdoHistoricalPartyDto { Tin = "300000001", Name = "Supplier" },
        Buyer = new EdoHistoricalPartyDto { Tin = "309142275", Name = "Buyer" },
        NetAmount = 200,
        VatAmount = 24,
        TotalAmount = 224,
        Lines = [new EdoHistoricalDocumentLineDto
        {
            Number = 1,
            CatalogCode = "12345678901234567",
            Quantity = 2,
            UnitPrice = 100,
            NetAmount = 200,
            VatRate = 12,
            VatAmount = 24,
            TotalAmount = 224,
            MarkingNumbers = markings
        }]
    };

    private static EdoHistoricalDocumentDetailDto DetailWithMarkingLine(
        string id,
        decimal quantity,
        bool isService,
        params string[] markings)
    {
        var source = ValidDetail(id, markings);
        var sourceLine = Assert.Single(source.Lines);
        return new EdoHistoricalDocumentDetailDto
        {
            ProviderDocumentId = source.ProviderDocumentId,
            Direction = source.Direction,
            Status = source.Status,
            DocumentType = source.DocumentType,
            DocumentNumber = source.DocumentNumber,
            DocumentDate = source.DocumentDate,
            Seller = source.Seller,
            Buyer = source.Buyer,
            NetAmount = source.NetAmount,
            VatAmount = source.VatAmount,
            TotalAmount = source.TotalAmount,
            Lines =
            [
                new EdoHistoricalDocumentLineDto
                {
                    Number = sourceLine.Number,
                    CatalogCode = sourceLine.CatalogCode,
                    IsService = isService,
                    Quantity = quantity,
                    UnitPrice = sourceLine.UnitPrice,
                    NetAmount = sourceLine.NetAmount,
                    VatRate = sourceLine.VatRate,
                    VatAmount = sourceLine.VatAmount,
                    TotalAmount = sourceLine.TotalAmount,
                    MarkingNumbers = markings
                }
            ]
        };
    }

    private static void SeedPurchaseMarkingOwner(
        AppDbContext context,
        long purchaseId,
        long purchaseLineId,
        IReadOnlyCollection<(int ProductTableId, string Marking)> markings,
        int organizationId,
        int productId)
    {
        context.PurchaseDocs.Add(new PurchaseDoc
        {
            Id = purchaseId,
            OrganizationId = organizationId,
            DocNumber = $"P-{purchaseId}",
            DocDate = Now,
            CounterpartyId = 200,
            WarehouseId = 12,
            CurrencyId = 1,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            ExchangeRate = 1,
            CreatedDate = Now
        });
        context.PurchaseDocProducts.Add(new PurchaseDocProduct
        {
            Id = purchaseLineId,
            OwnerId = purchaseId,
            ProductId = productId,
            Quantity = markings.Count,
            UnitId = 5,
            UnitPrice = 1
        });
        foreach (var (productTableId, marking) in markings)
        {
            context.ProductTables.Add(new ProductTable
            {
                Id = productTableId,
                ProductId = productId,
                MarkingNumber = marking,
                CreatedDate = Now
            });
            context.PurchaseDocTables.Add(new PurchaseDocTable
            {
                Id = productTableId,
                OwnerId = purchaseLineId,
                ProductTableId = productTableId,
                Amount = 1
            });
        }
    }

    private static EdoHistoricalDocumentDetailDto ValidDetailWithSellerTin(string id, string sellerTin)
    {
        var source = ValidDetail(id);
        return new EdoHistoricalDocumentDetailDto
        {
            ProviderDocumentId = source.ProviderDocumentId,
            Direction = source.Direction,
            Status = source.Status,
            DocumentType = source.DocumentType,
            DocumentNumber = source.DocumentNumber,
            DocumentDate = source.DocumentDate,
            Seller = new EdoHistoricalPartyDto { Tin = sellerTin, Name = source.Seller!.Name },
            Buyer = source.Buyer,
            ContractNumber = source.ContractNumber,
            ContractDate = source.ContractDate,
            NetAmount = source.NetAmount,
            VatAmount = source.VatAmount,
            TotalAmount = source.TotalAmount,
            Lines = source.Lines,
            MarkingNumbers = source.MarkingNumbers
        };
    }

    private static EdoHistoricalDocumentDetailDto ValidDetailWithProductName(string id, string productName)
    {
        var source = ValidDetail(id);
        var sourceLine = Assert.Single(source.Lines);
        return new EdoHistoricalDocumentDetailDto
        {
            ProviderDocumentId = source.ProviderDocumentId,
            Direction = source.Direction,
            Status = source.Status,
            DocumentType = source.DocumentType,
            DocumentNumber = source.DocumentNumber,
            DocumentDate = source.DocumentDate,
            Seller = source.Seller,
            Buyer = source.Buyer,
            NetAmount = source.NetAmount,
            VatAmount = source.VatAmount,
            TotalAmount = source.TotalAmount,
            Lines =
            [
                new EdoHistoricalDocumentLineDto
                {
                    Number = sourceLine.Number,
                    CatalogCode = sourceLine.CatalogCode,
                    CatalogName = productName,
                    PackageCode = sourceLine.PackageCode,
                    PackageName = "штука",
                    IsService = sourceLine.IsService,
                    Quantity = sourceLine.Quantity,
                    UnitPrice = sourceLine.UnitPrice,
                    NetAmount = sourceLine.NetAmount,
                    VatRate = sourceLine.VatRate,
                    VatAmount = sourceLine.VatAmount,
                    TotalAmount = sourceLine.TotalAmount,
                    MarkingNumbers = sourceLine.MarkingNumbers
                }
            ]
        };
    }

    private static PurchaseDoc Purchase(long id) => new()
    {
        Id = id,
        OrganizationId = 11,
        DocNumber = $"P-{id}",
        DocDate = Now,
        CounterpartyId = 200,
        WarehouseId = 12,
        CurrencyId = 1,
        StatusId = DocumentStatusIdConst.DRAFT,
        StateId = StateIdConst.ACTIVE,
        ExchangeRate = 1,
        CreatedDate = Now
    };

    private static async Task SeedMappingsAsync(AppDbContext context, bool pieceTracked)
    {
        context.CounterpartyCards.Add(new CounterpartyCard
        {
            Id = 200, OrganizationId = 11, CounterpartyTypeId = 1, ShortName = "Supplier",
            Inn = "300000001", StateId = StateIdConst.ACTIVE, IsSupplier = true, CreatedDate = Now
        });
        context.Contracts.Add(new Contract
        {
            Id = 201, OrganizationId = 11, CounterpartyId = 200, ContractTypeId = 1,
            ContractNumber = "C-1", ContractDate = Now, StartDate = new DateTime(2026, 1, 1),
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Products.Add(new Product
        {
            Id = 300, OrganizationId = 11, UnitId = 5, Name = "Product",
            Mxik = "12345678901234567", IsPieceTracked = pieceTracked,
            IsPurchased = true, DefaultVatRateId = 2,
            StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Units.Add(new Unit
        {
            Id = 5, Code = "796", Name = "Piece", StateId = StateIdConst.ACTIVE
        });
        context.VatRates.Add(new VatRate
        {
            Id = 2, Code = "12", Name = "12%", Rate = 12,
            StateId = StateIdConst.ACTIVE, EffectiveFrom = new DateOnly(2020, 1, 1), CreatedDate = Now
        });
        context.Warehouses.Add(new Warehouse
        {
            Id = 12, OrganizationId = 11, Name = "Main", StateId = StateIdConst.ACTIVE, CreatedDate = Now
        });
        context.Currencies.Add(new Currency
        {
            Id = 1, Code = "UZS", Name = "UZS", StateId = StateIdConst.ACTIVE
        });
        context.OrganizationDefaults.Add(new OrganizationDefault
        {
            Id = 1, OrganizationId = 11, WarehouseId = 12, CreatedDate = Now
        });
        context.OrganizationConfigs.Add(new OrganizationConfig
        {
            OrganizationId = 11, InventoryValuationMethod = "FIFO", FiscalYearStartMonth = 1,
            BaseCurrencyId = 1, AccountingStartDate = new DateOnly(2026, 1, 1)
        });
        await context.SaveChangesAsync();
    }

    private static AppDbContext CreateContext(
        IBackgroundOrganizationScope? backgroundOrganizationScope = null,
        bool setUserContext = true)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"edo-preflight-{Guid.NewGuid():N}")
            .Options;
        var context = new AppDbContext(options, backgroundOrganizationScope);
        if (setUserContext)
            context.SetUserContext(new TestUserContext());
        return context;
    }

    private sealed class StubHistoricalSource(
        EdoProviderCode providerCode,
        EdoHistoricalPageResultDto page,
        EdoHistoricalDocumentDetailDto? detail = null) : IEdoHistoricalDocumentSource
    {
        public EdoProviderCode ProviderCode { get; } = providerCode;
        public int PageReadCount { get; private set; }
        public List<int> RequestedPages { get; } = [];
        public ValueTask<EdoHistoricalSourceReadinessDto> CheckReadinessAsync(EdoHistoricalExecutionContextDto context, CancellationToken ct = default) =>
            ValueTask.FromResult(new EdoHistoricalSourceReadinessDto { ProviderCode = ProviderCode, IsProviderRegistered = true, IsSessionReady = true, State = EdoHistoricalReadState.COMPLETE });
        public Task<EdoHistoricalPageResultDto> ReadInboxPageAsync(EdoHistoricalExecutionContextDto context, EdoHistoricalPageRequestDto request, CancellationToken ct = default)
        {
            PageReadCount++;
            RequestedPages.Add(request.Page);
            return Task.FromResult(page);
        }
        public Task<EdoHistoricalDetailResultDto> ReadDetailAsync(EdoHistoricalExecutionContextDto context, EdoHistoricalDetailRequestDto request, CancellationToken ct = default) =>
            Task.FromResult(new EdoHistoricalDetailResultDto
            {
                ProviderCode = ProviderCode,
                State = detail is null ? EdoHistoricalReadState.VALIDATION_FAILURE : EdoHistoricalReadState.COMPLETE,
                IsImportReady = detail is not null,
                Document = detail
            });
    }

    private sealed class SequenceHistoricalSource(
        EdoProviderCode providerCode,
        IReadOnlyList<EdoHistoricalPageResultDto> pages,
        EdoHistoricalDetailResultDto detail) : IEdoHistoricalDocumentSource
    {
        private int _pageIndex;
        public EdoProviderCode ProviderCode { get; } = providerCode;
        public ValueTask<EdoHistoricalSourceReadinessDto> CheckReadinessAsync(
            EdoHistoricalExecutionContextDto context,
            CancellationToken ct = default) =>
            ValueTask.FromResult(new EdoHistoricalSourceReadinessDto
            {
                ProviderCode = ProviderCode,
                IsProviderRegistered = true,
                IsSessionReady = true,
                State = EdoHistoricalReadState.COMPLETE
            });
        public Task<EdoHistoricalPageResultDto> ReadInboxPageAsync(
            EdoHistoricalExecutionContextDto context,
            EdoHistoricalPageRequestDto request,
            CancellationToken ct = default) =>
            Task.FromResult(pages[_pageIndex++]);
        public Task<EdoHistoricalDetailResultDto> ReadDetailAsync(
            EdoHistoricalExecutionContextDto context,
            EdoHistoricalDetailRequestDto request,
            CancellationToken ct = default) => Task.FromResult(detail);
    }

    private sealed class PerDocumentHistoricalSource(
        EdoProviderCode providerCode,
        IReadOnlyList<EdoHistoricalPageResultDto> pages,
        Func<EdoHistoricalDetailRequestDto, EdoHistoricalDetailResultDto> detailFactory)
        : IEdoHistoricalDocumentSource
    {
        private int _pageIndex;
        public EdoProviderCode ProviderCode { get; } = providerCode;
        public List<int> RequestedPages { get; } = [];

        public ValueTask<EdoHistoricalSourceReadinessDto> CheckReadinessAsync(
            EdoHistoricalExecutionContextDto context,
            CancellationToken ct = default) =>
            ValueTask.FromResult(new EdoHistoricalSourceReadinessDto
            {
                ProviderCode = ProviderCode,
                IsProviderRegistered = true,
                IsSessionReady = true,
                State = EdoHistoricalReadState.COMPLETE
            });

        public Task<EdoHistoricalPageResultDto> ReadInboxPageAsync(
            EdoHistoricalExecutionContextDto context,
            EdoHistoricalPageRequestDto request,
            CancellationToken ct = default)
        {
            RequestedPages.Add(request.Page);
            return Task.FromResult(pages[_pageIndex++]);
        }

        public Task<EdoHistoricalDetailResultDto> ReadDetailAsync(
            EdoHistoricalExecutionContextDto context,
            EdoHistoricalDetailRequestDto request,
            CancellationToken ct = default) => Task.FromResult(detailFactory(request));
    }

    private sealed class StubRegistry(IEnumerable<IEdoHistoricalDocumentSource> sources) : IEdoProviderRegistry
    {
        private readonly IReadOnlyDictionary<EdoProviderCode, IEdoHistoricalDocumentSource> _sources =
            sources.ToDictionary(source => source.ProviderCode);
        public IReadOnlyCollection<EdoProviderCapabilityDto> GetProviders() => [];
        public IEdoProvider Resolve(EdoProviderCode providerCode) => throw new NotSupportedException();
        public IEdoHistoricalDocumentSource ResolveHistoricalSource(EdoProviderCode providerCode) => _sources[providerCode];
    }

    private sealed class MutableActiveProviderResolver(EdoProviderCode providerCode) : IActiveEdoProviderResolver
    {
        public EdoProviderCode ProviderCode { get; set; } = providerCode;
        public Task<EdoProviderCode> GetActiveProviderCodeAsync(CancellationToken ct = default) =>
            Task.FromResult(ProviderCode);
        public Task<IEdoProvider> GetActiveProviderAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task SetActiveProviderAsync(EdoProviderCode providerCode, CancellationToken ct = default)
        {
            ProviderCode = providerCode;
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class NoOpDocumentPostingLock : IDocumentPostingLock
    {
        public Task AcquireAsync(
            short documentTypeId,
            long documentId,
            CancellationToken ct = default) => Task.CompletedTask;

        public Task<bool> TryAcquireAsync(
            short documentTypeId,
            long documentId,
            CancellationToken ct = default) => Task.FromResult(true);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int RollbackCount { get; private set; }
        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default)
        {
            RollbackCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpAuditLogService : IAuditLogService
    {
        public void SetOldValues(object oldValues) { }
        public void SetNewValues(object newValues) { }
        public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null, int? organizationId = null) => Task.CompletedTask;
        public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
    }

    private sealed class RecordingHistoricalDraftFactory(
        AppDbContext context,
        string? failDocumentIdentity = null,
        string failErrorCode = "TEST_VALIDATION_FAILURE",
        IBackgroundOrganizationScope? backgroundOrganizationScope = null,
        IReadOnlyCollection<string>? failureCodes = null) : IEdoHistoricalPurchaseDraftFactory
    {
        private readonly Queue<string> _failureCodes = new(failureCodes ??
            (failDocumentIdentity is null ? [] : [failErrorCode]));
        public List<long> CandidateOrder { get; } = [];
        public int? ObservedOrganizationId { get; private set; }

        public async Task<Result<PurchaseDocDto>> CreateFromHistoricalSnapshotAsync(
            EdoDocumentDto document,
            PurchaseDocFromEdoRequestDto request,
            CancellationToken ct = default)
        {
            ObservedOrganizationId = backgroundOrganizationScope?.OrganizationId;
            var candidateId = await context.EdoImportCandidates.IgnoreQueryFilters()
                .Where(candidate => candidate.ProviderCode == document.ProviderCode.ToString()
                    && candidate.ProviderDocumentId == document.ProviderDocumentId)
                .OrderByDescending(candidate => candidate.Id)
                .Select(candidate => candidate.Id)
                .FirstAsync(ct);
            CandidateOrder.Add(candidateId);
            if (document.ProviderDocumentId == failDocumentIdentity
                && _failureCodes.TryDequeue(out var failureCode))
                return Result.Failure<PurchaseDocDto>(Error.Business(
                    failureCode,
                    "Controlled test validation failure."));

            var previewByNumber = document.PreviewLines.ToDictionary(line => line.Number);
            var purchase = new PurchaseDoc
            {
                Id = 10000 + candidateId,
                OrganizationId = 11,
                DocNumber = $"DRAFT-{CandidateOrder.Count}",
                DocDate = document.DocumentDate!.Value.ToDateTime(TimeOnly.MinValue),
                CounterpartyId = request.CounterpartyId,
                ContractId = request.ContractId,
                WarehouseId = request.WarehouseId,
                CurrencyId = request.CurrencyId,
                ExchangeRate = 1,
                TotalAmount = document.PreviewLines.Sum(line => line.NetAmount ?? 0m),
                VatAmount = document.PreviewLines.Sum(line => line.VatAmount ?? 0m),
                FinalAmount = document.PreviewLines.Sum(line => line.TotalWithVat ?? 0m),
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = Now,
                PurchaseDocProducts = request.Lines.Select(line =>
                {
                    var source = previewByNumber[line.LineNumber];
                    return new PurchaseDocProduct
                    {
                        ProductId = line.ProductId,
                        UnitId = line.UnitId,
                        Quantity = source.Quantity!.Value,
                        UnitPrice = source.UnitPrice!.Value,
                        Amount = source.NetAmount ?? 0m,
                        VatRateId = line.VatRateId,
                        VatAmount = source.VatAmount ?? 0m,
                        TotalAmount = source.TotalWithVat!.Value,
                        DebitAccountId = line.DebitAccountId,
                        VatAccountId = line.VatAccountId,
                        PurchaseDocTables = line.Items.Select(item => new PurchaseDocTable
                        {
                            Amount = source.UnitPrice.Value,
                            VatRateId = line.VatRateId,
                            ProductTable = new ProductTable
                            {
                                ProductId = line.ProductId,
                                MarkingNumber = item.MarkingNumber,
                                CreatedDate = Now
                            }
                        }).ToList()
                    };
                }).ToList()
            };
            context.PurchaseDocs.Add(purchase);
            context.EdoDocuments.Add(new EdoDocument
            {
                OrganizationId = 11,
                Provider = document.ProviderCode.ToString(),
                Direction = EdoDirection.INBOX.ToString(),
                InternalDocumentType = "PURCHASE",
                InternalDocumentId = purchase.Id,
                ProviderDocumentId = document.ProviderDocumentId,
                DocumentType = document.DocumentType,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                Status = EdoDocumentStatusCode.SIGNED.ToString(),
                OperationType = "PURCHASE_FROM_EDO",
                CreatedAt = Now
            });
            return Result.Success(new PurchaseDocDto
            {
                Id = purchase.Id,
                OrganizationId = purchase.OrganizationId,
                StatusId = purchase.StatusId
            });
        }
    }

    private sealed class RecordingScheduler : IEdoImportPreflightScheduler
    {
        public List<long> JobIds { get; } = [];
        public List<long> BulkJobIds { get; } = [];
        public Task ScheduleAsync(long jobId, CancellationToken ct = default)
        {
            JobIds.Add(jobId);
            return Task.CompletedTask;
        }

        public Task ScheduleBulkImportAsync(long jobId, CancellationToken ct = default)
        {
            BulkJobIds.Add(jobId);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now, TimeSpan.Zero);
    }

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 7;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => 1;
        public int? TenantId => 1;
        public int? OrganizationId => 11;
        public List<int> AllowedOrganizationIds => [11];
        public int? BranchId => null;
    }

    private sealed class TestOrganizationSourceReader(IReadOnlyDictionary<int, string> inns)
        : IOrganizationSourceReader
    {
        public Task<int?> GetProductOrganizationIdAsync(int productId, CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<int?> GetCounterpartyOrganizationIdAsync(int counterpartyId, CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<int?> GetProductTableOrganizationIdAsync(int productTableId, CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<string?> GetOrganizationInnAsync(int organizationId, CancellationToken ct = default) =>
            Task.FromResult(inns.TryGetValue(organizationId, out var inn) ? inn : null);
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "IntegrationTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class TestHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new TestHttpMessageHandler(responder))
        {
            BaseAddress = new Uri("https://provider.test/")
        };
    }

    private sealed class TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(responder(request));
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class ScopedTestUserContext(int organizationId) : IUserContext
    {
        public int? Id => 7;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => 1;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
    }
}
