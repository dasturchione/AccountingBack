using Microsoft.Extensions.Options;

namespace Integration.Edocs.Configs;

public sealed class EdocsOptionsValidator : IValidateOptions<EdocsOptions>
{
    public ValidateOptionsResult Validate(string? name, EdocsOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add("Edocs:BaseUrl is required.");
        }
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
                 || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Edocs:BaseUrl must be an absolute HTTPS URL.");
        }

        if (options.TimeoutSeconds <= 0)
            failures.Add("Edocs:TimeoutSeconds must be greater than zero.");

        if (string.IsNullOrWhiteSpace(options.Product))
            failures.Add("Edocs:Product is required.");

        if (string.IsNullOrWhiteSpace(options.PartnerId))
            failures.Add("Edocs:PartnerId is required.");

        if (options.ChallengeTtlSeconds <= 0)
            failures.Add("Edocs:ChallengeTtlSeconds must be greater than zero.");
        else if (options.ChallengeTtlSeconds > 120)
            failures.Add("Edocs:ChallengeTtlSeconds must not exceed the provider contract TTL of 120 seconds.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
