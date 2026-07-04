using Application.Abstractions.Integration;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes.Integration.Services;

public interface ITaxIntegrationService
{
    Task<Result<IReadOnlyCollection<TaxProviderInfoDto>>> GetSupportedProvidersAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<TaxProviderStatusDto>>> GetProviderStatusAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<TaxProviderInfoDto>>> GetLookupProvidersAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<TaxProviderInfoDto>>> GetDocumentProvidersAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<TaxLookupItemDto>>> SearchMxikAsync(TaxLookupRequestDto request, CancellationToken ct = default);
    Task<Result<TaxLookupItemDto?>> GetMxikByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<TaxLookupItemDto>>> SearchSoliqAsync(TaxLookupRequestDto request, CancellationToken ct = default);
    Task<Result<TaxDocumentResultDto>> SubmitEFakturaAsync(TaxDocumentRequestDto request, CancellationToken ct = default);
    Task<Result<TaxDocumentResultDto>> GetEFakturaStatusAsync(TaxDocumentRequestDto request, CancellationToken ct = default);
    Task<Result<TaxDocumentResultDto>> CancelEFakturaAsync(TaxDocumentRequestDto request, CancellationToken ct = default);
    Task<Result<TaxDocumentResultDto>> SubmitDidoxAsync(TaxDocumentRequestDto request, CancellationToken ct = default);
    Task<Result<TaxDocumentResultDto>> GetDidoxStatusAsync(TaxDocumentRequestDto request, CancellationToken ct = default);
    Task<Result<TaxDocumentResultDto>> CancelDidoxAsync(TaxDocumentRequestDto request, CancellationToken ct = default);
}
