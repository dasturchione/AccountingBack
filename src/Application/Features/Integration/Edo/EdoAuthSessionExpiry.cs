namespace Application.Features.Integration.Edo;

public static class EdoAuthSessionExpiry
{
    public static readonly TimeSpan MinimumChallengeLifetime = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan MaximumChallengeLifetime = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan MinimumAuthenticatedSessionLifetime = TimeSpan.FromHours(6);
    public static readonly TimeSpan MaximumAuthenticatedSessionLifetime = TimeSpan.FromHours(24);

    public static DateTimeOffset Calculate(
        DateTimeOffset now,
        TimeSpan defaultLifetime,
        DateTimeOffset? providerExpiresAt)
    {
        if (defaultLifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(defaultLifetime));

        var defaultExpiry = now.Add(defaultLifetime);
        return providerExpiresAt is { } providerExpiry
            ? (providerExpiry <= defaultExpiry ? providerExpiry : defaultExpiry)
            : defaultExpiry;
    }

    public static DateTimeOffset CapAuthenticatedExpiry(
        DateTimeOffset now,
        DateTimeOffset providerExpiresAt)
    {
        if (providerExpiresAt <= now)
            throw new ArgumentOutOfRangeException(nameof(providerExpiresAt));

        var maximumExpiry = now.Add(MaximumAuthenticatedSessionLifetime);
        return providerExpiresAt <= maximumExpiry ? providerExpiresAt : maximumExpiry;
    }

    public static bool IsChallengeExpiryAllowed(DateTimeOffset now, DateTimeOffset expiresAt) =>
        expiresAt > now
        && expiresAt - now >= MinimumChallengeLifetime
        && expiresAt - now <= MaximumChallengeLifetime;

    public static bool IsAuthenticatedSessionExpiryAllowed(
        DateTimeOffset now,
        DateTimeOffset expiresAt) =>
        expiresAt > now
        && expiresAt - now >= MinimumAuthenticatedSessionLifetime
        && expiresAt - now <= MaximumAuthenticatedSessionLifetime;
}
