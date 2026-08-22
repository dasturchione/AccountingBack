using SharedKernel.Results;

namespace Application.Features.SaleDocs.EdoSalePreflight;

public interface IEdoSaleDraftApplyService
{
    Task<Result<EdoSaleDraftApplyResponseDto>> ApplyAsync(
        EdoSaleDraftApplyRequestDto request,
        CancellationToken ct = default);
}
