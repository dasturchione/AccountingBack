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

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
