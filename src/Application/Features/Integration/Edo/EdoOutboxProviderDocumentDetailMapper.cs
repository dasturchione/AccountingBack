using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Application.Features.Integration.Edo;

public static class EdoOutboxProviderDocumentDetailMapper
{
    private const int MaxProviderDocumentIdLength = 200;

    public static EdoOutboxProviderDocumentDetailDto MapAndValidate(
        EdoDocumentDto document,
        string requestedProviderDocumentId)
        => MapAndValidate(document, requestedProviderDocumentId, "FACTURA", allowSentDocuments: false);

    public static EdoOutboxProviderDocumentDetailDto MapAndValidate(
        EdoDocumentDto document,
        string requestedProviderDocumentId,
        string expectedDocumentType,
        bool allowSentDocuments)
    {
        var requestedId = RequireProviderDocumentId(requestedProviderDocumentId);
        var normalizedExpectedType = NormalizeDocumentType(expectedDocumentType);

        if (document is null
            || string.IsNullOrWhiteSpace(document.ProviderDocumentId)
            || !string.Equals(document.ProviderDocumentId, requestedId, StringComparison.Ordinal))
        {
            throw Failure("EDO_OUTBOX_DETAIL_INVALID", 502);
        }

        if (document.ProviderCode != EdoProviderCode.EDOCS
            || document.Direction != EdoDirection.OUTBOX
            || NormalizeDocumentType(document.DocumentType) != normalizedExpectedType
            || normalizedExpectedType is not ("FACTURA" or "WAYBILL_LOCAL"))
        {
            throw Failure("EDO_OUTBOX_DETAIL_INVALID", 502);
        }

        if (document.Status.Code == EdoDocumentStatusCode.UNKNOWN)
            throw Failure("EDO_OUTBOX_DETAIL_STATUS_INVALID", 502);

        if (document.Status.Code != EdoDocumentStatusCode.SIGNED
            && !(allowSentDocuments && document.Status.Code == EdoDocumentStatusCode.SENT))
            throw Failure("EDO_OUTBOX_DETAIL_NOT_SIGNED", 422);

        if (string.IsNullOrWhiteSpace(document.DocumentNumber)
            || !document.DocumentDate.HasValue
            || document.Seller is null
            || document.Buyer is null
            || !document.TotalAmount.HasValue)
        {
            throw Failure("EDO_OUTBOX_DETAIL_INVALID", 502);
        }

        var sourceLines = document.PreviewLines?.ToList() ?? [];
        if (sourceLines.Count == 0)
            throw Failure("EDO_OUTBOX_LINES_INVALID", 502);

        var lineNumbers = new HashSet<int>();
        var lines = new List<EdoOutboxProviderDocumentLineDto>(sourceLines.Count);
        foreach (var sourceLine in sourceLines)
        {
            if (sourceLine.Number <= 0 || !lineNumbers.Add(sourceLine.Number)
                || !sourceLine.Quantity.HasValue
                || sourceLine.Quantity.Value <= 0m
                || !sourceLine.UnitPrice.HasValue
                || !sourceLine.NetAmount.HasValue
                || !sourceLine.VatAmount.HasValue
                || !sourceLine.TotalWithVat.HasValue)
            {
                throw Failure("EDO_OUTBOX_LINES_INVALID", 502);
            }

            lines.Add(new EdoOutboxProviderDocumentLineDto
            {
                Number = sourceLine.Number,
                ProviderProductCode = sourceLine.CatalogCode,
                ProviderProductName = sourceLine.CatalogName,
                PackageCode = sourceLine.PackageCode,
                PackageName = sourceLine.PackageName,
                Quantity = sourceLine.Quantity.Value,
                UnitPrice = sourceLine.UnitPrice.Value,
                VatRate = sourceLine.VatRate,
                NetAmount = sourceLine.NetAmount.Value,
                VatAmount = sourceLine.VatAmount.Value,
                TotalWithVat = sourceLine.TotalWithVat.Value,
                Marking = new EdoProviderMarkingMetadataDto
                {
                    HasMarkings = GetMarkingCount(sourceLine) > 0,
                    Count = GetMarkingCount(sourceLine)
                }
            });
        }

        var safeStatus = new EdoDocumentStatusDto
        {
            Code = document.Status.Code,
            LocalCode = document.Status.Code,
            ProviderStatusCode = document.Status.Code.ToString(),
            IsTerminal = document.Status.IsTerminal,
            IsSuccessful = document.Status.IsSuccessful,
            IsReconciliationRequired = document.Status.IsReconciliationRequired
        };

        return new EdoOutboxProviderDocumentDetailDto
        {
            ProviderDocumentId = requestedId,
            ProviderCode = EdoProviderCode.EDOCS,
            Direction = EdoDirection.OUTBOX,
            Category = EdoDocumentCategory.OUTBOX,
            DocumentType = normalizedExpectedType,
            DocumentNumber = document.DocumentNumber.Trim(),
            DocumentDate = document.DocumentDate.Value,
            Status = safeStatus,
            Seller = document.Seller,
            Buyer = document.Buyer,
            ContractNumber = CleanOptional(document.PreviewContractNumber),
            ContractDate = document.PreviewContractDate,
            NetAmount = lines.Sum(x => x.NetAmount),
            VatAmount = lines.Sum(x => x.VatAmount),
            TotalAmount = document.TotalAmount.Value,
            CurrencyCode = CleanOptional(document.CurrencyCode),
            Lines = lines
        };
    }

    public static string RequireProviderDocumentId(string providerDocumentId)
    {
        var value = providerDocumentId?.Trim();
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaxProviderDocumentIdLength
            || value.Any(char.IsControl))
        {
            throw Failure("EDO_OUTBOX_DETAIL_INVALID", 400);
        }

        return value;
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int GetMarkingCount(EdoDocumentPreviewLineDto line) =>
        line.MarkingCount.GetValueOrDefault() > 0
            ? line.MarkingCount.Value
            : line.MarkingCodes.Count;

    private static string NormalizeDocumentType(string? value) =>
        string.Equals(value?.Trim(), "waybillLocal", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value?.Trim(), "WAYBILL_LOCAL", StringComparison.OrdinalIgnoreCase)
            ? "WAYBILL_LOCAL"
            : string.Equals(value?.Trim(), "FACTURA", StringComparison.OrdinalIgnoreCase)
                ? "FACTURA"
                : value?.Trim().ToUpperInvariant() ?? string.Empty;

    private static EdoOutboxProviderDocumentException Failure(string code, int statusCode) =>
        new(code, statusCode);
}
