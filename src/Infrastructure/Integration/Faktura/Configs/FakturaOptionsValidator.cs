using Microsoft.Extensions.Options;

namespace Integration.Faktura.Configs;

public sealed class FakturaOptionsValidator : IValidateOptions<FakturaOptions>
{
    public ValidateOptionsResult Validate(string? name, FakturaOptions options)
    {
        var failures = new List<string>();

        ValidateHttpsUrl(options.BaseUrl, $"{FakturaOptions.SectionName}:BaseUrl", failures);
        ValidateHttpsUrl(options.AuthUrl, $"{FakturaOptions.SectionName}:AuthUrl", failures);

        ValidateRequired(options.GrantType, $"{FakturaOptions.SectionName}:GrantType", failures);
        ValidateRequired(options.Username, $"{FakturaOptions.SectionName}:Username", failures);
        ValidateRequired(options.Password, $"{FakturaOptions.SectionName}:Password", failures);
        ValidateRequired(options.ClientId, $"{FakturaOptions.SectionName}:ClientId", failures);
        ValidateRequired(options.ClientSecret, $"{FakturaOptions.SectionName}:ClientSecret", failures);

        if (options.TimeoutSeconds <= 0)
            failures.Add($"{FakturaOptions.SectionName}:TimeoutSeconds must be greater than zero.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateRequired(string? value, string key, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
            failures.Add($"{key} is required.");
    }

    private static void ValidateHttpsUrl(string? value, string key, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"{key} is required.");
            return;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{key} must be an absolute HTTPS URL.");
        }
    }
}
