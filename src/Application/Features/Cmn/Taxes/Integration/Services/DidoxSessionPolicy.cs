namespace Application.Features.Cmn.Taxes.Integration.Services;

public static class DidoxSessionPolicy
{
    public static readonly TimeSpan AccessTokenTtl = TimeSpan.FromMinutes(360);

    public static DateTime GetAccessExpiresAtUtc(DateTime issuedAtUtc)
        => issuedAtUtc.Add(AccessTokenTtl);
}
