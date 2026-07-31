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

        if (options.TimeoutSeconds <= 0)
            failures.Add("Didox:TimeoutSeconds must be greater than zero.");

        // PartnerToken ATAYLAB shu yerda tekshirilmaydi — INT_DIDOX.md §1.3: partner
        // tokeni faqat Didox akkaunt menejeri orqali qo'lda beriladi, ilova PartnerToken
        // hali yozilmagan holatda ham ishga tushishi shart. Yo'qligi runtime'da,
        // DidoxAuthorizationHandler va Didox AuthClient darajasida aniq xato bilan chiqadi.

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
