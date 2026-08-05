using Microsoft.Extensions.Options;

namespace Integration.Didox.Configs;

public sealed class DidoxOptionsValidator : IValidateOptions<DidoxOptions>
{
    public ValidateOptionsResult Validate(string? name, DidoxOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add("Didox:BaseUrl is required.");
        }
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
                 || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Didox:BaseUrl must be an absolute HTTPS URL.");
        }
        else if (options.UsePartnerlessLegacyApi
                 && !string.Equals(baseUri.Host, "api.didox.uz", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Didox:UsePartnerlessLegacyApi requires Didox:BaseUrl to use api.didox.uz.");
        }

        if (options.TimeoutSeconds <= 0)
            failures.Add("Didox:TimeoutSeconds must be greater than zero.");

        // PartnerToken ixtiyoriy: legacy/production rejimida u yuborilmaydi.
        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
