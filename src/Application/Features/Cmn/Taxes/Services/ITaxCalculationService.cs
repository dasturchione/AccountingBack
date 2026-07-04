using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public interface ITaxCalculationService
{
    Task<Result<TaxCalculationResultDto>> CalculateAsync(TaxCalculationRequestDto request, CancellationToken ct = default);
    Task<Result> ValidateAsync(TaxBusinessValidationRequestDto request, CancellationToken ct = default);
}
