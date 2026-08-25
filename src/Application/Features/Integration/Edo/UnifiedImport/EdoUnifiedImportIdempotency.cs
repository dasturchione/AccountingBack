using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Features.Integration.Edo.UnifiedImport;

public static class EdoUnifiedImportIdempotency
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static string Compute(int organizationId, EdoUnifiedImportApplyRequestDto request)
    {
        var canonical = new CanonicalRequest
        {
            OrganizationId = organizationId,
            ExpectedPlanHash = request.ExpectedPlanHash,
            AllowSentDocuments = request.AllowSentDocuments,
            AllowUnmatchedMarkings = request.AllowUnmatchedMarkings,
            Items = request.Items
                .Select(item => new CanonicalItem
                {
                    ProviderDocumentId = item.ProviderDocumentId,
                    Direction = item.Direction,
                    DocumentType = EdoUnifiedImportPlanRules.NormalizeDocumentType(item.DocumentType),
                    CounterpartyId = item.CounterpartyId,
                    ContractId = item.ContractId,
                    CurrencyId = item.CurrencyId,
                    WarehouseId = item.WarehouseId,
                    ExchangeRate = item.ExchangeRate,
                    Comment = item.Comment,
                    Lines = item.Lines
                        .Select(line => new CanonicalLine
                        {
                            LineNumber = line.LineNumber,
                            ProductId = line.ProductId,
                            Quantity = line.Quantity,
                            UnitPrice = line.UnitPrice,
                            UnitId = line.UnitId,
                            VatRateId = line.VatRateId,
                            CostPrice = line.CostPrice,
                            CostPriceSource = line.CostPriceSource,
                            MarkingSource = line.MarkingSource,
                            ProductTableIds = line.ProductTableIds.OrderBy(id => id).ToArray()
                        })
                        .OrderBy(line => line.LineNumber)
                        .ThenBy(line => line.ProductId)
                        .ThenBy(line => line.UnitId)
                        .ThenBy(line => line.VatRateId)
                        .ThenBy(line => line.CostPriceSource, StringComparer.Ordinal)
                        .ThenBy(line => line.MarkingSource, StringComparer.Ordinal)
                        .ThenBy(line => line.Quantity)
                        .ThenBy(line => line.UnitPrice)
                        .ThenBy(line => line.CostPrice)
                        .ToArray()
                })
                .OrderBy(item => item.ProviderDocumentId, StringComparer.Ordinal)
                .ThenBy(item => item.Direction, StringComparer.Ordinal)
                .ToArray()
        };

        var payload = JsonSerializer.Serialize(canonical, JsonOptions);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    private sealed class CanonicalRequest
    {
        public int OrganizationId { get; init; }
        public string ExpectedPlanHash { get; init; } = string.Empty;
        public bool AllowSentDocuments { get; init; }
        public bool AllowUnmatchedMarkings { get; init; }
        public CanonicalItem[] Items { get; init; } = [];
    }

    private sealed class CanonicalItem
    {
        public string ProviderDocumentId { get; init; } = string.Empty;
        public string Direction { get; init; } = string.Empty;
        public string DocumentType { get; init; } = string.Empty;
        public int CounterpartyId { get; init; }
        public long? ContractId { get; init; }
        public short CurrencyId { get; init; }
        public int WarehouseId { get; init; }
        public decimal ExchangeRate { get; init; }
        public string? Comment { get; init; }
        public CanonicalLine[] Lines { get; init; } = [];
    }

    private sealed class CanonicalLine
    {
        public int LineNumber { get; init; }
        public int ProductId { get; init; }
        public decimal? Quantity { get; init; }
        public decimal? UnitPrice { get; init; }
        public short UnitId { get; init; }
        public short? VatRateId { get; init; }
        public decimal? CostPrice { get; init; }
        public string? CostPriceSource { get; init; }
        public string MarkingSource { get; init; } = string.Empty;
        public int[] ProductTableIds { get; init; } = [];
    }
}
