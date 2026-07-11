using System.Net;
using Application.Abstractions;
using Application.Features.Register;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Constants;

namespace IntegrationTests;

public class FaReceiptPostingIntegrationTests
{
    private const long DocumentId = 7001;
    private const long LineId = 70011;
    private const long ReceiptAssetId = 700111;
    private static readonly DateTime DocDate = new(2026, 7, 6, 0, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task FaReceiptConfirm_ShouldCreateExpectedPostingEntries_AndFixedAssetSubkonto()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddAuthHeaders(client);

        var response = await client.PutAsync($"/api/fa-receipts/{DocumentId}/confirm", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);

        var receipt = await db.FaReceiptDocs
            .AsNoTracking()
            .Include(x => x.Lines)
            .ThenInclude(x => x.Assets)
            .SingleAsync(x => x.Id == DocumentId);

        var createdAssetId = receipt.Lines.Single().Assets.Single().FaAssetId;
        Assert.NotNull(createdAssetId);
        Assert.Equal(DocumentStatusIdConst.POSTED, receipt.StatusId);

        var asset = await db.FaAssets
            .AsNoTracking()
            .SingleAsync(x => x.Id == createdAssetId!.Value);

        Assert.Equal(FaAssetStatusIdConst.ACTIVE, asset.StatusId);
        Assert.Equal(StateIdConst.ACTIVE, asset.StateId);

        var entries = await db.AccountingRegisterEntries
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                        x.DocumentId == DocumentId &&
                        x.ReversalEntryId == null)
            .Include(x => x.RegisterEntrySubkontos)
            .ToListAsync();

        Assert.Equal(3, entries.Count);
        Assert.Equal(212m, entries.Sum(x => x.Amount));
        Assert.Equal(entries.Where(x => x.DebitAccountId.HasValue).Sum(x => x.Amount),
                     entries.Where(x => x.CreditAccountId.HasValue).Sum(x => x.Amount));

        var accountCodes = await db.ChartAccounts
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Code);

        var capitalizationEntry = GetEntry(entries, accountCodes, "0820", "6010");
        Assert.Equal(100m, capitalizationEntry.Amount);
        Assert.Contains(capitalizationEntry.RegisterEntrySubkontos,
            x => x.SubkontoTypeId == SubkontoTypeIdConst.FixedAssets &&
                 x.EntityId == createdAssetId &&
                 x.Side == SubkontoSideConst.DEBIT);

        var vatEntry = GetEntry(entries, accountCodes, "4410.1", "6010");
        Assert.Equal(12m, vatEntry.Amount);
        Assert.DoesNotContain(vatEntry.RegisterEntrySubkontos,
            x => x.SubkontoTypeId == SubkontoTypeIdConst.FixedAssets);

        var commissioningEntry = GetEntry(entries, accountCodes, "0190", "0820");
        Assert.Equal(100m, commissioningEntry.Amount);
        Assert.Contains(commissioningEntry.RegisterEntrySubkontos,
            x => x.SubkontoTypeId == SubkontoTypeIdConst.FixedAssets &&
                 x.EntityId == createdAssetId &&
                 x.Side == SubkontoSideConst.DEBIT);
        Assert.Contains(commissioningEntry.RegisterEntrySubkontos,
            x => x.SubkontoTypeId == SubkontoTypeIdConst.FixedAssets &&
                 x.EntityId == createdAssetId &&
                 x.Side == SubkontoSideConst.CREDIT);
    }

    [Fact]
    public async Task FaReceiptCancel_ShouldCreateReversalEntries_AndReverseFixedAssetSubkontoSides()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddAuthHeaders(client);

        var confirmResponse = await client.PutAsync($"/api/fa-receipts/{DocumentId}/confirm", content: null);
        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);

        var cancelResponse = await client.PutAsync($"/api/fa-receipts/{DocumentId}/cancel", content: null);
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SetGlobalUserContext(db);

        var receipt = await db.FaReceiptDocs
            .AsNoTracking()
            .Include(x => x.Lines)
            .ThenInclude(x => x.Assets)
            .SingleAsync(x => x.Id == DocumentId);

        var createdAssetId = receipt.Lines.Single().Assets.Single().FaAssetId;
        Assert.NotNull(createdAssetId);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, receipt.StatusId);

        var asset = await db.FaAssets
            .AsNoTracking()
            .SingleAsync(x => x.Id == createdAssetId!.Value);

        Assert.Equal(FaAssetStatusIdConst.DRAFT, asset.StatusId);
        Assert.Equal(StateIdConst.PASSIVE, asset.StateId);
        Assert.Null(asset.CommissioningDate);
        Assert.Null(asset.DeprStartDate);

        var originalEntries = await db.AccountingRegisterEntries
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                        x.DocumentId == DocumentId &&
                        x.ReversalEntryId == null)
            .Include(x => x.RegisterEntrySubkontos)
            .ToListAsync();

        var reversalEntries = await db.AccountingRegisterEntries
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                        x.DocumentId == DocumentId &&
                        x.ReversalEntryId != null)
            .Include(x => x.RegisterEntrySubkontos)
            .ToListAsync();

        Assert.Equal(3, originalEntries.Count);
        Assert.Equal(3, reversalEntries.Count);
        Assert.Equal(originalEntries.Sum(x => x.Amount), reversalEntries.Sum(x => x.Amount));

        foreach (var original in originalEntries)
        {
            var reversal = reversalEntries.Single(x => x.ReversalEntryId == original.Id);

            Assert.Equal(original.DebitAccountId, reversal.CreditAccountId);
            Assert.Equal(original.CreditAccountId, reversal.DebitAccountId);
            Assert.Equal(original.Amount, reversal.Amount);

            var originalFixedAssetSubkontos = original.RegisterEntrySubkontos
                .Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.FixedAssets)
                .ToList();
            var reversalFixedAssetSubkontos = reversal.RegisterEntrySubkontos
                .Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.FixedAssets)
                .ToList();

            Assert.Equal(originalFixedAssetSubkontos.Count, reversalFixedAssetSubkontos.Count);

            foreach (var originalSubkonto in originalFixedAssetSubkontos)
            {
                Assert.Contains(reversalFixedAssetSubkontos, reversalSubkonto =>
                    reversalSubkonto.EntityId == originalSubkonto.EntityId &&
                    reversalSubkonto.Side == ReverseSide(originalSubkonto.Side));
            }
        }

        var batches = await db.PostingBatches
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                        x.DocumentId == DocumentId)
            .ToListAsync();

        Assert.Contains(batches, x => x.Status == PostingBatchStatusConst.REVERSED);
        Assert.Contains(batches, x => x.Status == PostingBatchStatusConst.REVERSAL);
    }

    private static TestWebApplicationFactory CreateFactory()
    {
        return new TestWebApplicationFactory(
            overrideServices: services =>
            {
                services.RemoveAll(typeof(IDocumentPostingLock));
                services.AddScoped<IDocumentPostingLock, NoOpDocumentPostingLock>();
            },
            seedDatabase: SeedFaPostingData);
    }

    private static void SeedFaPostingData(AppDbContext db)
    {
        SetGlobalUserContext(db);

        db.States.Add(new State
        {
            Id = StateIdConst.ACTIVE,
            ShortName = "Active",
            FullName = "Active",
            CreatedDate = DocDate
        });

        db.DocumentStatuses.AddRange(
            new DocumentStatus { Id = DocumentStatusIdConst.DRAFT, Code = "DRAFT", Name = "Draft", StateId = StateIdConst.ACTIVE },
            new DocumentStatus { Id = DocumentStatusIdConst.POSTED, Code = "POSTED", Name = "Posted", StateId = StateIdConst.ACTIVE },
            new DocumentStatus { Id = DocumentStatusIdConst.CANCELLED, Code = "CANCELLED", Name = "Cancelled", StateId = StateIdConst.ACTIVE },
            new DocumentStatus { Id = DocumentStatusIdConst.PENDING, Code = "PENDING", Name = "Pending", StateId = StateIdConst.ACTIVE });

        db.FaAssetStatuses.AddRange(
            new FaAssetStatus { Id = FaAssetStatusIdConst.DRAFT, Code = "DRAFT", Name = "Draft", StateId = StateIdConst.ACTIVE },
            new FaAssetStatus { Id = FaAssetStatusIdConst.ACTIVE, Code = "ACTIVE", Name = "Active", StateId = StateIdConst.ACTIVE },
            new FaAssetStatus { Id = FaAssetStatusIdConst.DISPOSED, Code = "DISPOSED", Name = "Disposed", StateId = StateIdConst.ACTIVE });

        db.Organizations.Add(new Organization
        {
            Id = 1,
            ShortName = "FA Test Org",
            FullName = "FA Test Organization",
            Inn = "123456789",
            RegionId = 1,
            IsParent = false,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DocDate,
            SetupStatus = "COMPLETED"
        });

        db.Currencies.Add(new Currency
        {
            Id = 1,
            Code = "UZS",
            Name = "Uzbek sum",
            StateId = StateIdConst.ACTIVE
        });

        db.VatRates.Add(new VatRate
        {
            Id = 1,
            Code = "VAT12",
            Name = "VAT 12%",
            Rate = 12m,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DocDate
        });

        db.FaDepreciationMethods.AddRange(
            new FaDepreciationMethod
            {
                Id = 1,
                Code = FaDepreciationMethodCodeConst.LINEAR,
                Name = "Linear",
                StateId = StateIdConst.ACTIVE
            },
            new FaDepreciationMethod
            {
                Id = 2,
                Code = FaDepreciationMethodCodeConst.DECLINING_BALANCE,
                Name = "Declining balance",
                StateId = StateIdConst.ACTIVE
            },
            new FaDepreciationMethod
            {
                Id = 3,
                Code = FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION,
                Name = "Units of production",
                StateId = StateIdConst.ACTIVE
            });

        db.FaGroups.Add(new FaGroup
        {
            Id = 1,
            OrganizationId = 1,
            Code = "TECH",
            Name = "Equipment",
            StateId = StateIdConst.ACTIVE
        });

        db.AccountingPeriods.Add(new AccountingPeriod
        {
            Id = 1,
            OrganizationId = 1,
            Year = 2026,
            Month = 7,
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 7, 31),
            IsClosed = false,
            CreatedDate = DocDate
        });

        db.ChartAccounts.AddRange(
            CreateChartAccount(1018, "4410.1", "VAT for fixed assets"),
            CreateChartAccount(1035, "9430", "Other operating expense"),
            CreateChartAccount(1036, "6010", "Supplier settlements"),
            CreateChartAccount(1073, "0100", "Fixed assets", isGroup: true),
            CreateChartAccount(1074, "0190", "Other fixed assets", parentId: 1073),
            CreateChartAccount(1075, "0200", "Accumulated depreciation", isGroup: true),
            CreateChartAccount(1076, "0290", "Depreciation of other fixed assets", parentId: 1075),
            CreateChartAccount(1077, "0800", "Capital investments", isGroup: true),
            CreateChartAccount(1078, "0820", "Fixed asset acquisition", parentId: 1077));

        db.PostingAliases.AddRange(
            CreatePostingAlias(3, AliasConst.Supplier, "Supplier"),
            CreatePostingAlias(9, AliasConst.VATIn, "VAT in"),
            CreatePostingAlias(35, AliasConst.FixedAsset, "Fixed asset"),
            CreatePostingAlias(36, AliasConst.FixedAssetInProgress, "Fixed asset in progress"),
            CreatePostingAlias(37, AliasConst.FixedAssetDepreciation, "Fixed asset depreciation"),
            CreatePostingAlias(38, AliasConst.FixedAssetExpense, "Fixed asset expense"),
            CreatePostingAlias(39, AliasConst.FixedAssetDisposalLoss, "Fixed asset disposal loss"));

        db.SubkontoTypes.Add(new SubkontoType
        {
            Id = SubkontoTypeIdConst.FixedAssets,
            Code = "fixed_asset",
            Name = "Fixed asset",
            SourceTable = "fa_asset",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DocDate
        });

        db.PostingRules.Add(new PostingRule
        {
            Id = PostingRuleIdConst.FA_RECEIPT,
            Code = "FA_RECEIPT",
            Name = "Fixed asset receipt"
        });

        db.PostingRuleLines.AddRange(
            CreatePostingRuleLine(25, 10, 1, 36, 3, AmountSourceConst.Base, false),
            CreatePostingRuleLine(26, 10, 2, 9, 3, AmountSourceConst.VAT, true),
            CreatePostingRuleLine(27, 10, 3, 35, 36, AmountSourceConst.Base, false));

        db.Set<AccountResolveRule>().AddRange(
            CreateResolveRule(1, 1, AliasConst.FixedAsset, RegisterDefaultsConst.DefaultDimensionValue, 1074),
            CreateResolveRule(2, 1, AliasConst.FixedAssetInProgress, RegisterDefaultsConst.DefaultDimensionValue, 1078),
            CreateResolveRule(3, 1, AliasConst.FixedAssetDepreciation, RegisterDefaultsConst.DefaultDimensionValue, 1076),
            CreateResolveRule(4, 1, AliasConst.FixedAssetExpense, RegisterDefaultsConst.DefaultDimensionValue, 1035),
            CreateResolveRule(5, 1, AliasConst.FixedAssetDisposalLoss, RegisterDefaultsConst.DefaultDimensionValue, 1035),
            CreateResolveRule(6, 1, AliasConst.VATIn, RegisterDefaultsConst.VatKindFixedAsset, 1018),
            CreateResolveRule(7, 1, AliasConst.Supplier, RegisterDefaultsConst.DefaultDimensionValue, 1036));

        db.FaReceiptDocs.Add(new FaReceiptDoc
        {
            Id = DocumentId,
            OrganizationId = 1,
            StateId = StateIdConst.ACTIVE,
            DocNumber = "FA-2026-000001",
            DocDate = DocDate,
            CurrencyId = 1,
            TotalAmount = 100m,
            VatAmount = 12m,
            FinalAmount = 112m,
            StatusId = DocumentStatusIdConst.DRAFT,
            ReceiptType = FaReceiptTypeConst.PURCHASE,
            CreatedDate = DocDate,
            UpdatedDate = DocDate,
            Lines =
            [
                new FaReceiptDocLine
                {
                    Id = LineId,
                    OwnerId = DocumentId,
                    Name = "Industrial laptop",
                    Quantity = 1m,
                    Price = 100m,
                    Amount = 100m,
                    VatRateId = 1,
                    VatAmount = 12m,
                    TotalAmount = 112m,
                    Assets =
                    [
                        new FaReceiptDocAsset
                        {
                            Id = ReceiptAssetId,
                            OwnerId = LineId,
                            InventoryNumber = "FA-INV-001",
                            Name = "Industrial laptop",
                            InitialCost = 100m,
                            SalvageValue = 0m,
                            UsefulLifeMonths = 60,
                            DepreciationMethodId = 1,
                            FaGroupId = 1,
                            CommissioningDate = DocDate,
                            DeprStartDate = DocDate
                        }
                    ]
                }
            ]
        });

        db.SaveChanges();
    }

    private static ChartAccount CreateChartAccount(int id, string code, string name, bool isGroup = false, int? parentId = null) =>
        new()
        {
            Id = id,
            ParentId = parentId,
            Code = code,
            Number = code,
            Name = name,
            IsGroup = isGroup,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DocDate,
            IsQuantity = false,
            IsCurrency = false
        };

    private static PostingAlias CreatePostingAlias(short id, string code, string name) =>
        new()
        {
            Id = id,
            Code = code,
            Name = name
        };

    private static PostingRuleLine CreatePostingRuleLine(
        int id,
        short templateId,
        short orderNumber,
        short debitAliasId,
        short creditAliasId,
        string amountSource,
        bool isOptional) =>
        new()
        {
            Id = id,
            TemplateId = templateId,
            OrderNumber = orderNumber,
            DebitAliasId = debitAliasId,
            CreditAliasId = creditAliasId,
            AmountSource = amountSource,
            IsOptional = isOptional
        };

    private static AccountResolveRule CreateResolveRule(int id, short policyId, string alias, string dimensionValue, int accountId) =>
        new()
        {
            Id = id,
            PolicyId = policyId,
            Alias = alias,
            DimensionKey = "_none",
            DimensionValue = dimensionValue,
            AccountId = accountId,
            Priority = 100
        };

    private static AccountingRegisterEntry GetEntry(
        IEnumerable<AccountingRegisterEntry> entries,
        IReadOnlyDictionary<int, string> accountCodes,
        string debitCode,
        string creditCode)
    {
        return entries.Single(x =>
            x.DebitAccountId.HasValue &&
            x.CreditAccountId.HasValue &&
            accountCodes[x.DebitAccountId.Value] == debitCode &&
            accountCodes[x.CreditAccountId.Value] == creditCode);
    }

    private static string ReverseSide(string side) =>
        side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT : SubkontoSideConst.DEBIT;

    private static void AddAuthHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-Test-OrgId", "1");
        client.DefaultRequestHeaders.Add("X-Test-GlobalAccess", "true");
    }

    private static void SetGlobalUserContext(AppDbContext db)
    {
        db.SetUserContext(new FakeUserContext
        {
            Id = 101,
            RoleId = 1,
            OrganizationId = 1,
            AllowedOrganizationIds = [1],
            HasGlobalAccess = true
        });
    }
}

file sealed class NoOpDocumentPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) =>
        Task.FromResult(true);
}
