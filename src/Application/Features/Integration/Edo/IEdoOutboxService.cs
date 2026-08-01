using Application.Abstractions.Integration.Edo;

namespace Application.Features.Integration.Edo;

public interface IEdoOutboxService
{
    Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default);

    Task<EdoOutboxSignDto> SignAsync(
        long id,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default);
}
