using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Configs;

public sealed class GoogleDriveOptionsValidator : IValidateOptions<GoogleDriveOptions>
{
    public ValidateOptionsResult Validate(string? name, GoogleDriveOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.CredentialsPath))
            failures.Add($"{GoogleDriveOptions.SectionName}:CredentialsPath must be configured.");

        if (string.IsNullOrWhiteSpace(options.BackupFolderId))
            failures.Add($"{GoogleDriveOptions.SectionName}:BackupFolderId must be configured.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

}
