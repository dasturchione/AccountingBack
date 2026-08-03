using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Integration.Faktura.Dtos;
using Integration.Faktura.Http;
using Integration.Edo.Http;
using Microsoft.AspNetCore.Http;
using SharedKernel.Exceptions;
using SharedKernel.Query.Specifications;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Integration.Faktura.Edo;

public sealed class FakturaEdoOperations(
    IHttpClientFactory httpClientFactory,
    IFakturaTokenService tokenService,
    IUserContext userContext,
    IQueryRepository<Organization> organizationQuery)
{
    private const int MaxErrorResponseBodyBytes = 4 * 1024;
    private const int MaxDiagnosticValueLength = 512;
    private static readonly HashSet<string> SafeErrorFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "message",
        "error",
        "detail"
    };
    private static readonly Regex SensitiveDiagnosticRegex = new(
        """(?i)(token|password|client[_-]?secret|authorization|pkcs7|signature|private[_-]?key|inn|tin|tax[_-]?id)\s*['"]?\s*[:=]\s*(?:"[^"]*"|'[^']*'|[^,;\s}\]]+)""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<EdoAuthCompleteDto> CompleteAuthAsync(CancellationToken ct = default)
    {
        _ = await tokenService.GetAccessTokenAsync(ct);
        return new EdoAuthCompleteDto { IsAuthenticated = true };
    }

    public async Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default)
    {
        // TAXMIN: umumiy outbox modelidagi Seller.TaxIdentifier Faktura.uz
        // companyInn query parametriga moslashtiriladi.
        var companyInn = RequireText(request.Seller.TaxIdentifier, "Seller.TaxIdentifier");
        var payload = new FakturaImportDocumentRequestDto
        {
            Invoices = [MapInvoice(request)]
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        var endpoint = $"Api/Document/ImportDocumentRegister?companyInn={Uri.EscapeDataString(companyInn)}";
        using var response = await client.PostAsJsonAsync(
            endpoint,
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);

        await EnsureSuccessAsync(response, "Faktura factura import");

        var result = await response.Content.ReadFromJsonAsync<FakturaImportDocumentResponseDto>(ct)
            ?? throw new IntegrationHttpException(
                "Faktura import response could not be parsed.",
                StatusCodes.Status502BadGateway);

        if (result.ErrorCount != 0
            || result.SuccessCount != 1
            || result.SuccessItems is null
            || result.SuccessItems.Count != 1)
        {
            throw new IntegrationHttpException(
                $"Faktura import did not return exactly one successful document (success={result.SuccessCount}, errors={result.ErrorCount}).",
                StatusCodes.Status502BadGateway);
        }

        var item = result.SuccessItems.Single();
        // Faktura.uz UniqueId — provider hujjat identifikatori. Id ayrim javoblarda
        // string/null bo'lishi mumkin; u faqat numeric bo'lsa LegacyDocumentId sifatida
        // qo'shimcha mapping qilinadi. Ikkalasi ham mavjud bo'lmasa keyingi provider
        // operatsiyalarini xavfsiz davom ettirib bo'lmaydi.
        var providerDocumentId = RequireProviderDocumentId(
            string.IsNullOrWhiteSpace(item.UniqueId) ? item.Id : item.UniqueId,
            "SuccessItems.UniqueId/Id");
        var legacyDocumentId = ParseLegacyDocumentId(item.Id);

        return new EdoOutboxCreateDto
        {
            Document = new EdoDocumentDto
            {
                LegacyDocumentId = legacyDocumentId,
                ProviderDocumentId = providerDocumentId,
                Direction = EdoDirection.OUTBOX,
                DocumentType = "FACTURA",
                DocumentNumber = request.DocumentNumber,
                DocumentDate = request.DocumentDate,
                // Import response does not document a document status; do not infer success.
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.UNKNOWN,
                    CheckedAt = DateTimeOffset.UtcNow
                }
            }
        };
    }

    public async Task<EdoOutboxSignDto> SignOutboxAsync(
        string providerDocumentId,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var signedContent = RequireText(request.PreparedPkcs7, nameof(request.PreparedPkcs7));
        var certificateSerial = RequireText(
            request.CertificateSerialNumber,
            nameof(request.CertificateSerialNumber));

        var payload = new FakturaSignDocumentRequestDto
        {
            UniqueId = uniqueId,
            SignedContent = signedContent,
            CertificateSerial = certificateSerial
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            "Api/Sign/SignDocument",
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);

        await EnsureSuccessAsync(response, "Faktura outbox sign");
        var resultCode = await ReadSignResultCodeAsync(response, ct);
        if (resultCode != 0)
        {
            throw new IntegrationHttpException(
                $"Faktura outbox sign returned code {resultCode} ({MapSignResultCode(resultCode)}).",
                StatusCodes.Status502BadGateway);
        }

        return new EdoOutboxSignDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = uniqueId,
                Direction = EdoDirection.OUTBOX,
                DocumentType = "FACTURA",
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.UNKNOWN,
                    CheckedAt = DateTimeOffset.UtcNow
                }
            }
        };
    }

    public async Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default)
    {
        var companyInn = RequireText(request.CompanyInn, nameof(request.CompanyInn));
        var skip = checked((request.Page - 1) * request.PageSize);
        var query = string.Join(
            "&",
            $"CompanyInn={Uri.EscapeDataString(companyInn)}",
            $"Limit={request.PageSize.ToString(CultureInfo.InvariantCulture)}",
            $"Skip={skip.ToString(CultureInfo.InvariantCulture)}",
            "IsInbox=true");

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.GetAsync(
            $"Api/Document/GetDocuments?{query}",
            HttpCompletionOption.ResponseHeadersRead,
            ct);
        await EnsureSuccessAsync(response, "Faktura inbox list");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("documents", out var documents)
            || documents.ValueKind != JsonValueKind.Array)
        {
            throw new IntegrationHttpException(
                "Faktura inbox response did not contain the documented documents array.",
                StatusCodes.Status502BadGateway);
        }

        if (!json.RootElement.TryGetProperty("totalCount", out var totalCountProperty)
            || !totalCountProperty.TryGetInt32(out var totalCount))
        {
            throw new IntegrationHttpException(
                "Faktura inbox response did not contain the documented totalCount field.",
                StatusCodes.Status502BadGateway);
        }

        var items = documents.EnumerateArray().Select(ParseInboxDocument).ToList();
        return new EdoInboxListDto
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentId,
        string providerDocumentType,
        CancellationToken ct = default)
    {
        var documentUniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var companyInn = await RequireOrganizationInnAsync(ct);
        var payload = new FakturaRejectDocumentRequestDto
        {
            DocumentUniqueId = documentUniqueId,
            CompanyInn = companyInn
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            "Api/Document/RejectDocument",
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);
        await EnsureSuccessAsync(response, "Faktura inbox reject");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        var result = ParseRejectResult(json.RootElement);
        if (!result.Success || result.Code != 0)
        {
            throw new IntegrationHttpException(
                $"Faktura inbox reject returned code {result.Code} ({MapRejectResultCode(result.Code)}).",
                StatusCodes.Status502BadGateway);
        }

        return new EdoInboxRejectDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = documentUniqueId,
                Direction = EdoDirection.INBOX,
                DocumentType = string.IsNullOrWhiteSpace(providerDocumentType)
                    ? "UNKNOWN"
                    : providerDocumentType,
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.REJECTED,
                    ProviderStatusCode = result.Code.ToString(CultureInfo.InvariantCulture),
                    IsTerminal = true,
                    IsSuccessful = true,
                    CheckedAt = DateTimeOffset.UtcNow
                }
            }
        };
    }

    public async Task<EdoFileDto> GetFileAsync(
        string providerDocumentId,
        CancellationToken ct = default)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        var response = await client.GetAsync(
            $"Api/DownloadArchive/{Uri.EscapeDataString(uniqueId)}",
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        try
        {
            await EnsureSuccessAsync(response, "Faktura ZIP download");
            var stream = await response.Content.ReadAsStreamAsync(ct);
            return new EdoFileDto
            {
                ProviderFileId = uniqueId,
                FileName = "UNKNOWN",
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "UNKNOWN",
                Length = response.Content.Headers.ContentLength ?? -1,
                Content = new EdoProviderResponseStream(stream, response, long.MaxValue)
            };
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        GetStatusAsync(providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        GetStatusAsync(providerDocumentId, ct);

    private async Task<EdoDocumentStatusDto> GetStatusAsync(
        string providerDocumentId,
        CancellationToken ct)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var companyInn = await RequireOrganizationInnAsync(ct);
        var payload = new FakturaDocumentStatusRequestDto
        {
            DocumentUniqueIds = [uniqueId]
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            $"Api/GetDocumentStatus?companyInn={Uri.EscapeDataString(companyInn)}",
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);
        await EnsureSuccessAsync(response, "Faktura document status");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        return ParseDocumentStatus(json.RootElement, uniqueId);
    }

    private static FakturaInvoiceDto MapInvoice(EdoOutboxFacturaCreateRequestDto request) => new()
    {
        Head = new FakturaHeadDto
        {
            Sender = MapParty(request.Seller, true),
            Receiver = MapParty(request.Buyer, false)
        },
        Document = new FakturaDocumentDto
        {
            DocumentNumber = RequireText(request.DocumentNumber, nameof(request.DocumentNumber)),
            DocumentDate = request.DocumentDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            ContractNumber = request.ContractNumber,
            ContractDate = request.ContractDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            Items = request.Lines.Select(MapItem).ToList(),
            ColumnSummaryValues = BuildSummary(request.Lines)
        }
    };

    private static FakturaPartyEnvelopeDto MapParty(EdoPartyDto party, bool sender) =>
        sender
            ? new FakturaPartyEnvelopeDto
            {
                SenderInfo = MapPartyInfo(party),
                ApproverAndSigners = MapSigners(party)
            }
            : new FakturaPartyEnvelopeDto
            {
                ReceiverInfo = MapPartyInfo(party),
                ApproverAndSigners = MapSigners(party)
            };

    private static FakturaPartyInfoDto MapPartyInfo(EdoPartyDto party) => new()
    {
        INN = RequireText(party.TaxIdentifier, "Party.TaxIdentifier"),
        CompanyName = RequireText(party.Name, "Party.Name"),
        Address = string.IsNullOrWhiteSpace(party.Address)
            ? null
            : new FakturaAddressDto { Street = party.Address },
        BankDetails = string.IsNullOrWhiteSpace(party.BankCode)
            && string.IsNullOrWhiteSpace(party.AccountNumber)
            ? null
            : new FakturaBankDetailsDto
            {
                AccountNumber = party.AccountNumber,
                BankCode = party.BankCode
            }
    };

    private static FakturaApproverAndSignersDto? MapSigners(EdoPartyDto party)
    {
        if (string.IsNullOrWhiteSpace(party.DirectorName)
            && string.IsNullOrWhiteSpace(party.AccountantName))
        {
            return null;
        }

        return new FakturaApproverAndSignersDto
        {
            Approver = MapEmployee(party.DirectorName),
            SignerAccountant = MapEmployee(party.AccountantName)
        };
    }

    private static FakturaEmployeeDto? MapEmployee(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : new FakturaEmployeeDto { FirstName = name };

    private static FakturaItemDto MapItem(EdoFacturaLineDto line)
    {
        if (line.Quantity <= 0 || line.Quantity != decimal.Truncate(line.Quantity))
            throw new InvalidOperationException("Line.Quantity must be a positive integer for Faktura import.");

        var unit = RequireText(line.UnitName ?? line.UnitCode, "Line.UnitName/UnitCode");
        var taxRate = line.TaxRate;
        var taxAmount = line.TaxAmount ?? 0m;

        return new FakturaItemDto
        {
            ItemNumber = line.Number.ToString(CultureInfo.InvariantCulture),
            Description = RequireText(line.Name, "Line.Name"),
            Volume = checked((int)line.Quantity),
            UnitPrice = line.Amount / line.Quantity,
            Subtotal = line.Amount,
            Vat = taxRate is null
                ? null
                : new FakturaVatDto
                {
                    VatRate = $"{taxRate.Value.ToString(CultureInfo.InvariantCulture)}%",
                    VatValue = taxAmount
                },
            SubtotalWithTaxes = line.Amount + taxAmount,
            MeasurementUnit = unit
        };
    }

    private static FakturaColumnSummaryValuesDto BuildSummary(
        IReadOnlyCollection<EdoFacturaLineDto> lines)
    {
        var subtotal = lines.Sum(line => line.Amount);
        var vat = lines.Sum(line => line.TaxAmount ?? 0m);
        return new FakturaColumnSummaryValuesDto
        {
            ColumnSubtotal = subtotal,
            ColumnVatValue = vat,
            ColumnSubtotalWithTaxes = subtotal + vat
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
            return;

        var status = (int)response.StatusCode;
        var diagnostics = await ReadErrorDiagnosticsAsync(response);
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException(
                $"{operation} was rejected by Faktura. {diagnostics}"),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException(
                $"{operation} was denied by Faktura. {diagnostics}"),
            _ => new IntegrationHttpException(
                $"{operation} failed with HTTP status {status}. {diagnostics}",
                status)
        };
    }

    private static async Task<string> ReadErrorDiagnosticsAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        var buffer = new byte[MaxErrorResponseBodyBytes];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset));
            if (bytesRead == 0)
                break;

            offset += bytesRead;
        }

        var body = System.Text.Encoding.UTF8.GetString(buffer, 0, offset);
        var fields = new List<string>();
        try
        {
            using var json = JsonDocument.Parse(body);
            CollectSafeErrorFields(json.RootElement, fields);
        }
        catch (JsonException)
        {
            // The bounded body is included only as a redacted summary below.
        }

        var fieldSummary = fields.Count == 0 ? "none" : string.Join(", ", fields);
        var bodySummary = RedactDiagnosticText(body);
        var requestUri = response.RequestMessage?.RequestUri;
        return $"status={(int)response.StatusCode}, "
            + $"host={requestUri?.Host ?? "none"}, "
            + $"path={requestUri?.AbsolutePath ?? "none"}, "
            + $"contentType={response.Content.Headers.ContentType?.ToString() ?? "none"}, "
            + $"bodyPresent={offset > 0}, "
            + $"fields={fieldSummary}, "
            + $"bodySummary={bodySummary ?? "none"}";
    }

    private static void CollectSafeErrorFields(JsonElement element, ICollection<string> fields, int depth = 0)
    {
        if (depth > 5 || fields.Count >= 16)
            return;

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (SafeErrorFieldNames.Contains(property.Name)
                    && property.Value.ValueKind is JsonValueKind.String
                        or JsonValueKind.Number
                        or JsonValueKind.True
                        or JsonValueKind.False)
                {
                    var value = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()
                        : property.Value.GetRawText();
                    var safeValue = RedactDiagnosticText(value);
                    if (!string.IsNullOrWhiteSpace(safeValue))
                        fields.Add($"{property.Name}={safeValue}");
                }

                CollectSafeErrorFields(property.Value, fields, depth + 1);
                if (fields.Count >= 16)
                    return;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                CollectSafeErrorFields(item, fields, depth + 1);
                if (fields.Count >= 16)
                    return;
            }
        }
    }

    private static string? RedactDiagnosticText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var redacted = SensitiveDiagnosticRegex.Replace(value, "$1=<redacted>");
        var compact = Regex.Replace(redacted, @"\s+", " ").Trim();
        return compact.Length <= MaxDiagnosticValueLength
            ? compact
            : compact[..MaxDiagnosticValueLength] + "...";
    }

    private static async Task<int> ReadSignResultCodeAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        var body = (await response.Content.ReadAsStringAsync(ct)).Trim();
        if (int.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rawCode))
            return rawCode;

        try
        {
            using var json = JsonDocument.Parse(body);
            var value = json.RootElement;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numericCode))
                return numericCode;
            if (value.ValueKind == JsonValueKind.String
                && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringCode))
                return stringCode;
        }
        catch (JsonException)
        {
            // The documented response wrapper is incomplete; the caller receives a controlled integration error below.
        }

        throw new IntegrationHttpException(
            "Faktura outbox sign response code could not be determined from the documented scalar response.",
            StatusCodes.Status502BadGateway);
    }

    private static string MapSignResultCode(int code) => code switch
    {
        1 => "BalanceIsLow",
        3 => "SignatureValidationFailed",
        4 => "InternalError",
        5 => "OfferNotSigned",
        6 => "NotAllowedNow",
        7 => "CreditLimitIsLow",
        _ => "UNKNOWN"
    };

    private static (bool Success, int Code) ParseRejectResult(JsonElement root)
    {
        if (!root.TryGetProperty("Success", out var successProperty)
            || successProperty.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
            || !root.TryGetProperty("code", out var codeProperty)
            || !codeProperty.TryGetInt32(out var code))
        {
            throw new IntegrationHttpException(
                "Faktura reject response did not contain the documented Success and code fields.",
                StatusCodes.Status502BadGateway);
        }

        return (successProperty.GetBoolean(), code);
    }

    private static string MapRejectResultCode(int code) => code switch
    {
        0 => "Success",
        2 => "YouAreNotAMemberOfDocument",
        3 => "SignatureValidationFailed",
        4 => "InternalError",
        6 => "NotAllowedNow",
        _ => "UNKNOWN"
    };

    private static EdoDocumentStatusDto ParseDocumentStatus(
        JsonElement root,
        string providerDocumentId)
    {
        if (!root.TryGetProperty("Success", out var successProperty)
            || successProperty.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new IntegrationHttpException(
                "Faktura status response did not contain the documented Success field.",
                StatusCodes.Status502BadGateway);
        }

        if (!successProperty.GetBoolean())
        {
            throw new IntegrationHttpException(
                "Faktura status response reported Success=false.",
                StatusCodes.Status502BadGateway);
        }

        if (!root.TryGetProperty("Data", out var data)
            || data.ValueKind != JsonValueKind.Object
            || !data.TryGetProperty("DocumentStatuses", out var statuses)
            || statuses.ValueKind != JsonValueKind.Array)
        {
            throw new IntegrationHttpException(
                "Faktura status response did not contain the documented Data.DocumentStatuses array.",
                StatusCodes.Status502BadGateway);
        }

        var statusItem = statuses.EnumerateArray()
            .FirstOrDefault(item => item.TryGetProperty("UniqueId", out var idProperty)
                && idProperty.ValueKind == JsonValueKind.String
                && string.Equals(idProperty.GetString(), providerDocumentId, StringComparison.Ordinal));
        if (statusItem.ValueKind == JsonValueKind.Undefined)
        {
            throw new IntegrationHttpException(
                "Faktura status response did not contain the requested document UniqueId.",
                StatusCodes.Status502BadGateway);
        }

        if (!statusItem.TryGetProperty("Status", out var statusProperty))
        {
            throw new IntegrationHttpException(
                "Faktura status item did not contain the documented Status field.",
                StatusCodes.Status502BadGateway);
        }

        var providerStatusCode = ReadProviderStatusCode(statusProperty);
        var description = ReadStatusDescription(statusItem);
        return new EdoDocumentStatusDto
        {
            Code = EdoDocumentStatusCode.UNKNOWN,
            ProviderStatusCode = providerStatusCode,
            Description = description,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }

    private static string ReadProviderStatusCode(JsonElement statusProperty) =>
        statusProperty.ValueKind switch
        {
            JsonValueKind.String => statusProperty.GetString() ?? "UNKNOWN",
            JsonValueKind.Number => statusProperty.GetRawText(),
            _ => "UNKNOWN"
        };

    private static string? ReadStatusDescription(JsonElement statusItem)
    {
        if (!statusItem.TryGetProperty("StatusDescription", out var descriptionProperty)
            || descriptionProperty.ValueKind != JsonValueKind.Array)
            return null;

        var descriptions = descriptionProperty.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join("; ", descriptions);
    }

    private async Task<string> RequireOrganizationInnAsync(CancellationToken ct)
    {
        var organizationId = userContext.OrganizationId
            ?? throw new InvalidOperationException("An active organization is required for Faktura integration.");
        var organization = await organizationQuery.GetAsync(
            new QuerySpecification<Organization> { Criteria = x => x.Id == organizationId },
            ct);
        return RequireText(organization?.Inn, "Organization.Inn");
    }

    private static EdoDocumentDto ParseInboxDocument(JsonElement item)
    {
        if (!item.TryGetProperty("UniqueId", out var idProperty)
            || idProperty.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(idProperty.GetString()))
        {
            throw new IntegrationHttpException(
                "Faktura inbox item did not contain the documented UniqueId field.",
                StatusCodes.Status502BadGateway);
        }

        return new EdoDocumentDto
        {
            ProviderDocumentId = idProperty.GetString(),
            Direction = EdoDirection.INBOX,
            DocumentType = "UNKNOWN",
            Status = new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.UNKNOWN,
                CheckedAt = DateTimeOffset.UtcNow
            }
        };
    }

    private static string RequireText(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for Faktura integration.")
            : value;

    private static string RequireProviderDocumentId(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new IntegrationHttpException(
                $"Faktura import response did not contain a provider document ID ({name}).",
                StatusCodes.Status502BadGateway)
            : value;

    private static long? ParseLegacyDocumentId(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            && id > 0
            ? id
            : null;
}
