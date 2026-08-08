using Application.Abstractions.Integration.Edo;
using Application.Features.Integration.Edocs.Facturas;
using Application.Features.Integration.Edocs.Services;
using SharedKernel.Exceptions;
using System.Globalization;
using System.Text.Json;

namespace Integration.Edo.Providers;

public sealed class EdocsEdoProvider(
    IEdocsAuthService authService,
    IEdocsFacturaService facturaService,
    Integration.Edocs.Facturas.EdocsEdoOperations edoOperations) : IEdoProvider
{
    public EdoProviderCode Code => EdoProviderCode.EDOCS;

    public EdoProviderCapabilityDto Capabilities { get; } = CreateCapabilities();

    public async Task<EdoAuthChallengeDto> GetAuthChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default)
    {
        var serialNumber = RequireCertificateSerialNumber(request.CertificateSerialNumber);
        var result = await authService.GetAuthChallengeAsync(serialNumber, ct);
        var challengePayload = JsonSerializer.Serialize(new { challenge = result.AuthId });

        return new EdoAuthChallengeDto
        {
            ChallengeId = result.AuthId,
            AuthMode = EdoAuthMode.EImzo,
            Payload = challengePayload,
            PayloadFormat = "Json",
            ExpiresAt = result.ExpiresAt
        };
    }

    public async Task<EdoAuthCompleteDto> CompleteAuthAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default)
    {
        var serialNumber = RequireCertificateSerialNumber(request.CertificateSerialNumber);
        if (string.IsNullOrWhiteSpace(request.ChallengeId))
            throw new InvalidOperationException("ChallengeId is required for Edocs authentication.");

        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7))
            throw new InvalidOperationException("PreparedPkcs7 is required for Edocs authentication.");

        var result = await authService.CompleteAuthAsync(
            new EdocsAuthCompleteRequestDto
            {
                AuthId = request.ChallengeId,
                SerialNumber = serialNumber,
                Pkcs7 = request.PreparedPkcs7
            },
            ct);

        return new EdoAuthCompleteDto { IsAuthenticated = result.Success };
    }

    public async Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default)
    {
        var result = await facturaService.CreateFacturaDocumentAsync(
            new EdocsFacturaCreateRequestDto
            {
                InternalDocumentId = request.InternalDocumentId,
                InternalDocumentType = request.InternalDocumentType,
                SellerTin = RequireText(request.Seller.TaxIdentifier, "Seller.TaxIdentifier"),
                BuyerTin = RequireText(request.Buyer.TaxIdentifier, "Buyer.TaxIdentifier"),
                Seller = MapParty(request.Seller),
                Buyer = MapParty(request.Buyer),
                FacturaNo = RequireText(request.DocumentNumber, nameof(request.DocumentNumber)),
                FacturaDate = request.DocumentDate,
                ContractNo = request.ContractNumber,
                ContractDate = request.ContractDate,
                Products = request.Lines.Select(MapProduct).ToList(),
                IdempotencyKey = request.IdempotencyKey
            },
            ct);

        return new EdoOutboxCreateDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = result.ProviderDocumentId,
                LegacyDocumentId = result.MarkingEdocsDocumentId,
                Direction = EdoDirection.OUTBOX,
                DocumentType = "FACTURA",
                DocumentNumber = request.DocumentNumber,
                DocumentDate = request.DocumentDate,
                Status = EdoProviderStatusMapper.Map(result.Status)
            },
            IsReplay = result.IsReplay
        };
    }

    public async Task<EdoOutboxSignDto> SignOutboxAsync(
        long id,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7))
            throw new InvalidOperationException("Edocs signing requires PreparedPkcs7.");

        var result = await facturaService.SignFacturaDocumentAsync(
            new EdocsFacturaSignRequestDto
            {
                MarkingEdocsDocumentId = id,
                Pkcs7 = request.PreparedPkcs7
            },
            ct);

        return new EdoOutboxSignDto
        {
            Document = new EdoDocumentDto
            {
                LegacyDocumentId = result.MarkingEdocsDocumentId,
                Direction = EdoDirection.OUTBOX,
                DocumentType = "FACTURA",
                Status = EdoProviderStatusMapper.Map(result.Status)
            }
        };
    }

    public Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default) =>
        edoOperations.ListInboxAsync(request, ct);

    public Task<EdoInboxListDto> ListDocumentsAsync(
        EdoDocumentQueryDto request,
        CancellationToken ct = default) =>
        edoOperations.ListDocumentsAsync(request, ct);

    public Task<EdoDocumentDto> GetDocumentDetailsAsync(
        EdoDirection direction,
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetDocumentDetailsAsync(direction, providerDocumentType, providerDocumentId, ct);

    public Task<EdoInboxSummaryDto> GetInboxSummaryAsync(CancellationToken ct = default) =>
        edoOperations.GetInboxSummaryAsync(ct);

    public Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentType,
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default) =>
        edoOperations.RejectInboxAsync(providerDocumentType, providerDocumentId, request, ct);

    public Task<EdoFileDto> GetFileAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetFileAsync(providerDocumentType, providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetStatusAsync("FACTURA", providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetStatusAsync(providerDocumentType, providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetStatusAsync("FACTURA", providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetStatusAsync(providerDocumentType, providerDocumentId, ct);

    private Task<T> ThrowUnavailable<T>(EdoCapabilityKind capability) =>
        throw new EdoCapabilityUnavailableException(
            Code.ToString(),
            capability.ToString(),
            EdoCapabilityStatus.UNKNOWN.ToString());

    private static string RequireCertificateSerialNumber(string? serialNumber) =>
        string.IsNullOrWhiteSpace(serialNumber)
            ? throw new InvalidOperationException("CertificateSerialNumber is required for Edocs authentication.")
            : serialNumber;

    private static EdocsFacturaPartyDto MapParty(EdoPartyDto party) => new()
    {
        BankId = RequireText(party.BankCode, "Party.BankCode"),
        Name = RequireText(party.Name, "Party.Name"),
        Account = RequireText(party.AccountNumber, "Party.AccountNumber"),
        Address = RequireText(party.Address, "Party.Address"),
        DistrictId = RequireText(party.DistrictId, "Party.DistrictId"),
        Director = party.DirectorName,
        Accountant = party.AccountantName,
        VatRegCode = party.TaxIdentifier
    };

    private static EdocsFacturaProductRequestDto MapProduct(EdoFacturaLineDto line) => new()
    {
        OrdNo = line.Number,
        Name = RequireText(line.Name, "Line.Name"),
        MeasureId = ParseMeasureId(line.UnitCode),
        Count = line.Quantity.ToString(CultureInfo.InvariantCulture),
        Summa = line.Amount.ToString(CultureInfo.InvariantCulture),
        VatRate = line.TaxRate ?? 0m,
        WithoutVat = line.IsTaxFree,
        HasVat = !line.IsTaxFree,
        MarkingCodeIds = line.MarkingCodeIds.ToList()
    };

    private static int ParseMeasureId(string? unitCode)
    {
        if (!int.TryParse(unitCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var measureId)
            || measureId <= 0)
        {
            throw new InvalidOperationException("Line.UnitCode must contain a positive Edocs measure id.");
        }

        return measureId;
    }

    private static string RequireText(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for Edocs factura creation.")
            : value;

    private static EdoProviderCapabilityDto CreateCapabilities() =>
        new()
        {
            ProviderCode = EdoProviderCode.EDOCS,
            DisplayName = "Edocs",
            AuthModes = [EdoAuthMode.EImzo],
            SigningModes = [EdoSigningMode.PreparedPkcs7],
            Capabilities = Enum.GetValues<EdoCapabilityKind>()
                .Select(kind => new EdoCapabilityDto
                {
                    Kind = kind,
                    Status = kind switch
                    {
                        EdoCapabilityKind.AuthChallenge
                            or EdoCapabilityKind.AuthComplete
                            or EdoCapabilityKind.CreateFactura
                            or EdoCapabilityKind.SignOutbox
                            or EdoCapabilityKind.ListInbox
                            or EdoCapabilityKind.ListOutbox
                            or EdoCapabilityKind.ListDrafts
                            or EdoCapabilityKind.GetDetail
                            or EdoCapabilityKind.GetFile
                            or EdoCapabilityKind.GetOutboxStatus
                            or EdoCapabilityKind.GetInboxStatus
                            or EdoCapabilityKind.RejectInbox
                            or EdoCapabilityKind.Summary => EdoCapabilityStatus.SUPPORTED,
                        EdoCapabilityKind.SearchFilter
                            or EdoCapabilityKind.Marking => EdoCapabilityStatus.PARTIAL,
                        _ => EdoCapabilityStatus.UNKNOWN
                    }
                })
                .ToList()
        };
}
