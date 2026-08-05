using Microsoft.Extensions.Options;

namespace Integration.Didox.Configs;

public sealed class DidoxTokenStorageOptions
{
    public const string SectionName = "DidoxTokenStorage";

    public string RootPath { get; set; } = "appdata/didox-token-cache";
    public int MaxTokenFileBytes { get; set; } = 16 * 1024;
}

public sealed class DidoxTokenStorageOptionsValidator : IValidateOptions<DidoxTokenStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, DidoxTokenStorageOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.RootPath))
            failures.Add("DidoxTokenStorage:RootPath is required.");

        if (options.MaxTokenFileBytes < 1024)
            failures.Add("DidoxTokenStorage:MaxTokenFileBytes must be at least 1024.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
