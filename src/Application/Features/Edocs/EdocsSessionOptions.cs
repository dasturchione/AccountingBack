namespace Application.Features.Edocs;

/// <summary>
/// Session lifetime policy for E-DOCS, resolved from the existing <c>Edocs:TokenTtlMinutes</c>
/// configuration value by the infrastructure registration. Out-of-range or invalid values fall
/// back to the safe default so a misconfiguration can never produce a never-expiring session.
/// No refresh flow exists: an expired session always requires a fresh E-IMZO login.
/// </summary>
public sealed class EdocsSessionOptions
{
    /// <summary>Matches the documented <c>Edocs:TokenTtlMinutes</c> convention (23 hours).</summary>
    public const int DefaultTokenTtlMinutes = 1380;

    /// <summary>Hard upper bound (30 days); anything above is treated as misconfiguration.</summary>
    public const int MaxTokenTtlMinutes = 43_200;

    public TimeSpan AccessTokenTtl { get; init; } = TimeSpan.FromMinutes(DefaultTokenTtlMinutes);

    public static EdocsSessionOptions FromMinutes(int minutes) => new()
    {
        AccessTokenTtl = TimeSpan.FromMinutes(
            minutes >= 1 && minutes <= MaxTokenTtlMinutes ? minutes : DefaultTokenTtlMinutes)
    };
}
