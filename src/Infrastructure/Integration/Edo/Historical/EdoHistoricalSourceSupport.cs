using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;
using SharedKernel.Text;

namespace Integration.Edo.Historical;

internal sealed class EdoHistoricalMappingException : Exception
{
    public EdoHistoricalMappingException(string safeFailureCode, Exception? innerException = null)
        : base(safeFailureCode, innerException)
    {
        SafeFailureCode = safeFailureCode;
    }

    public string SafeFailureCode { get; }
}

internal static class EdoHistoricalSourceSupport
{
    public const string AuthRequired = "AUTHENTICATION_REQUIRED";
    public const string TransientProviderFailure = "TRANSIENT_PROVIDER_FAILURE";
    public const string TerminalProviderFailure = "TERMINAL_PROVIDER_FAILURE";
    public const string OrganizationNotConfigured = "ORGANIZATION_NOT_CONFIGURED";

    public static EdoHistoricalPageResultDto BuildSuccessfulPage(
        EdoProviderCode providerCode,
        EdoHistoricalPageRequestDto request,
        int page,
        int pageSize,
        int? providerTotal,
        bool? hasNextPage,
        int? nextPage,
        bool hasCompleteMetadata,
        bool providerRequiresOverlapRescan,
        IReadOnlyCollection<EdoHistoricalDocumentSummaryDto> items)
    {
        var distinctItems = items
            .GroupBy(item => item.ProviderDocumentId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        var duplicateItemCount = items.Count - distinctItems.Length;
        var currentIds = distinctItems.Select(item => item.ProviderDocumentId).ToHashSet(StringComparer.Ordinal);
        var previousIds = request.PreviousPageProviderDocumentIds.ToHashSet(StringComparer.Ordinal);
        var isRepeatedPage = currentIds.Count > 0
            && previousIds.Count == currentIds.Count
            && previousIds.SetEquals(currentIds);
        var providerTotalChanged = request.PreviousProviderTotal.HasValue
            && providerTotal.HasValue
            && request.PreviousProviderTotal.Value != providerTotal.Value;
        var hasSafetyAnomaly = duplicateItemCount > 0 || isRepeatedPage || providerTotalChanged;
        var isCompletenessConfirmed = hasCompleteMetadata
            && !providerRequiresOverlapRescan
            && !hasSafetyAnomaly;

        return new EdoHistoricalPageResultDto
        {
            ProviderCode = providerCode,
            Page = page,
            PageSize = pageSize,
            ProviderTotal = providerTotal,
            HasNextPage = hasNextPage,
            NextPage = nextPage,
            IsCompletenessConfirmed = isCompletenessConfirmed,
            RequiresOverlapRescan = providerRequiresOverlapRescan || !hasCompleteMetadata || hasSafetyAnomaly,
            IsRepeatedPage = isRepeatedPage,
            ProviderTotalChanged = providerTotalChanged,
            DuplicateItemCount = duplicateItemCount,
            State = isCompletenessConfirmed
                ? EdoHistoricalReadState.COMPLETE
                : EdoHistoricalReadState.PARTIAL,
            Items = distinctItems
        };
    }

    public static EdoHistoricalPageResultDto BuildFailurePage(
        EdoProviderCode providerCode,
        EdoHistoricalPageRequestDto request,
        Exception exception,
        string? operationCode = null)
    {
        var (state, code) = Classify(exception);
        return new EdoHistoricalPageResultDto
        {
            ProviderCode = providerCode,
            Page = request.Page,
            PageSize = request.PageSize,
            IsCompletenessConfirmed = false,
            RequiresOverlapRescan = true,
            State = state,
            SafeFailureCode = QualifyFailureCode(operationCode, exception, code)
        };
    }

    public static EdoHistoricalDetailResultDto BuildFailureDetail(
        EdoProviderCode providerCode,
        Exception exception,
        string? operationCode = null)
    {
        var (state, code) = Classify(exception);
        return new EdoHistoricalDetailResultDto
        {
            ProviderCode = providerCode,
            State = state,
            IsImportReady = false,
            SafeFailureCode = QualifyFailureCode(operationCode, exception, code)
        };
    }

    public static EdoHistoricalDetailResultDto ValidateAndMapDetail(
        EdoProviderCode providerCode,
        string organizationInn,
        EdoHistoricalDetailRequestDto request,
        EdoDocumentDto document)
    {
        var normalizedStatus = IsConfirmedSigned(providerCode, document.Status.ProviderStatusCode)
            ? EdoDocumentStatusCode.SIGNED
            : EdoDocumentStatusCode.UNKNOWN;
        var normalizedType = NormalizeDocumentType(providerCode, document.DocumentType);
        var lines = document.PreviewLines.Select(line => new EdoHistoricalDocumentLineDto
        {
            Number = line.Number,
            CatalogCode = line.CatalogCode,
            CatalogName = NormalizeProviderProductName(line.CatalogName),
            PackageCode = line.PackageCode,
            PackageName = line.PackageName,
            IsService = line.IsService,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            NetAmount = line.NetAmount,
            VatRate = line.VatRate,
            VatAmount = line.VatAmount,
            TotalAmount = line.TotalWithVat,
            MarkingNumbers = line.MarkingCodes
        }).ToArray();
        var sellerTin = NormalizeTin(string.IsNullOrWhiteSpace(document.Seller?.TaxIdentifier)
            ? document.PreviewSellerTin
            : document.Seller.TaxIdentifier);
        var buyerTin = NormalizeTin(document.Buyer?.TaxIdentifier);
        var detail = new EdoHistoricalDocumentDetailDto
        {
            ProviderDocumentId = document.ProviderDocumentId ?? string.Empty,
            Direction = document.Direction,
            Status = normalizedStatus,
            DocumentType = normalizedType,
            DocumentNumber = document.DocumentNumber,
            DocumentDate = document.DocumentDate,
            Seller = string.IsNullOrWhiteSpace(sellerTin)
                ? null
                : new EdoHistoricalPartyDto
                {
                    Name = document.Seller?.Name ?? string.Empty,
                    Tin = sellerTin
                },
            Buyer = string.IsNullOrWhiteSpace(buyerTin)
                ? null
                : new EdoHistoricalPartyDto
                {
                    Name = document.Buyer?.Name ?? string.Empty,
                    Tin = buyerTin
                },
            ContractNumber = document.PreviewContractNumber,
            ContractDate = document.PreviewContractDate,
            NetAmount = SumWhenComplete(lines.Select(line => line.NetAmount)),
            VatAmount = SumWhenComplete(lines.Select(line => line.VatAmount)),
            TotalAmount = document.TotalAmount,
            Lines = lines,
            MarkingNumbers = document.MarkingCodes
                .Concat(lines.SelectMany(line => line.MarkingNumbers))
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };

        var failureCode = ValidateDetail(organizationInn, request, detail);
        return new EdoHistoricalDetailResultDto
        {
            ProviderCode = providerCode,
            State = failureCode is null
                ? EdoHistoricalReadState.COMPLETE
                : EdoHistoricalReadState.VALIDATION_FAILURE,
            IsImportReady = failureCode is null,
            SafeFailureCode = failureCode,
            Document = detail
        };
    }

    private static string? ValidateDetail(
        string organizationInn,
        EdoHistoricalDetailRequestDto request,
        EdoHistoricalDocumentDetailDto document)
    {
        if (request.DateFrom > request.DateTo)
            return "INVALID_DATE_RANGE";
        if (request.Item.Direction != EdoDirection.INBOX || document.Direction != EdoDirection.INBOX)
            return "DOCUMENT_DIRECTION_NOT_INBOX";
        if (!string.Equals(
                request.Item.ProviderDocumentId,
                document.ProviderDocumentId,
                StringComparison.Ordinal))
            return "PROVIDER_DOCUMENT_IDENTITY_MISMATCH";
        if (document.Status != EdoDocumentStatusCode.SIGNED)
            return "DOCUMENT_NOT_SIGNED";
        if (!string.Equals(document.DocumentType, "FACTURA", StringComparison.Ordinal))
            return "UNSUPPORTED_DOCUMENT_TYPE";
        if (!document.DocumentDate.HasValue)
            return "DOCUMENT_DATE_REQUIRED";
        if (document.DocumentDate.Value < request.DateFrom || document.DocumentDate.Value > request.DateTo)
            return "DOCUMENT_OUTSIDE_DATE_RANGE";
        if (document.Buyer is null
            || !string.Equals(NormalizeTin(organizationInn), document.Buyer.Tin, StringComparison.Ordinal))
            return "BUYER_ORGANIZATION_MISMATCH";
        if (document.Seller is null)
            return "SELLER_TIN_REQUIRED";
        if (string.Equals(document.Seller.Tin, document.Buyer.Tin, StringComparison.Ordinal))
            return "SELLER_BUYER_MUST_DIFFER";

        var lineIntegrityFailure = ValidateLineIntegrity(document.Lines);
        if (lineIntegrityFailure is not null)
            return lineIntegrityFailure;

        return null;
    }

    internal static string? ValidateLineIntegrity(
        IReadOnlyCollection<EdoHistoricalDocumentLineDto> lines)
    {
        if (lines.Count == 0)
            return "DOCUMENT_LINES_REQUIRED";
        if (lines.Any(line => line.Number <= 0))
            return "PROVIDER_LINE_NUMBER_INVALID";
        if (lines.GroupBy(line => line.Number).Any(group => group.Count() > 1))
            return "PROVIDER_LINE_NUMBER_DUPLICATE";

        return null;
    }

    private static bool IsConfirmedSigned(EdoProviderCode providerCode, string? providerStatusCode) =>
        providerCode switch
        {
            EdoProviderCode.DIDOX => string.Equals(providerStatusCode?.Trim(), "3", StringComparison.Ordinal),
            EdoProviderCode.EDOCS => string.Equals(providerStatusCode?.Trim(), "signed", StringComparison.OrdinalIgnoreCase),
            _ => false
        };

    private static string NormalizeDocumentType(EdoProviderCode providerCode, string? documentType)
    {
        var value = documentType?.Trim();
        return providerCode switch
        {
            EdoProviderCode.DIDOX when string.Equals(value, "002", StringComparison.Ordinal)
                || string.Equals(value, "FACTURA", StringComparison.OrdinalIgnoreCase) => "FACTURA",
            EdoProviderCode.EDOCS when string.Equals(value, "factura", StringComparison.OrdinalIgnoreCase) => "FACTURA",
            _ => "UNKNOWN"
        };
    }

    private static string NormalizeTin(string? value) => value?.Trim() ?? string.Empty;

    private static string? NormalizeProviderProductName(string? value)
    {
        if (Utf8MojibakeNormalizer.TryNormalizeProviderProductName(value, out var normalized))
            return normalized;

        throw new EdoHistoricalMappingException("PROVIDER_PRODUCT_NAME_INVALID");
    }

    private static decimal? SumWhenComplete(IEnumerable<decimal?> values)
    {
        var items = values.ToArray();
        return items.Length > 0 && items.All(value => value.HasValue)
            ? items.Sum(value => value!.Value)
            : null;
    }

    private static (EdoHistoricalReadState State, string SafeFailureCode) Classify(Exception exception) =>
        exception switch
        {
            EdoHistoricalMappingException mappingException =>
                (EdoHistoricalReadState.VALIDATION_FAILURE, mappingException.SafeFailureCode),
            IntegrationHttpException { StatusCode: 401 } =>
                (EdoHistoricalReadState.WAITING_AUTH, AuthRequired),
            IntegrationHttpException { StatusCode: 429 } =>
                (EdoHistoricalReadState.TRANSIENT_FAILURE, TransientProviderFailure),
            IntegrationHttpException { StatusCode: >= 500 } =>
                (EdoHistoricalReadState.TRANSIENT_FAILURE, TransientProviderFailure),
            HttpRequestException or TimeoutException or OperationCanceledException =>
                (EdoHistoricalReadState.TRANSIENT_FAILURE, TransientProviderFailure),
            _ =>
                (EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE, TerminalProviderFailure)
        };

    private static string QualifyFailureCode(
        string? operationCode,
        Exception exception,
        string fallbackCode)
    {
        if (string.IsNullOrWhiteSpace(operationCode))
            return fallbackCode;

        if (exception is EdoHistoricalMappingException documentExclusion
            && documentExclusion.SafeFailureCode is
                "PROVIDER_LINE_NUMBER_INVALID"
                or "PROVIDER_LINE_NUMBER_DUPLICATE"
                or "DOCUMENT_LINES_REQUIRED")
        {
            return documentExclusion.SafeFailureCode;
        }

        if (exception is EdoHistoricalMappingException existingMappingException
            && existingMappingException.SafeFailureCode.StartsWith(
                operationCode + "_",
                StringComparison.Ordinal))
        {
            return existingMappingException.SafeFailureCode;
        }

        if (exception is EdoHistoricalMappingException legacyDetailException
            && operationCode.EndsWith("_HISTORICAL_DETAIL", StringComparison.Ordinal)
            && operationCode[..^"_HISTORICAL_DETAIL".Length] is { Length: > 0 } providerCode
            && legacyDetailException.SafeFailureCode.StartsWith(
                providerCode + "_DETAIL_",
                StringComparison.Ordinal))
        {
            return operationCode + legacyDetailException.SafeFailureCode[(providerCode.Length + "_DETAIL".Length)..];
        }

        var suffix = exception switch
        {
            IntegrationHttpException httpException => $"HTTP_{httpException.StatusCode}",
            EdoHistoricalMappingException mappingException => mappingException.SafeFailureCode,
            _ => fallbackCode
        };
        return $"{operationCode}_{suffix}";
    }
}
