using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.Errors;
using Integration.AslBelgi.Abstractions;
using Integration.AslBelgi.Configs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Results;

namespace Integration.AslBelgi.Services;

/// <summary>
/// Thread-safe Asl Belgisi token lifecycle: caches the technical access token, refreshes it before
/// expiry, and re-authenticates when the refresh token is gone. Business mode returns the configured
/// apiKey directly. The apiKey/login/password are never generated here — they come from configuration
/// (⏳ waiting on the accountant), so a missing credential yields a clear failure instead of a call.
/// </summary>
public sealed class AslBelgiTokenProvider : IAslBelgiTokenProvider
{
    private const string BusinessMode = "business";
    private const string AccessTokenCacheKey = "AslBelgi:AccessToken";
    private const string RefreshTokenCacheKey = "AslBelgi:RefreshToken";
    private const string PlaceholderMarker = "SET_VIA_ENVIRONMENT";

    // Static so token acquisition is serialized process-wide even though the provider is scoped.
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly IAslBelgiAuthClient _authClient;
    private readonly IMemoryCache _cache;
    private readonly AslBelgiSettings _settings;
    private readonly ILogger<AslBelgiTokenProvider> _logger;

    public AslBelgiTokenProvider(
        IAslBelgiAuthClient authClient,
        IMemoryCache cache,
        IOptions<AslBelgiSettings> settings,
        ILogger<AslBelgiTokenProvider> logger)
    {
        _authClient = authClient;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (IsBusinessMode)
        {
            return IsConfigured(_settings.ApiKey)
                ? Result.Success(_settings.ApiKey)
                : Result.Failure<string>(AslBelgiErrors.CredentialsNotConfigured(
                    "Asl Belgisi apiKey sozlanmagan (business rejim). apiKey ЛК'da E-IMZO bilan olinib, environmentга qo'yilishi kerak."));
        }

        if (!IsConfigured(_settings.Login) || !IsConfigured(_settings.Password))
        {
            return Result.Failure<string>(AslBelgiErrors.CredentialsNotConfigured(
                "Asl Belgisi login/parol sozlanmagan (texnik rejim). Credentials'ni environmentга qo'ying."));
        }

        if (_cache.TryGetValue(AccessTokenCacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
            return Result.Success(cached!);

        await Gate.WaitAsync(ct);
        try
        {
            // Double-check: another caller may have refreshed while we waited on the gate.
            if (_cache.TryGetValue(AccessTokenCacheKey, out string? fresh) && !string.IsNullOrWhiteSpace(fresh))
                return Result.Success(fresh!);

            return await AcquireTokenAsync(ct);
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<Result<string>> AcquireTokenAsync(CancellationToken ct)
    {
        // Prefer refresh when a refresh token is still cached (24h), otherwise authenticate.
        if (_cache.TryGetValue(RefreshTokenCacheKey, out string? refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            try
            {
                var refreshed = await _authClient.RefreshAsync(new AslBelgiRefreshRequest { RefreshToken = refreshToken! }, ct);
                return Store(refreshed);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Asl Belgisi token refresh failed; re-authenticating with login/password.");
            }
        }

        try
        {
            var authenticated = await _authClient.AuthenticateAsync(
                new AslBelgiAuthRequest { Login = _settings.Login, Password = _settings.Password }, ct);
            return Store(authenticated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Asl Belgisi authentication failed.");
            return Result.Failure<string>(AslBelgiErrors.TokenAcquisitionFailed(ex.Message));
        }
    }

    private Result<string> Store(AslBelgiAuthResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.AccessToken))
            return Result.Failure<string>(AslBelgiErrors.TokenAcquisitionFailed("Asl Belgisi auth response did not contain an access token."));

        // AccessTokenExpiresIn is milliseconds; refresh a safety margin before real expiry.
        var lifetimeSeconds = Math.Max(1, (response.AccessTokenExpiresIn / 1000) - _settings.AccessTokenSafetyMarginSeconds);
        _cache.Set(AccessTokenCacheKey, response.AccessToken, TimeSpan.FromSeconds(lifetimeSeconds));

        if (!string.IsNullOrWhiteSpace(response.RefreshToken))
            _cache.Set(RefreshTokenCacheKey, response.RefreshToken, TimeSpan.FromHours(24));

        return Result.Success(response.AccessToken);
    }

    private bool IsBusinessMode => string.Equals(_settings.AuthMode, BusinessMode, StringComparison.OrdinalIgnoreCase);

    private static bool IsConfigured(string? value)
        => !string.IsNullOrWhiteSpace(value) && !value.Contains(PlaceholderMarker, StringComparison.OrdinalIgnoreCase);
}
