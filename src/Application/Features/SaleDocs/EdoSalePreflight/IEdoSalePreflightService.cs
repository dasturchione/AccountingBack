using Application.Features.SaleDocs.EdoSalePreflight;

namespace Application.Features.SaleDocs;

public interface IEdoSalePreflightService
{
    Task<EdoSalePreflightPlanDto> GetPlanAsync(CancellationToken ct = default);
}
