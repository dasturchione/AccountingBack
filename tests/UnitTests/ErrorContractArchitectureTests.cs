using SharedKernel.Results;
using System.Text.RegularExpressions;

namespace UnitTests;

public sealed class ErrorContractArchitectureTests
{
    private static readonly Regex DirectErrorFactoryPattern = new(
        @"\bError\.(?:NotFound|Conflict|Unauthorized|Forbidden|Validation|Business|Problem|Timeout)\s*\(|\bnew\s+Error\s*\(",
        RegexOptions.Compiled);

    [Fact]
    public void ValidationFactory_CreatesValidationError()
    {
        var error = Error.Validation("Sample.Invalid", "Invalid sample.");

        Assert.Equal("Sample.Invalid", error.Code);
        Assert.Equal("Invalid sample.", error.Description);
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    [Fact]
    public void ApplicationErrorCatalogs_AreStoredInErrorsDirectories()
    {
        var applicationPath = Path.Combine(FindRepositoryRoot(), "src", "Application");
        var misplacedFiles = Directory
            .EnumerateFiles(applicationPath, "*Errors.cs", SearchOption.AllDirectories)
            .Where(path => !HasDirectorySegment(path, "Errors"))
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .OrderBy(path => path)
            .ToArray();

        Assert.True(
            misplacedFiles.Length == 0,
            $"Application error catalogs must be stored in an Errors directory:{Environment.NewLine}{string.Join(Environment.NewLine, misplacedFiles)}");
    }

    [Fact]
    public void DirectErrorFactories_ExistOnlyInErrorsDirectories()
    {
        var root = FindRepositoryRoot();
        var sourcePath = Path.Combine(root, "src");
        var violations = Directory
            .EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Where(path => !HasDirectorySegment(path, "Errors"))
            .Where(path => !path.EndsWith(Path.Combine("SharedKernel", "Results", "Error.cs"), StringComparison.OrdinalIgnoreCase))
            .Where(path => DirectErrorFactoryPattern.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(root, path))
            .OrderBy(path => path)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Direct Error factories must be declared in an Errors directory:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ApplicationErrorCatalogs_DeclareAllSupportedLanguages()
    {
        var root = FindRepositoryRoot();
        var applicationPath = Path.Combine(root, "src", "Application");
        var violations = Directory
            .EnumerateFiles(applicationPath, "*Errors.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Where(path =>
            {
                var source = File.ReadAllText(path);
                return !source.Contains("LanguageIdConst.UZ", StringComparison.Ordinal)
                    || !source.Contains("LanguageIdConst.UZ_CYRL", StringComparison.Ordinal)
                    || !source.Contains("LanguageIdConst.RU", StringComparison.Ordinal);
            })
            .Select(path => Path.GetRelativePath(root, path))
            .OrderBy(path => path)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Application error catalogs must support UZ, UZ_CYRL, RU and EN fallback:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ApplicationErrorMethods_AcceptLanguageId()
    {
        var violations = typeof(Application.Features.Platform.PlatformErrors).Assembly
            .GetTypes()
            .Where(type => type.IsClass && type.IsAbstract && type.IsSealed && type.Name.EndsWith("Errors", StringComparison.Ordinal))
            .SelectMany(type => type
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(method => method.ReturnType == typeof(Error))
                .Where(method => method.GetParameters().All(parameter => parameter.Name != "languageId"))
                .Select(method => $"{type.FullName}.{method.Name}"))
            .OrderBy(name => name)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Every public Application error factory must accept languageId:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Accounting.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Accounting repository root was not found.");
    }

    private static bool HasDirectorySegment(string path, string segment) =>
        new FileInfo(path).Directory?
            .EnumerateParentsAndSelf()
            .Any(directory => string.Equals(directory.Name, segment, StringComparison.OrdinalIgnoreCase)) == true;

    private static bool IsGeneratedPath(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}

internal static class DirectoryInfoExtensions
{
    public static IEnumerable<DirectoryInfo> EnumerateParentsAndSelf(this DirectoryInfo directory)
    {
        for (var current = directory; current is not null; current = current.Parent)
            yield return current;
    }
}
