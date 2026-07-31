using Application.Features.Hr.Files;
using Infrastructure.Integration.HrFiles;
using Infrastructure.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UnitTests;

public sealed class HrFileStorageTests
{
    [Fact]
    public async Task SaveAsync_StoresFileDirectlyInOrganizationDirectory()
    {
        var contentRoot = CreateTemporaryContentRoot();
        try
        {
            var storage = CreateStorage(contentRoot);
            await using var content = new MemoryStream([1, 2, 3]);

            var result = await storage.SaveAsync(
                organizationId: 8,
                absenceId: 42,
                new HrFileUpload(content, "document.pdf", "application/pdf", content.Length));

            var normalizedPath = result.RelativePath.Replace('\\', '/');
            Assert.StartsWith("appdata/absences/8/", normalizedPath);
            Assert.DoesNotContain("appdata/absences/8/42/", normalizedPath);
            Assert.True(File.Exists(result.AbsolutePath));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private static HrFileStorage CreateStorage(string contentRoot) =>
        new(
            new TestHostEnvironment(contentRoot),
            Options.Create(new HrFileStorageOptions
            {
                LocalRoot = "appdata/absences",
                MaxFileSizeMb = 20,
                AllowedExtensions = [".pdf"]
            }));

    private static string CreateTemporaryContentRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), "AccountingBack.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = nameof(UnitTests);
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);
    }
}
