using Microsoft.Extensions.Options;

namespace Integration.AslBelgi.Configs;

public sealed class AslBelgiOptionsValidator : IValidateOptions<AslBelgiOptions>
{
    public ValidateOptionsResult Validate(string? name, AslBelgiOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add("AslBelgi:BaseUrl is required.");
        }
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
                 || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("AslBelgi:BaseUrl must be an absolute HTTPS URL.");
        }

        if (options.TimeoutSeconds <= 0)
            failures.Add("AslBelgi:TimeoutSeconds must be greater than zero.");

        if (string.IsNullOrWhiteSpace(options.ProductGroup))
            failures.Add("AslBelgi:ProductGroup is required.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
