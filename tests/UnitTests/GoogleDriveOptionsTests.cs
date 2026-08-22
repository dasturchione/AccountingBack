using Integration.GoogleDrive.Configs;
using Microsoft.Extensions.Options;

namespace UnitTests;

public sealed class GoogleDriveOptionsTests
{
    [Fact]
    public void Validator_RequiresOAuthSettingsAndBackupFolder()
    {
        var result = new GoogleDriveOptionsValidator().Validate(
            Options.DefaultName,
            new GoogleDriveSettings());

        Assert.True(result.Failed);
        Assert.Contains("OAuthClientSecretsPath", result.FailureMessage);
        Assert.DoesNotContain("CredentialsPath", result.FailureMessage);

        Assert.True(new GoogleDriveOptionsValidator().Validate(
            Options.DefaultName,
            new GoogleDriveSettings
            {
                OAuthClientSecretsPath = "client.json"
            }).Failed);

        Assert.True(new GoogleDriveOptionsValidator().Validate(
            Options.DefaultName,
            new GoogleDriveSettings
            {
                OAuthClientSecretsPath = "client.json",
                OAuthTokenStorePath = "tokens"
            }).Failed);
    }

    [Fact]
    public void Validator_AcceptsConfiguredOAuthSettings()
    {
        var result = new GoogleDriveOptionsValidator().Validate(
            Options.DefaultName,
            new GoogleDriveSettings
            {
                OAuthClientSecretsPath = "appdata/drive/oauth-client.json",
                OAuthTokenStorePath = "appdata/drive/oauth-tokens",
                OAuthUser = "user@example.com",
                BackupFolderId = "folder-id"
            });

        Assert.True(result.Succeeded);
    }
}
