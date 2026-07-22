using Application.Features.Integration.AslBelgi.DTOs;
using System.Text.Json;

namespace Application.Features.Integration.AslBelgi.Services;

public interface IAslBelgiVerificationService
{
    Task<JsonElement> GetPublicCodeInformationAsync(MarkingCodeCheckRequestDto request, CancellationToken ct = default);
    Task<JsonElement> GetPrivateCodeInformationAsync(MarkingCodeCheckRequestDto request, CancellationToken ct = default);
    Task<JsonElement> GetProductsByGtinAsync(ProductRegistryByGtinRequestDto request, CancellationToken ct = default);
    Task<CounterpartyStatusResponseDto?> GetCounterpartyStatusAsync(string tin, CancellationToken ct = default);
}
