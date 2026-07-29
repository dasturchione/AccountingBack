using Microsoft.Extensions.Options;

namespace Integration.CentralBank.Configs;

public sealed class CentralBankOptionsValidator : IValidateOptions<CentralBankOptions>
{
    public ValidateOptionsResult Validate(string? name, CentralBankOptions options)
    {
        var failures = new List<string>();

        // BaseUrl'siz provayder ishga tusha olmaydi: named client uni
        // Uri sifatida talqin qiladi.
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add($"{CentralBankOptions.SectionName}:BaseUrl is required.");
        }
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
        {
            failures.Add($"{CentralBankOptions.SectionName}:BaseUrl must be an absolute URL.");
        }

        if (options.TimeoutSeconds <= 0)
            failures.Add($"{CentralBankOptions.SectionName}:TimeoutSeconds must be greater than zero.");

        if (options.RetryCount < 1)
            failures.Add($"{CentralBankOptions.SectionName}:RetryCount must be at least 1.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
