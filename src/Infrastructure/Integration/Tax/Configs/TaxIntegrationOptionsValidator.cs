using Microsoft.Extensions.Options;

namespace Integration.Tax.Configs;

public sealed class TaxIntegrationOptionsValidator : IValidateOptions<TaxIntegrationOptions>
{
    public ValidateOptionsResult Validate(string? name, TaxIntegrationOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.DefaultProviderCode))
            failures.Add($"{TaxIntegrationOptions.SectionName}:DefaultProviderCode is required.");

        if (options.TimeoutSeconds <= 0)
            failures.Add($"{TaxIntegrationOptions.SectionName}:TimeoutSeconds must be greater than zero.");

        if (options.RetryCount < 1)
            failures.Add($"{TaxIntegrationOptions.SectionName}:RetryCount must be at least 1.");

        ValidateProvider(options.Mxik, nameof(options.Mxik), failures);
        ValidateProvider(options.SoliqApi, nameof(options.SoliqApi), failures);
        ValidateProvider(options.EFaktura, nameof(options.EFaktura), failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    // BaseUrl mavjudligi TALAB QILINMAYDI: sozlanmagan provayder ish vaqtida
    // "Tax provider ... is not configured" xatosini beradi va ilovaning ishga
    // tushishini to'smaydi. Bu yerda faqat berilgan qiymatning shakli tekshiriladi.
    private static void ValidateProvider(TaxIntegrationOptions.ProviderOptions provider, string providerKey, List<string> failures)
    {
        if (!string.IsNullOrWhiteSpace(provider.BaseUrl)
            && !Uri.TryCreate(provider.BaseUrl, UriKind.Absolute, out _))
        {
            failures.Add($"{TaxIntegrationOptions.SectionName}:{providerKey}:BaseUrl must be an absolute URL.");
        }

        if (provider.TimeoutSeconds is <= 0)
            failures.Add($"{TaxIntegrationOptions.SectionName}:{providerKey}:TimeoutSeconds must be greater than zero.");

        if (provider.RetryCount is < 1)
            failures.Add($"{TaxIntegrationOptions.SectionName}:{providerKey}:RetryCount must be at least 1.");
    }
}
