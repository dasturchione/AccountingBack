using Microsoft.Extensions.Options;

namespace Integration.Faktura.Configs;

public sealed class FakturaAuthSessionStorageOptionsValidator
    : IValidateOptions<FakturaAuthSessionStorageOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        FakturaAuthSessionStorageOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.RootPath))
            failures.Add("RootPath is required.");

        if (options.SessionLifetimeMinutes <= 0)
            failures.Add("SessionLifetimeMinutes must be greater than zero.");

        if (options.MaxSessionFileBytes < 4 * 1024
            || options.MaxSessionFileBytes > 1024 * 1024)
        {
            failures.Add("MaxSessionFileBytes must be between 4096 and 1048576.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
