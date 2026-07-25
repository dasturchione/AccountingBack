using Application.Common.Settings;
using Integration.GoogleDrive.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Diagnostics;
using System.IO.Compression;

namespace Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public class BackupJob : IJob
{
    private readonly BackupJobSettings    _settings;
    private readonly IGoogleDriveUploader _driveUploader;
    private readonly ILogger<BackupJob>   _logger;

    public BackupJob(
        IOptions<BackupJobSettings> options,
        IGoogleDriveUploader        driveUploader,
        ILogger<BackupJob>          logger)
    {
        _settings      = options.Value;
        _driveUploader = driveUploader;
        _logger        = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var db = _settings.Database;

        string backupDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "appdata", "tempfiles"));
        Directory.CreateDirectory(backupDir);

        string timestamp  = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupFilePrefix = GetSafeFileNameSegment(db.Name);
        string backupPath = Path.Combine(backupDir, $"{backupFilePrefix}-{timestamp}.backup");
        string zipPath    = Path.Combine(backupDir, $"{backupFilePrefix}-{timestamp}.zip");

        string pgDumpPath = !string.IsNullOrWhiteSpace(_settings.PgDumpPath)
            ? _settings.PgDumpPath
            : FindPgDump();

        try
        {
            // 1. pg_dump
            _logger.LogInformation("Backup started.");

            var processInfo = new ProcessStartInfo
            {
                FileName               = pgDumpPath,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                Environment            = { ["PGPASSWORD"] = db.Password }
            };
            processInfo.ArgumentList.Add("-h");
            processInfo.ArgumentList.Add(db.Host);
            processInfo.ArgumentList.Add("-U");
            processInfo.ArgumentList.Add(db.User);
            processInfo.ArgumentList.Add("-F");
            processInfo.ArgumentList.Add("c");
            processInfo.ArgumentList.Add(db.Name);

            int exitCode;
            using (var process = Process.Start(processInfo)!)
            {
                // stderr ni o'qish (xato xabarlari)
                var stderrTask = process.StandardError.ReadToEndAsync();

                await using (var fs = new FileStream(backupPath, FileMode.Create))
                    await process.StandardOutput.BaseStream.CopyToAsync(fs);

                await process.WaitForExitAsync();
                exitCode = process.ExitCode;

                await stderrTask;
            }

            // pg_dump muvaffaqiyatsiz bo'lsa to'xtatamiz
            if (exitCode != 0)
            {
                _logger.LogError("pg_dump {ExitCode} kodi bilan tugadi. Backup bekor qilindi.", exitCode);
                CleanupFile(backupPath);
                return;
            }

            _logger.LogInformation("PostgreSQL backup file was created successfully.");

            // 2. Zip
            using (var zipStream = new FileStream(zipPath, FileMode.Create))
            using (var archive   = new ZipArchive(zipStream, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath));

            _logger.LogInformation("Backup archive was created successfully.");

            // 3. Google Drive ga yuklash (ixtiyoriy)
            if (_settings.EnableEmailSend)
            {
                _logger.LogInformation("Google Drive upload enabled.");
                _logger.LogInformation("Google Drive upload started.");
                await _driveUploader.UploadAsync(zipPath, "application/octet-stream");
                _logger.LogInformation("Backup was uploaded to Google Drive successfully.");
            }
            else
            {
                _logger.LogInformation("Google Drive upload is disabled by BackupJob:EnableEmailSend; local backup files were retained.");
            }

            // 4. Temp fayllarni tozalash (faqat Drive ga yuklangandan keyin)
            if (_settings.EnableEmailSend)
            {
                foreach (var file in new[] { backupPath, zipPath })
                {
                    try { File.Delete(file); }
                    catch (Exception) { _logger.LogWarning("A successfully uploaded backup file could not be removed from the local temporary directory."); }
                }
                _logger.LogInformation("Local backup files were cleaned up after a successful upload.");
            }
        }
        catch (GoogleDriveConfigurationException exception)
        {
            _logger.LogError("Backup stopped because Google Drive configuration is invalid: {Reason}", exception.Message);
        }
        catch (GoogleDrivePermissionException exception)
        {
            _logger.LogError("Backup was retained locally because Google Drive permission or configuration was rejected: {Reason}", exception.Message);
        }
        catch (GoogleDriveUploadException exception)
        {
            _logger.LogError("Backup was retained locally because Google Drive upload failed: {Reason}", exception.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Backup failed. Failure type: {FailureType}.", ex.GetType().Name);
        }
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //
    private void CleanupFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* ignore */ }
    }

    private static string GetSafeFileNameSegment(string? value)
    {
        var source = string.IsNullOrWhiteSpace(value) ? "database" : value;
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeValue = new string(source
            .Select(character => invalidCharacters.Contains(character) || character is '/' or '\\' ? '_' : character)
            .ToArray())
            .Trim('.', ' ');

        return string.IsNullOrWhiteSpace(safeValue) ? "database" : safeValue;
    }

    private static string FindPgDump()
    {
        // Windows standart joylari
        string[] windowsPaths =
        [
            @"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\17\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
        ];
        foreach (var path in windowsPaths)
            if (File.Exists(path)) return path;

        // Linux/Mac
        string[] unixPaths = ["/usr/bin/pg_dump", "/usr/local/bin/pg_dump"];
        foreach (var path in unixPaths)
            if (File.Exists(path)) return path;

        throw new FileNotFoundException(
            "pg_dump was not found. Configure BackupJob:PgDumpPath with an installed pg_dump executable path.");
    }
}
