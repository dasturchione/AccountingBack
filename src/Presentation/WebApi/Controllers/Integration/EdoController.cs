using Application.Abstractions.Integration.Edo;
using Application.Abstractions.Integration.Faktura;
using Application.Features.Integration.Edo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/edo")]
[ApiController]
[Authorize]
public sealed class EdoController(
    IEdoProviderManagementService service,
    IEdoAuthenticationService authenticationService,
    IEdoOutboxService outboxService,
    IEdoInboxService inboxService) : ControllerBase
{
    [HttpGet("active-provider")]
    public async Task<IResult> GetActiveProvider(CancellationToken ct = default) =>
        Results.Ok(ToSelection(await service.GetActiveProviderAsync(ct)));

    [HttpPut("active-provider")]
    public async Task<IResult> SetActiveProvider(
        [FromBody] EdoActiveProviderRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(ToSelection(await service.SetActiveProviderAsync(request, ct)));

    [HttpGet("auth/challenge")]
    public async Task<IResult> GetAuthChallenge(
        [FromQuery] EdoAuthChallengeRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await authenticationService.GetChallengeAsync(request, ct));

    [HttpPost("auth/complete")]
    public async Task<IResult> CompleteAuth(
        [FromBody] EdoAuthCompleteRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await authenticationService.CompleteAsync(request, ct));

    [HttpPost("auth/faktura/complete")]
    public async Task<IResult> CompleteFakturaAuth(
        [FromBody] FakturaAuthCompleteRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await authenticationService.CompleteFakturaAsync(request, ct));

    [HttpPost("outbox/facturas")]
    public async Task<IResult> CreateFactura(
        [FromBody] EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await outboxService.CreateFacturaAsync(request, ct));

    [HttpPost("outbox/{id:long}/sign")]
    public async Task<IResult> SignOutbox(
        long id,
        [FromBody] EdoOutboxSignRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await outboxService.SignAsync(id, request, ct));

    [HttpGet("inbox")]
    public async Task<IResult> GetInbox(
        [FromQuery] EdoInboxQueryDto request,
        CancellationToken ct = default) =>
        Results.Ok(await inboxService.ListInboxAsync(request, ct));

    [HttpPost("inbox/{id:long}/reject")]
    public async Task<IResult> RejectInbox(
        long id,
        [FromBody] EdoInboxRejectRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await inboxService.RejectAsync(id, request, ct));

    [HttpGet("files/{id:long}")]
    public async Task<IResult> GetFile(long id, CancellationToken ct = default)
    {
        var file = await inboxService.GetFileAsync(id, ct);
        return Results.Stream(file.Content, file.ContentType, file.FileName, enableRangeProcessing: true);
    }

    [HttpGet("outbox/{id:long}/status")]
    public async Task<IResult> GetOutboxStatus(long id, CancellationToken ct = default) =>
        Results.Ok(await inboxService.GetStatusAsync(id, EdoDirection.OUTBOX, ct));

    [HttpGet("inbox/{id:long}/status")]
    public async Task<IResult> GetInboxStatus(long id, CancellationToken ct = default) =>
        Results.Ok(await inboxService.GetStatusAsync(id, EdoDirection.INBOX, ct));

    private static EdoProviderSelectionDto ToSelection(EdoProviderDto provider) => new()
    {
        Id = EdoProviderFrontendCatalog.GetId(provider.ProviderCode),
        Name = provider.DisplayName,
        Code = provider.ProviderCode.ToString()
    };
}
