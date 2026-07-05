using System.Text.RegularExpressions;

namespace UnitTests;

public sealed class PermissionCodeConstSeedSyncTests
{
    [Fact]
    public void PermissionCodeConst_AllCodes_ShouldExistInSysModuleSeedScripts()
    {
        var repoRoot = FindRepositoryRoot();
        var constFile = Path.Combine(repoRoot, "src", "SharedKernel", "Constants", "PermissionCodeConst.cs");
        var scriptsDirectory = Path.Combine(repoRoot, "src", "Infrastructure", "Persistence", "Scripts", "00_sys");

        var constantCodes = Regex.Matches(
                File.ReadAllText(constFile),
                "public const string \\w+ = \"([^\"]+)\";")
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        var seededCodes = Directory
            .EnumerateFiles(scriptsDirectory, "*.sql", SearchOption.TopDirectoryOnly)
            .SelectMany(path => GetSeedCodes(File.ReadAllText(path)))
            .ToHashSet(StringComparer.Ordinal);

        var missingCodes = constantCodes
            .Where(code => !seededCodes.Contains(code))
            .OrderBy(code => code)
            .ToArray();

        Assert.True(
            missingCodes.Length == 0,
            $"Missing sys_module seed entries for: {string.Join(", ", missingCodes)}");
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "src")) &&
                Directory.Exists(Path.Combine(current.FullName, "tests")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root could not be located from the test output directory.");
    }

    private static IEnumerable<string> GetSeedCodes(string sql)
    {
        foreach (Match match in Regex.Matches(
                     sql,
                     "'([A-Z][A-Z0-9_]+)'",
                     RegexOptions.Multiline))
        {
            yield return match.Groups[1].Value;
        }
    }
}
