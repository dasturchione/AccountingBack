using Application.Features.Hr.Files;
using Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Integration.HrFiles;

public sealed class HrFileStorage : IHrFileStorage
{
    private readonly string _contentRoot;
    private readonly string _storageRoot;
    private readonly HrFileStorageOptions _options;
    private readonly HashSet<string> _allowedExtensions;

    public HrFileStorage(
        IHostEnvironment environment,
        IOptions<HrFileStorageOptions> options)
    {
        _contentRoot = Path.GetFullPath(environment.ContentRootPath);
        _options = options.Value;
        _storageRoot = Path.GetFullPath(Path.IsPathRooted(_options.LocalRoot)
            ? _options.LocalRoot
            : Path.Combine(_contentRoot, _options.LocalRoot));
        _allowedExtensions = _options.AllowedExtensions
            .Select(NormalizeExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<HrStoredFile> SaveAsync(
        int organizationId,
        long absenceId,
        HrFileUpload file,
        CancellationToken ct = default)
    {
        Validate(file);

        var extension = NormalizeExtension(Path.GetExtension(Path.GetFileName(file.FileName)));
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var directory = ResolveDirectory(organizationId, absenceId);
        EnsureDirectory(directory);

        var absolutePath = Path.Combine(directory, storedName);
        var relativePath = Path.GetRelativePath(_contentRoot, absolutePath).Replace('\\', '/');
        await SaveNewFileAsync(absolutePath, file.Content, ct);
        return new HrStoredFile(relativePath, absolutePath, storedName);
    }

    public async Task RestoreAsync(string relativePath, Stream content, CancellationToken ct = default)
    {
        var absolutePath = ResolveExistingRelativePath(relativePath);
        EnsureDirectory(Path.GetDirectoryName(absolutePath)!);

        var temporaryPath = absolutePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var target = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             81920,
                             FileOptions.Asynchronous))
            {
                await content.CopyToAsync(target, ct);
                await target.FlushAsync(ct);
            }

            File.Move(temporaryPath, absolutePath, overwrite: true);
            SetFilePermissions(absolutePath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    public Task<Stream?> OpenAsync(string relativePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var absolutePath = ResolveExistingRelativePath(relativePath);
        Stream? result = File.Exists(absolutePath)
            ? new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous)
            : null;
        return Task.FromResult(result);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var absolutePath = ResolveExistingRelativePath(relativePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
        return Task.CompletedTask;
    }

    public bool Exists(string relativePath) =>
        File.Exists(ResolveExistingRelativePath(relativePath));

    private void Validate(HrFileUpload file)
    {
        if (file.Length <= 0)
            throw new InvalidOperationException("An empty file cannot be uploaded.");

        var maxBytes = Math.Max(_options.MaxFileSizeMb, 1) * 1024L * 1024L;
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File size exceeds the configured {_options.MaxFileSizeMb} MB limit.");

        var extension = NormalizeExtension(Path.GetExtension(Path.GetFileName(file.FileName)));
        if (string.IsNullOrWhiteSpace(extension) || !_allowedExtensions.Contains(extension))
            throw new InvalidOperationException("This file extension is not allowed for HR attachments.");
    }

    private string ResolveDirectory(int organizationId, long absenceId)
    {
        var path = Path.GetFullPath(Path.Combine(
            _storageRoot,
            organizationId.ToString(),
            absenceId.ToString()));
        EnsureInsideStorageRoot(path);
        return path;
    }

    private string ResolveExistingRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new InvalidOperationException("Invalid relative HR attachment path.");

        var path = Path.GetFullPath(Path.Combine(_contentRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureInsideStorageRoot(path);
        return path;
    }

    private void EnsureInsideStorageRoot(string path)
    {
        var rootWithSeparator = _storageRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(path, _storageRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("HR attachment path escapes the configured AppData directory.");
    }

    private static async Task SaveNewFileAsync(
        string absolutePath,
        Stream content,
        CancellationToken ct)
    {
        try
        {
            await using var target = new FileStream(
                absolutePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous);
            await content.CopyToAsync(target, ct);
            await target.FlushAsync(ct);
            SetFilePermissions(absolutePath);
        }
        catch
        {
            if (File.Exists(absolutePath))
                File.Delete(absolutePath);
            throw;
        }
    }

    private static void EnsureDirectory(string path)
    {
        if (OperatingSystem.IsLinux())
        {
            const UnixFileMode mode =
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute;
            Directory.CreateDirectory(path, mode);
            File.SetUnixFileMode(path, mode);
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void SetFilePermissions(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        const UnixFileMode mode =
            UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.GroupRead | UnixFileMode.GroupWrite;
        File.SetUnixFileMode(path, mode);
    }

    private static string NormalizeExtension(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : (value.StartsWith('.') ? value : "." + value).ToLowerInvariant();
}
