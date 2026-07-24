using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Configs;

public sealed class GoogleDriveSettingsValidator : IValidateOptions<GoogleDriveSettings>
{
    public ValidateOptionsResult Validate(string? name, GoogleDriveSettings settings)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.CredentialsPath))
            failures.Add("GoogleDrive:CredentialsPath must be configured.");

        if (string.IsNullOrWhiteSpace(settings.BackupFolderId))
            failures.Add("GoogleDrive:BackupFolderId must be configured.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

}
