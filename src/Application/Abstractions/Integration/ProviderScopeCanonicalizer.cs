using System.Text;

namespace Application.Abstractions.Integration;

/// <summary>
/// Canonicalizes the provider scope without accepting caller-controlled tenant fields.
/// E-DOCS EntityId is the certificate serial identity; it is trimmed and Unicode-normalized
/// while preserving the serial's case.
/// </summary>
public static class ProviderScopeCanonicalizer
{
    public static OrganizationScope Canonicalize(OrganizationScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return scope.Provider == Domain.Entities.Provider.EDocs
            ? scope with { EntityId = NormalizeEdocsEntityId(scope.EntityId) }
            : scope;
    }

    public static string? NormalizeEdocsEntityId(string? entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            return null;

        var normalized = entityId.Trim().Normalize(NormalizationForm.FormC);
        return normalized.Length == 0 ? null : normalized;
    }
}
