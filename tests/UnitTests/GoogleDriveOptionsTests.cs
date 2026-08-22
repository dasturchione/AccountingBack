using Integration.GoogleDrive.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace UnitTests;

public sealed class GoogleDriveOptionsTests
{
    [Fact]
    public void Validator_RequiresOAuthSettingsAndBackupFolder()
    {
        var validator = CreateValidator(enabled: true);
        var result = validator.Validate(
            Options.DefaultName,
            new GoogleDriveSettings());

        Assert.True(result.Failed);
        Assert.Contains("OAuthClientSecretsPath", result.FailureMessage);
        Assert.DoesNotContain("CredentialsPath", result.FailureMessage);

        Assert.True(validator.Validate(
            Options.DefaultName,
            new GoogleDriveSettings
            {
                OAuthClientSecretsPath = "client.json"
            }).Failed);

        Assert.True(validator.Validate(
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
        var result = CreateValidator(enabled: true).Validate(
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

    [Fact]
    public void Validator_AllowsMissingOAuthSettingsWhenBackupUploadIsDisabled()
    {
        var result = CreateValidator(enabled: false).Validate(
            Options.DefaultName,
            new GoogleDriveSettings());

        Assert.True(result.Succeeded);
    }

    private static GoogleDriveOptionsValidator CreateValidator(bool enabled)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackupJob:EnableEmailSend"] = enabled.ToString()
            })
            .Build();

        return new GoogleDriveOptionsValidator(configuration);
    }
}
