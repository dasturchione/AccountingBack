using Integration.AslBelgi.Abstractions;
using Integration.AslBelgi.Configs;
using Integration.AslBelgi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTests;

// Proves the Asl Belgisi token lifecycle without real credentials or network:
// technical login/refresh/re-auth, business apiKey, caching, and credential guards.
public sealed class AslBelgiTokenProviderTests
{
    private const string AccessTokenCacheKey = "AslBelgi:AccessToken";

    [Fact]
    public async Task Technical_FirstCall_Authenticates_ThenCaches()
    {
        var auth = new FakeAuthClient();
        var cache = NewCache();
        var provider = CreateProvider(auth, cache, TechnicalSettings());

        var first = await provider.GetAccessTokenAsync();
        var second = await provider.GetAccessTokenAsync();

        Assert.True(first.IsSuccess);
        Assert.Equal("access-1", first.Value);
        Assert.Equal("access-1", second.Value);
        Assert.Equal(1, auth.AuthenticateCount); // second call served from cache
        Assert.Equal(0, auth.RefreshCount);
    }

    [Fact]
    public async Task Technical_AccessExpired_RefreshTokenPresent_Refreshes()
    {
        var auth = new FakeAuthClient();
        var cache = NewCache();
        var provider = CreateProvider(auth, cache, TechnicalSettings());

        await provider.GetAccessTokenAsync();          // authenticate + cache access/refresh
        cache.Remove(AccessTokenCacheKey);             // simulate 30-min access expiry (refresh still valid)

        var refreshed = await provider.GetAccessTokenAsync();

        Assert.True(refreshed.IsSuccess);
        Assert.Equal("access-2", refreshed.Value);
        Assert.Equal(1, auth.AuthenticateCount);
        Assert.Equal(1, auth.RefreshCount);
    }

    [Fact]
    public async Task Technical_RefreshFails_FallsBackToAuthenticate()
    {
        var auth = new FakeAuthClient { RefreshThrows = true };
        var cache = NewCache();
        var provider = CreateProvider(auth, cache, TechnicalSettings());

        await provider.GetAccessTokenAsync();  // authenticate #1
        cache.Remove(AccessTokenCacheKey);

        var reauth = await provider.GetAccessTokenAsync(); // refresh throws -> authenticate #2

        Assert.True(reauth.IsSuccess);
        Assert.Equal(1, auth.RefreshCount);
        Assert.Equal(2, auth.AuthenticateCount);
    }

    [Fact]
    public async Task Technical_MissingCredentials_FailsWithoutCallingAuth()
    {
        var auth = new FakeAuthClient();
        var settings = TechnicalSettings();
        settings.Login = "SET_VIA_ENVIRONMENT";
        var provider = CreateProvider(auth, NewCache(), settings);

        var result = await provider.GetAccessTokenAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.CredentialsNotConfigured", result.Error.Code);
        Assert.Equal(0, auth.AuthenticateCount);
    }

    [Fact]
    public async Task Business_ReturnsApiKey_WithoutCallingAuth()
    {
        var auth = new FakeAuthClient();
        var settings = TechnicalSettings();
        settings.AuthMode = "business";
        settings.ApiKey = "party-api-key";

        var provider = CreateProvider(auth, NewCache(), settings);

        var result = await provider.GetAccessTokenAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("party-api-key", result.Value);
        Assert.Equal(0, auth.AuthenticateCount);
    }

    [Fact]
    public async Task Business_MissingApiKey_Fails()
    {
        var settings = TechnicalSettings();
        settings.AuthMode = "business";
        settings.ApiKey = "SET_VIA_ENVIRONMENT";

        var provider = CreateProvider(new FakeAuthClient(), NewCache(), settings);

        var result = await provider.GetAccessTokenAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.CredentialsNotConfigured", result.Error.Code);
    }

    private static AslBelgiSettings TechnicalSettings() => new()
    {
        AuthMode = "technical",
        Login = "tech-login",
        Password = "tech-password",
        AccessTokenSafetyMarginSeconds = 60
    };

    private static MemoryCache NewCache() => new(new MemoryCacheOptions());

    private static AslBelgiTokenProvider CreateProvider(IAslBelgiAuthClient auth, IMemoryCache cache, AslBelgiSettings settings)
        => new(auth, cache, Options.Create(settings), NullLogger<AslBelgiTokenProvider>.Instance);

    private sealed class FakeAuthClient : IAslBelgiAuthClient
    {
        public int AuthenticateCount;
        public int RefreshCount;
        public bool RefreshThrows;

        public Task<AslBelgiAuthResponse> AuthenticateAsync(AslBelgiAuthRequest request, CancellationToken ct = default)
        {
            AuthenticateCount++;
            return Task.FromResult(Response("access-1", "rt-1"));
        }

        public Task<AslBelgiAuthResponse> RefreshAsync(AslBelgiRefreshRequest request, CancellationToken ct = default)
        {
            RefreshCount++;
            if (RefreshThrows)
                throw new InvalidOperationException("refresh failed");
            return Task.FromResult(Response("access-2", "rt-2"));
        }

        private static AslBelgiAuthResponse Response(string access, string refresh) => new()
        {
            AccessToken = access,
            AccessTokenType = "BEARER",
            AccessTokenExpiresIn = 1_800_000,
            RefreshToken = refresh
        };
    }
}
