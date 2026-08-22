using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Integration.GoogleDrive.Configs;
using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Services;

public sealed class GoogleDriveServiceFactory : IGoogleDriveServiceFactory
{
    private readonly GoogleDriveSettings _settings;

    public GoogleDriveServiceFactory(IOptions<GoogleDriveSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<DriveService> CreateAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OAuthClientSecretsPath))
            throw new InvalidOperationException("Google Drive OAuth client secrets path is not configured.");
        if (string.IsNullOrWhiteSpace(_settings.OAuthUser))
            throw new InvalidOperationException("Google Drive OAuth user is not configured.");
        if (string.IsNullOrWhiteSpace(_settings.OAuthTokenStorePath))
            throw new InvalidOperationException("Google Drive OAuth token store path is not configured.");

        var clientSecretsPath = ResolvePath(_settings.OAuthClientSecretsPath);
        var tokenStorePath = ResolvePath(_settings.OAuthTokenStorePath);
        if (!File.Exists(clientSecretsPath))
            throw new FileNotFoundException("Google Drive OAuth client secrets file was not found.", clientSecretsPath);

        Directory.CreateDirectory(tokenStorePath);
        var clientSecrets = GoogleClientSecrets.FromFile(clientSecretsPath).Secrets;
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = clientSecrets,
            Scopes = [DriveService.Scope.Drive],
            DataStore = new FileDataStore(tokenStorePath, true)
        });

        var token = await flow.DataStore.GetAsync<TokenResponse>(_settings.OAuthUser);
        if (token is null && !string.IsNullOrWhiteSpace(_settings.OAuthRefreshToken))
        {
            token = new TokenResponse { RefreshToken = _settings.OAuthRefreshToken.Trim() };
            await flow.DataStore.StoreAsync(_settings.OAuthUser, token);
        }

        if (token is null || string.IsNullOrWhiteSpace(token.RefreshToken))
            throw new InvalidOperationException("Google Drive OAuth refresh token is not configured.");

        var credential = new UserCredential(flow, _settings.OAuthUser, token);
        if (credential.Token.IsStale && !await credential.RefreshTokenAsync(cancellationToken))
            throw new InvalidOperationException("Google Drive OAuth access token could not be refreshed.");

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "AccountingBack Google Drive"
        });
    }

    private static string ResolvePath(string path) => Path.IsPathRooted(path)
        ? path
        : Path.Combine(AppContext.BaseDirectory, path);
}
