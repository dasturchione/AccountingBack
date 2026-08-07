using Microsoft.Extensions.Options;

namespace Integration.Edocs.Configs;

public sealed class EdocsTokenStorageOptions
{
    public const string SectionName = "EdocsTokenStorage";

    public string RootPath { get; set; } = "appdata/edocs-token-cache";
    public int MaxTokenFileBytes { get; set; } = 16 * 1024;
}

public sealed class EdocsTokenStorageOptionsValidator : IValidateOptions<EdocsTokenStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, EdocsTokenStorageOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.RootPath))
            failures.Add("EdocsTokenStorage:RootPath is required.");

        if (options.MaxTokenFileBytes < 1024)
            failures.Add("EdocsTokenStorage:MaxTokenFileBytes must be at least 1024.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
