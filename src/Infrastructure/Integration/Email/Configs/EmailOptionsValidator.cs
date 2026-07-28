using Microsoft.Extensions.Options;

namespace Integration.Email.Configs;

public sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var failures = new List<string>();

        // Host, FromEmail, Username va Password — EmailSender ularni shartsiz
        // ishlatadi (ConnectAsync va AuthenticateAsync), shuning uchun majburiy.
        if (string.IsNullOrWhiteSpace(options.Host))
            failures.Add($"{EmailOptions.SectionName}:Host is required.");

        if (string.IsNullOrWhiteSpace(options.FromEmail))
            failures.Add($"{EmailOptions.SectionName}:FromEmail is required.");

        if (string.IsNullOrWhiteSpace(options.Username))
            failures.Add($"{EmailOptions.SectionName}:Username is required.");

        if (string.IsNullOrWhiteSpace(options.Password))
            failures.Add($"{EmailOptions.SectionName}:Password is required.");

        if (options.Port is <= 0 or > 65535)
            failures.Add($"{EmailOptions.SectionName}:Port must be between 1 and 65535.");

        if (options.TimeoutSeconds <= 0)
            failures.Add($"{EmailOptions.SectionName}:TimeoutSeconds must be greater than zero.");

        if (options.MaxRetries < 1)
            failures.Add($"{EmailOptions.SectionName}:MaxRetries must be at least 1.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
