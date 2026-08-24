using Application.Features.Integration.Edo.UnifiedImport;
using Domain.Entities;

public sealed class EdoUnifiedImportIdempotencyTests
{
    [Fact]
    public void SameMappingProducesSameKeyRegardlessOfInputOrder()
    {
        var first = Request(reverse: false, reverseProductTables: false);
        var second = Request(reverse: true, reverseProductTables: true);

        Assert.Equal(
            EdoUnifiedImportIdempotency.Compute(2, first),
            EdoUnifiedImportIdempotency.Compute(2, second));
    }

    [Fact]
    public void ProductTableChangeProducesDifferentKey()
    {
        var original = Request(reverse: false);
        var changed = Request(reverse: false, productTableIds: [100, 103]);

        Assert.NotEqual(
            EdoUnifiedImportIdempotency.Compute(2, original),
            EdoUnifiedImportIdempotency.Compute(2, changed));
    }

    [Fact]
    public void EveryMappingFieldParticipatesInTheKey()
    {
        var original = Request(reverse: false);
        var changed = Request(reverse: false, productId: 999, costPrice: 12.5m, markingSource: "NONE");

        Assert.NotEqual(
            EdoUnifiedImportIdempotency.Compute(2, original),
            EdoUnifiedImportIdempotency.Compute(2, changed));
    }

    [Fact]
    public void OldFailedBatchKeyDoesNotBlockCorrectedMapping()
    {
        var oldRequest = Request(reverse: false, productTableIds: [100]);
        var correctedRequest = Request(reverse: false, productTableIds: [100, 101]);
        var oldKey = EdoUnifiedImportIdempotency.Compute(2, oldRequest);
        var correctedKey = EdoUnifiedImportIdempotency.Compute(2, correctedRequest);
        var oldBatch = new EdoImportBatch(2, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", oldKey, DateTime.UtcNow);
        oldBatch.SetStatus(EdoImportBatchStatus.Failed, DateTime.UtcNow);

        Assert.Equal(EdoImportBatchStatus.Failed, oldBatch.Status);
        Assert.NotEqual(oldKey, correctedKey);
    }

    [Fact]
    public void SentOverrideAndDocumentTypeParticipateInIdempotencyKey()
    {
        var factura = Request(reverse: false);
        var sentOverride = new EdoUnifiedImportApplyRequestDto
        {
            Confirm = factura.Confirm,
            ExpectedPlanHash = factura.ExpectedPlanHash,
            AllowSentDocuments = true,
            Items = factura.Items.Select(x => new EdoUnifiedImportApplyItemDto
            {
                ProviderDocumentId = x.ProviderDocumentId,
                Direction = x.Direction,
                DocumentType = "waybillLocal",
                CounterpartyId = x.CounterpartyId,
                ContractId = x.ContractId,
                CurrencyId = x.CurrencyId,
                WarehouseId = x.WarehouseId,
                ExchangeRate = x.ExchangeRate,
                Comment = x.Comment,
                Lines = x.Lines
            }).ToArray()
        };

        Assert.NotEqual(
            EdoUnifiedImportIdempotency.Compute(2, factura),
            EdoUnifiedImportIdempotency.Compute(2, sentOverride));
    }

    private static EdoUnifiedImportApplyRequestDto Request(
        bool reverse,
        IReadOnlyCollection<int>? productTableIds = null,
        int productId = 10,
        decimal costPrice = 10m,
        string markingSource = "PROVIDER_SNAPSHOT",
        bool reverseProductTables = false)
    {
        var lines = new[]
        {
            new EdoUnifiedImportApplyLineDto
            {
                LineNumber = 2,
                ProductId = productId,
                Quantity = 2m,
                UnitPrice = 25.25m,
                UnitId = 1,
                VatRateId = 12,
                CostPrice = costPrice,
                CostPriceSource = "STOCK",
                MarkingSource = markingSource,
                ProductTableIds = productTableIds ?? (reverseProductTables ? [101, 100] : [100, 101])
            },
            new EdoUnifiedImportApplyLineDto
            {
                LineNumber = 1,
                ProductId = 11,
                Quantity = 1m,
                UnitPrice = 5m,
                UnitId = 1,
                VatRateId = 12,
                CostPrice = 4m,
                CostPriceSource = "STOCK",
                MarkingSource = "NONE",
                ProductTableIds = [200]
            }
        };

        var items = new[]
        {
            new EdoUnifiedImportApplyItemDto
            {
                ProviderDocumentId = "doc-b",
                Direction = "OUTBOX",
                CounterpartyId = 7,
                ContractId = 9,
                CurrencyId = 1,
                WarehouseId = 2,
                ExchangeRate = 1m,
                Comment = "draft",
                Lines = reverse ? lines.Reverse().ToArray() : lines
            },
            new EdoUnifiedImportApplyItemDto
            {
                ProviderDocumentId = "doc-a",
                Direction = "INBOX",
                CounterpartyId = 8,
                ContractId = null,
                CurrencyId = 1,
                WarehouseId = 2,
                ExchangeRate = 1m,
                Comment = null,
                Lines = []
            }
        };

        return new EdoUnifiedImportApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
            Items = reverse ? items.Reverse().ToArray() : items
        };
    }
}
