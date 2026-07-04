using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public interface ITaxResolverService
{
    Task<Result<TaxResolutionResultDto>> ResolveAsync(int organizationId, short taxTypeId, DateOnly? effectiveDate = null, CancellationToken ct = default);
}
