using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Configs;

public sealed class GoogleDriveOptionsValidator : IValidateOptions<GoogleDriveSettings>
{
    private readonly IConfiguration _configuration;

    public GoogleDriveOptionsValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ValidateOptionsResult Validate(string? name, GoogleDriveSettings options)
    {
        if (!_configuration.GetValue<bool>("BackupJob:EnableEmailSend"))
            return ValidateOptionsResult.Success;

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.OAuthClientSecretsPath))
            failures.Add($"{GoogleDriveSettings.SectionName}:OAuthClientSecretsPath must be configured.");

        if (string.IsNullOrWhiteSpace(options.OAuthTokenStorePath))
            failures.Add($"{GoogleDriveSettings.SectionName}:OAuthTokenStorePath must be configured.");

        if (string.IsNullOrWhiteSpace(options.OAuthUser))
            failures.Add($"{GoogleDriveSettings.SectionName}:OAuthUser must be configured.");

        if (string.IsNullOrWhiteSpace(options.BackupFolderId))
            failures.Add($"{GoogleDriveSettings.SectionName}:BackupFolderId must be configured.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(string.Join("; ", failures));
    }

}
