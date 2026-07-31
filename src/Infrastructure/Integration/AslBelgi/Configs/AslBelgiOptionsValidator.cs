using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Integration.AslBelgi.Configs;

public sealed partial class AslBelgiOptionsValidator : IValidateOptions<AslBelgiOptions>
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

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            failures.Add("AslBelgi:ApiKey is required.");

        if (string.IsNullOrWhiteSpace(options.Tin))
        {
            failures.Add("AslBelgi:Tin is required.");
        }
        else if (!TinPattern().IsMatch(options.Tin))
        {
            failures.Add("AslBelgi:Tin must contain either 9-digit TIN or 14-digit PINFL.");
        }

        if (options.TimeoutSeconds <= 0)
            failures.Add("AslBelgi:TimeoutSeconds must be greater than zero.");

        if (string.IsNullOrWhiteSpace(options.ProductGroup))
            failures.Add("AslBelgi:ProductGroup is required.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    [GeneratedRegex("^(?:\\d{9}|\\d{14})$")]
    private static partial Regex TinPattern();
}
