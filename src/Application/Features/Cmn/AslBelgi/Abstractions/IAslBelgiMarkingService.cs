using Application.Features.Cmn.AslBelgi.DTOs;
using SharedKernel.Results;

namespace Application.Features.Cmn.AslBelgi.Abstractions;

/// <summary>
/// Asl Belgisi marking business logic: register a KM emission order, then pull the issued codes and
/// bind each one to a ProductTable instance (idempotently).
/// </summary>
public interface IAslBelgiMarkingService
{
    Task<Result<AslBelgiOrderResponse>> RequestMarkingAsync(AslBelgiMarkingRequestDto request, CancellationToken ct = default);

    Task<Result<AslBelgiBindResultDto>> FetchAndBindCodesAsync(AslBelgiBindCodesRequestDto request, CancellationToken ct = default);
}
