using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Integration.AslBelgi.DTOs;
using Application.Features.Integration.AslBelgi.Transfers;
using Domain.Entities;
using Integration.AslBelgi.Http;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.AslBelgi.Transfers;

public sealed class AslBelgiTransferService : IAslBelgiTransferService
{
    private const string RequestOperation = "CRPT_TRANSFER_REQUEST";
    private const string ConfirmationOperation = "CRPT_TRANSFER_CONFIRMATION";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditLogService _auditLogService;

    public AslBelgiTransferService(AppDbContext context, IUserContext userContext, IHttpClientFactory httpClientFactory, IAuditLogService auditLogService)
    {
        _context = context;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _auditLogService = auditLogService;
    }

    public Task<JsonElement> SearchTransferRequestsAsync(CrptTransferSearchRequestDto request, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Get, $"public/api/v1/doc/transfer/request/search{BuildSearchQuery(request)}", null, ct);

    public Task<JsonElement> GetDocumentStatusAsync(string documentId, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Get, $"public/api/v1/doc/storage/docs/{Uri.EscapeDataString(documentId)}", null, ct);

    public async Task<CrptTransferWriteResultDto> SubmitTransferRequestAsync(CrptTransferRequestSubmitDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var body = ParseTransferBody(request.SignedPayload.DocumentBody);
        var seller = await RequireParticipantAsync(request.SellerCounterpartyId, organizationId, "seller", ct);
        var buyer = await RequireParticipantAsync(request.BuyerCounterpartyId, organizationId, "buyer", ct);
        if (body.OwnerId != seller.CrptParticipantId || body.BuyerId != buyer.CrptParticipantId)
            throw new InvalidOperationException("Signed CRPT transfer document participant IDs do not match the selected counterparties.");

        var hash = ComputeHash(RequestOperation, request.SignedPayload.DocumentBody, request.SellerCounterpartyId, request.BuyerCounterpartyId);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, RequestOperation, hash, body.ExternalDocumentReference, ct);
        if (replay is not null)
            return replay;

        try
        {
            var response = await SendJsonAsync(HttpMethod.Post, "public/api/v1/doc/transfer/request", request.SignedPayload, ct);
            var documentId = response.GetProperty("documentId").GetString();
            if (string.IsNullOrWhiteSpace(documentId))
                throw new IntegrationHttpException("CRPT transfer request response did not include a document ID.", 502);

            var transfer = new MarkingTransfer
            {
                OrganizationId = organizationId,
                SellerCounterpartyId = seller.Id,
                BuyerCounterpartyId = buyer.Id,
                TransferDate = body.BusinessDatetime.UtcDateTime,
                DocumentId = documentId,
                Status = "SUBMITTED",
                ExternalDocumentReference = body.ExternalDocumentReference,
                CreatedDate = DateTime.UtcNow
            };
            _context.MarkingTransfers.Add(transfer);
            await _context.SaveChangesAsync(ct);

            var singleGtin = body.Products.Count == 1 ? body.Products[0].Gtin : null;
            _context.MarkingTransferCodes.AddRange(body.Codes.Select(code => new MarkingTransferCode
            {
                OrganizationId = organizationId,
                MarkingTransferId = transfer.Id,
                MarkingCode = code,
                Gtin = singleGtin,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow
            }));
            await CompleteIdempotencyAsync(organizationId, request.IdempotencyKey, documentId, "COMPLETED", ct);
            await _context.SaveChangesAsync(ct);

            _auditLogService.SetNewValues(new { transfer.Id, transfer.DocumentId, transfer.Status });
            await _auditLogService.CreateAsync("marking_transfer", transfer.Id.ToString(), AuditLogOperationTypeConst.Create);
            return new CrptTransferWriteResultDto { DocumentId = documentId, Status = transfer.Status };
        }
        catch
        {
            await MarkFailedAsync(organizationId, request.IdempotencyKey, ct);
            throw;
        }
    }

    public async Task<CrptTransferWriteResultDto> SubmitTransferConfirmationAsync(CrptTransferConfirmationSubmitDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var body = ParseConfirmationBody(request.SignedPayload.DocumentBody);
        var hash = ComputeHash(ConfirmationOperation, request.SignedPayload.DocumentBody, 0, 0);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, ConfirmationOperation, hash, body.TransferRequestDocId, ct);
        if (replay is not null)
            return replay;

        var transfer = await _context.MarkingTransfers.SingleOrDefaultAsync(x => x.DocumentId == body.TransferRequestDocId, ct)
            ?? throw new InvalidOperationException("The CRPT transfer request is not available in the current organization.");
        try
        {
            var response = await SendJsonAsync(HttpMethod.Post, "public/api/v1/doc/transfer/confirmation", request.SignedPayload, ct);
            var documentId = response.GetProperty("documentId").GetString();
            if (string.IsNullOrWhiteSpace(documentId))
                throw new IntegrationHttpException("CRPT transfer confirmation response did not include a document ID.", 502);

            transfer.Status = body.Resolution;
            transfer.UpdatedDate = DateTime.UtcNow;
            await CompleteIdempotencyAsync(organizationId, request.IdempotencyKey, documentId, "COMPLETED", ct);
            await _context.SaveChangesAsync(ct);
            _auditLogService.SetNewValues(new { transfer.Id, transfer.Status, ConfirmationDocumentId = documentId });
            await _auditLogService.CreateAsync("marking_transfer", transfer.Id.ToString(), AuditLogOperationTypeConst.Update, "CRPT confirmation");
            return new CrptTransferWriteResultDto { DocumentId = documentId, Status = transfer.Status };
        }
        catch
        {
            await MarkFailedAsync(organizationId, request.IdempotencyKey, ct);
            throw;
        }
    }

    private async Task<CrptTransferWriteResultDto?> TryCreateIdempotencyRecordAsync(int organizationId, string key, string operation, string hash, string? reference, CancellationToken ct)
    {
        var existing = await _context.IdempotencyRecords.SingleOrDefaultAsync(x => x.IdempotencyKey == key, ct);
        if (existing is not null)
            return ReplayOrReject(existing, operation, hash);

        _context.IdempotencyRecords.Add(new IdempotencyRecord { OrganizationId = organizationId, IdempotencyKey = key, OperationType = operation, RequestHash = hash, RequestReference = reference, Status = "PENDING", CreatedDate = DateTime.UtcNow });
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();
            existing = await _context.IdempotencyRecords.SingleAsync(x => x.IdempotencyKey == key, ct);
            return ReplayOrReject(existing, operation, hash);
        }
        return null;
    }

    private static CrptTransferWriteResultDto ReplayOrReject(IdempotencyRecord record, string operation, string hash)
    {
        if (record.OperationType != operation || record.RequestHash != hash)
            throw new InvalidOperationException("The idempotency key has already been used for a different request.");
        if (record.Status == "COMPLETED" && !string.IsNullOrWhiteSpace(record.ResultDocumentId))
            return new CrptTransferWriteResultDto { DocumentId = record.ResultDocumentId, Status = record.Status, IsReplay = true };
        throw new InvalidOperationException("A request with this idempotency key is already in progress or requires reconciliation.");
    }

    private async Task CompleteIdempotencyAsync(int orgId, string key, string documentId, string status, CancellationToken ct)
    {
        var record = await _context.IdempotencyRecords.SingleAsync(x => x.OrganizationId == orgId && x.IdempotencyKey == key, ct);
        record.ResultDocumentId = documentId; record.Status = status; record.UpdatedDate = DateTime.UtcNow;
    }
    private async Task MarkFailedAsync(int orgId, string key, CancellationToken ct)
    {
        var record = await _context.IdempotencyRecords.SingleOrDefaultAsync(x => x.OrganizationId == orgId && x.IdempotencyKey == key, ct);
        if (record is null || record.Status == "COMPLETED") return;
        record.Status = "FAILED"; record.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
    private async Task<CounterpartyCard> RequireParticipantAsync(int id, int orgId, string role, CancellationToken ct)
    {
        var counterparty = await _context.CounterpartyCards.SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == orgId, ct)
            ?? throw new InvalidOperationException($"The {role} counterparty is not available in the current organization.");
        if (!counterparty.CrptParticipantId.HasValue)
            throw new InvalidOperationException($"The {role} counterparty does not have a CRPT participant ID. CRPT support'dan aniqlanishi kerak.");
        return counterparty;
    }
    private int RequireOrganization() => _userContext.OrganizationId ?? throw new InvalidOperationException("An active organization is required for CRPT transfer operations.");
    private static string ComputeHash(string operation, string documentBody, int sellerId, int buyerId) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{operation}|{sellerId}|{buyerId}|{documentBody}")));
    private static CrptTransferDocument ParseTransferBody(string documentBody) => Parse<CrptTransferDocument>(documentBody, x => x.BuyerId > 0 && x.OwnerId > 0 && x.BusinessDatetime.Offset == TimeSpan.Zero && x.Products.Count > 0 && x.Codes.Count > 0);
    private static CrptConfirmationDocument ParseConfirmationBody(string documentBody) => Parse<CrptConfirmationDocument>(documentBody, x => !string.IsNullOrWhiteSpace(x.TransferRequestDocId) && !string.IsNullOrWhiteSpace(x.Resolution));
    private static T Parse<T>(string body, Func<T, bool> valid)
    {
        try { var value = JsonSerializer.Deserialize<T>(Convert.FromBase64String(body), JsonOptions); if (value is not null && valid(value)) return value; }
        catch (Exception ex) when (ex is FormatException or JsonException) { }
        throw new InvalidOperationException("The signed CRPT documentBody does not match the documented transfer contract.");
    }
    private async Task<JsonElement> SendJsonAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path) { Content = body is null ? null : JsonContent.Create(body, options: JsonOptions) };
        using var response = await _httpClientFactory.CreateClient(AslBelgiHttpClientNames.Client).SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        EnsureSuccess(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct); using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct); return document.RootElement.Clone();
    }
    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        throw response.StatusCode switch { HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("CRPT credentials were rejected."), HttpStatusCode.Forbidden => new IntegrationForbiddenException("CRPT denied the request."), _ => new IntegrationHttpException($"CRPT request failed with HTTP status {(int)response.StatusCode}.", (int)response.StatusCode) };
    }
    private static string BuildSearchQuery(CrptTransferSearchRequestDto request)
    {
        var values = new List<KeyValuePair<string, string?>> { new("documentId", request.DocumentId), new("ownerId", request.OwnerId?.ToString()), new("buyerId", request.BuyerId?.ToString()), new("dateFrom", request.DateFrom?.ToUniversalTime().ToString("O")), new("dateTo", request.DateTo?.ToUniversalTime().ToString("O")), new("businessDateFrom", request.BusinessDateFrom?.ToUniversalTime().ToString("O")), new("businessDateTo", request.BusinessDateTo?.ToUniversalTime().ToString("O")), new("limit", request.Limit?.ToString()), new("cursor", request.Cursor) };
        values.AddRange((request.Statuses ?? []).Select(x => new KeyValuePair<string, string?>("statuses", x))); values.AddRange((request.ConfirmationStatuses ?? []).Select(x => new KeyValuePair<string, string?>("confirmationStatuses", x)));
        var query = values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"); return query.Any() ? "?" + string.Join("&", query) : string.Empty;
    }
    private sealed class CrptTransferDocument { public int BuyerId { get; init; } public int OwnerId { get; init; } public DateTimeOffset BusinessDatetime { get; init; } public List<CrptTransferProduct> Products { get; init; } = []; public List<string> Codes { get; init; } = []; public string? ExternalDocumentReference { get; init; } }
    private sealed class CrptTransferProduct { public string Gtin { get; init; } = string.Empty; public int Quantity { get; init; } }
    private sealed class CrptConfirmationDocument { public string TransferRequestDocId { get; init; } = string.Empty; public string Resolution { get; init; } = string.Empty; }
}
