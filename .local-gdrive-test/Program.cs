using System.IO.Compression;
using System.Text.Json;
using Integration.GoogleDrive.Configs;
using Integration.GoogleDrive.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

var projectRoot = args.Length > 0 ? args[0] : throw new ArgumentException("Project root is required.");
var sqlPath = args.Length > 1 ? args[1] : throw new ArgumentException("SQL path is required.");
var zipPath = args.Length > 2 ? args[2] : throw new ArgumentException("ZIP path is required.");
var configPath = Path.Combine(projectRoot, "src", "Presentation", "WebApi", "appsettings.Production.json");
var uploadSucceeded = false;

try
{
    using var configDocument = JsonDocument.Parse(await File.ReadAllTextAsync(configPath));
    var drive = configDocument.RootElement.GetProperty("GoogleDrive");
    var credentialsPath = drive.GetProperty("CredentialsPath").GetString()
        ?? throw new InvalidOperationException("CredentialsPath is empty.");
    var settings = new GoogleDriveSettings
    {
        CredentialsPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(configPath)!, credentialsPath)),
        BackupFolderId = drive.GetProperty("BackupFolderId").GetString() ?? throw new InvalidOperationException("BackupFolderId is empty.")
    };

    Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
    if (File.Exists(zipPath))
        throw new InvalidOperationException("ZIP path already exists; refusing to overwrite it.");

    using (var zipStream = new FileStream(zipPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
    {
        var entry = archive.CreateEntry(Path.GetFileName(sqlPath), CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await using var sqlStream = new FileStream(sqlPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await sqlStream.CopyToAsync(entryStream);
    }

    var uploader = new GoogleDriveUploader(
        Options.Create(settings),
        NullLogger<GoogleDriveUploader>.Instance);

    await uploader.UploadAsync(zipPath, "application/octet-stream");
    uploadSucceeded = true;
    Console.WriteLine("UPLOAD_OK");
}
catch (GoogleDriveUploadException exception)
{
    Console.WriteLine($"UPLOAD_FAILED:SafeReason={exception.SafeReason};HttpStatusCode={exception.HttpStatusCode?.ToString() ?? "none"}");
    return 1;
}
catch (Exception exception)
{
    Console.WriteLine($"UPLOAD_FAILED:SafeReason=Other;ExceptionType={exception.GetType().Name}");
    return 1;
}
finally
{
    if (uploadSucceeded && File.Exists(zipPath))
        File.Delete(zipPath);
}

return 0;
