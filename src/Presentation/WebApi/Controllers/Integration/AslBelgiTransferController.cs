using Application.Features.Integration.AslBelgi.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/asl-belgi/transfers")]
[ApiController]
[Authorize]
public sealed class AslBelgiTransferController(IAslBelgiTransferService service) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> Search([FromQuery] CrptTransferSearchRequestDto request, CancellationToken ct = default)
        => Results.Json(await service.SearchTransferRequestsAsync(request, ct));

    [HttpGet("documents/{documentId}")]
    public async Task<IResult> GetDocumentStatus([FromRoute] string documentId, CancellationToken ct = default)
        => Results.Json(await service.GetDocumentStatusAsync(documentId, ct));

    [HttpPost("request")]
    public async Task<IResult> SubmitRequest([FromBody] CrptTransferRequestSubmitDto request, CancellationToken ct = default)
        => Results.Ok(await service.SubmitTransferRequestAsync(request, ct));

    [HttpPost("confirmation")]
    public async Task<IResult> SubmitConfirmation([FromBody] CrptTransferConfirmationSubmitDto request, CancellationToken ct = default)
        => Results.Ok(await service.SubmitTransferConfirmationAsync(request, ct));
}
