using Application.Abstractions.Integration.Edo;
using Application.Features.Integration.Didox.Facturas;
using Application.Features.Integration.Didox.Services;
using SharedKernel.Exceptions;
using System.Globalization;

namespace Integration.Edo.Providers;

public sealed class DidoxEdoProvider(
    IDidoxAuthService authService,
    IDidoxFacturaService facturaService,
    Integration.Didox.Facturas.DidoxEdoOperations edoOperations) : IEdoProvider
{
    public EdoProviderCode Code => EdoProviderCode.DIDOX;

    public EdoProviderCapabilityDto Capabilities { get; } = CreateCapabilities();

    public async Task<EdoAuthChallengeDto> GetAuthChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default)
    {
        var result = await authService.GetAuthChallengeAsync(ct);

        return new EdoAuthChallengeDto
        {
            ChallengeId = Guid.NewGuid().ToString("N"),
            AuthMode = EdoAuthMode.EImzo,
            Payload = result.InnBase64,
            PayloadFormat = "Base64Utf8"
        };
    }

    public async Task<EdoAuthCompleteDto> CompleteAuthAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7))
            throw new InvalidOperationException("PreparedPkcs7 is required for Didox authentication.");

        if (string.IsNullOrWhiteSpace(request.SignatureHex))
            throw new InvalidOperationException("SignatureHex is required for Didox authentication.");

        var result = await authService.CompleteAuthAsync(
            new DidoxAuthCompleteRequestDto
            {
                Pkcs7 = request.PreparedPkcs7,
                SignatureHex = request.SignatureHex
            },
            ct);

        return new EdoAuthCompleteDto
        {
            IsAuthenticated = result.Success,
            ExpiresAt = result.ExpiresAt,
            AuthenticatedSessionExpiresAt = result.ExpiresAt
        };
    }

    public async Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default)
    {
        var result = await facturaService.CreateFacturaDocumentAsync(
            new DidoxFacturaCreateRequestDto
            {
                InternalDocumentId = request.InternalDocumentId,
                InternalDocumentType = request.InternalDocumentType,
                SellerTin = RequireText(request.Seller.TaxIdentifier, "Seller.TaxIdentifier"),
                BuyerTin = RequireText(request.Buyer.TaxIdentifier, "Buyer.TaxIdentifier"),
                Seller = MapParty(request.Seller),
                Buyer = MapParty(request.Buyer),
                FacturaNo = RequireText(request.DocumentNumber, nameof(request.DocumentNumber)),
                FacturaDate = request.DocumentDate,
                ContractNo = RequireText(request.ContractNumber, nameof(request.ContractNumber)),
                ContractDate = request.ContractDate
                    ?? throw new InvalidOperationException("ContractDate is required for Didox factura creation."),
                Empowerment = MapEmpowerment(request.Empowerment),
                Products = request.Lines.Select(MapProduct).ToList(),
                IdempotencyKey = request.IdempotencyKey
            },
            ct);

        return new EdoOutboxCreateDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = result.ProviderDocumentId,
                LegacyDocumentId = result.MarkingDidoxDocumentId,
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
        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7)
            && string.IsNullOrWhiteSpace(request.SignatureHex))
        {
            var challenge = await facturaService.GetSignChallengeAsync(id, ct);
            return new EdoOutboxSignDto
            {
                Document = new EdoDocumentDto
                {
                    LegacyDocumentId = id,
                    Direction = EdoDirection.OUTBOX,
                    DocumentType = "FACTURA"
                },
                SigningSession = new EdoSigningSessionDto
                {
                    SigningMode = EdoSigningMode.TimestampedSignature,
                    DocumentId = id.ToString(CultureInfo.InvariantCulture),
                    Payload = challenge.DataBase64,
                    PayloadFormat = "Base64Utf8",
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
                }
            };
        }

        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7)
            || string.IsNullOrWhiteSpace(request.SignatureHex))
        {
            throw new InvalidOperationException("Didox signing requires both PreparedPkcs7 and SignatureHex.");
        }

        var result = await facturaService.SignFacturaDocumentAsync(
            new DidoxFacturaSignRequestDto
            {
                MarkingDidoxDocumentId = id,
                Pkcs7 = request.PreparedPkcs7,
                SignatureHex = request.SignatureHex
            },
            ct);

        return new EdoOutboxSignDto
        {
            Document = new EdoDocumentDto
            {
                LegacyDocumentId = result.MarkingDidoxDocumentId,
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

    public Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentType,
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default) =>
        edoOperations.RejectInboxAsync(providerDocumentId, request, ct);

    public Task<EdoFileDto> GetFileAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetFileAsync(providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetStatusAsync(providerDocumentId, EdoDirection.OUTBOX, ct);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetStatusAsync(providerDocumentId, EdoDirection.INBOX, ct);

    private static DidoxFacturaPartyDto MapParty(EdoPartyDto party) => new()
    {
        Name = RequireText(party.Name, "Party.Name"),
        VatRegCode = RequireText(party.TaxIdentifier, "Party.TaxIdentifier"),
        VatRegStatus = RequireText(party.VatRegistrationStatus, "Party.VatRegistrationStatus"),
        Account = RequireText(party.AccountNumber, "Party.AccountNumber"),
        BankId = RequireText(party.BankCode, "Party.BankCode"),
        Address = RequireText(party.Address, "Party.Address"),
        Director = party.DirectorName,
        Accountant = party.AccountantName,
        BranchCode = party.BranchCode,
        BranchName = party.BranchName
    };

    private static DidoxFacturaEmpowermentDto MapEmpowerment(EdoEmpowermentDto? empowerment) =>
        empowerment is null
            ? throw new InvalidOperationException("Empowerment is required for Didox factura creation.")
            : new DidoxFacturaEmpowermentDto
            {
                EmpowermentNo = RequireText(empowerment.EmpowermentNumber, "Empowerment.EmpowermentNumber"),
                EmpowermentDateOfIssue = empowerment.DateOfIssue,
                AgentFio = RequireText(empowerment.AgentName, "Empowerment.AgentName"),
                AgentPinfl = RequireText(empowerment.AgentPinfl, "Empowerment.AgentPinfl")
            };

    private static DidoxFacturaProductRequestDto MapProduct(EdoFacturaLineDto line) => new()
    {
        OrdNo = line.Number,
        Name = RequireText(line.Name, "Line.Name"),
        CatalogCode = RequireText(line.CatalogCode, "Line.CatalogCode"),
        CatalogName = RequireText(line.CatalogName, "Line.CatalogName"),
        PackageCode = RequireText(line.UnitCode, "Line.UnitCode"),
        PackageName = RequireText(line.UnitName, "Line.UnitName"),
        Count = line.Quantity.ToString(CultureInfo.InvariantCulture),
        Summa = line.Amount.ToString(CultureInfo.InvariantCulture),
        VatRate = (line.TaxRate ?? 0m).ToString(CultureInfo.InvariantCulture),
        VatSum = (line.TaxAmount ?? 0m).ToString(CultureInfo.InvariantCulture),
        WithoutVat = line.IsTaxFree,
        Origin = 0,
        MarkingCodeIds = line.MarkingCodeIds.ToList()
    };

    private static string RequireText(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for Didox factura creation.")
            : value;

    private Task<T> ThrowUnavailable<T>(EdoCapabilityKind capability) =>
        throw new EdoCapabilityUnavailableException(
            Code.ToString(),
            capability.ToString(),
            EdoCapabilityStatus.UNKNOWN.ToString());

    private static EdoProviderCapabilityDto CreateCapabilities() =>
        new()
        {
            ProviderCode = EdoProviderCode.DIDOX,
            DisplayName = "Didox",
            AuthModes = [EdoAuthMode.EImzo],
            SigningModes = [EdoSigningMode.TimestampedSignature],
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
                            or EdoCapabilityKind.RejectInbox
                            or EdoCapabilityKind.GetFile
                            or EdoCapabilityKind.GetOutboxStatus
                            or EdoCapabilityKind.GetInboxStatus => EdoCapabilityStatus.SUPPORTED,
                        EdoCapabilityKind.SearchFilter
                            or EdoCapabilityKind.Marking => EdoCapabilityStatus.PARTIAL,
                        _ => EdoCapabilityStatus.UNKNOWN
                    }
                })
                .ToList()
        };
}
