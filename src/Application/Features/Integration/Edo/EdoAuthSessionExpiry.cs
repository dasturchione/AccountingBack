namespace Application.Features.Integration.Edo;

public static class EdoAuthSessionExpiry
{
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
}
