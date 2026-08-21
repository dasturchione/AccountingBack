using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Features.SaleDocs.EdoSalePreflight;

public static class EdoSalePreflightPlanHash
{
    public static string Compute(
        IReadOnlyCollection<EdoSalePreflightCandidateDto> candidates,
        IReadOnlyCollection<string>? planCodes = null)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            ProviderCode = "EDOCS",
            SourceLinkStatus = "BLOCKED",
            PlanCodes = (planCodes ?? []).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            Candidates = candidates
                .OrderBy(x => x.ProviderDocumentId ?? string.Empty, StringComparer.Ordinal)
                .ToArray()
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
