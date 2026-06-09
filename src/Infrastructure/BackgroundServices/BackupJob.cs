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

        string backupDir = Path.Combine(Directory.GetCurrentDirectory(), "appdata", "tempfiles");
        Directory.CreateDirectory(backupDir);

        string timestamp  = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupPath = Path.Combine(backupDir, $"{db.Name}-{timestamp}.backup");
        string zipPath    = Path.Combine(backupDir, $"{db.Name}-{timestamp}.zip");

        string pgDumpPath = !string.IsNullOrWhiteSpace(_settings.PgDumpPath)
            ? _settings.PgDumpPath
            : FindPgDump();

        try
        {
            // 1. pg_dump
            _logger.LogInformation("Backup boshlandi: {DB} @ {Host}", db.Name, db.Host);

            var processInfo = new ProcessStartInfo
            {
                FileName               = pgDumpPath,
                Arguments              = $"-h {db.Host} -U {db.User} -F c {db.Name}",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                Environment            = { ["PGPASSWORD"] = db.Password }
            };

            int exitCode;
            using (var process = Process.Start(processInfo)!)
            {
                // stderr ni o'qish (xato xabarlari)
                var stderrTask = process.StandardError.ReadToEndAsync();

                await using (var fs = new FileStream(backupPath, FileMode.Create))
                    await process.StandardOutput.BaseStream.CopyToAsync(fs);

                await process.WaitForExitAsync();
                exitCode = process.ExitCode;

                var stderr = await stderrTask;
                if (!string.IsNullOrWhiteSpace(stderr))
                    _logger.LogWarning("pg_dump stderr: {Stderr}", stderr);
            }

            // pg_dump muvaffaqiyatsiz bo'lsa to'xtatamiz
            if (exitCode != 0)
            {
                _logger.LogError("pg_dump {ExitCode} kodi bilan tugadi. Backup bekor qilindi.", exitCode);
                CleanupFile(backupPath);
                return;
            }

            _logger.LogInformation("Backup yaratildi: {Path}", backupPath);

            // 2. Zip
            using (var zipStream = new FileStream(zipPath, FileMode.Create))
            using (var archive   = new ZipArchive(zipStream, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath));

            _logger.LogInformation("Arxiv yaratildi: {ZipPath}", zipPath);

            // 3. Google Drive ga yuklash (ixtiyoriy)
            if (_settings.EnableEmailSend)
            {
                var url = await _driveUploader.UploadAsync(zipPath, "application/zip");
                _logger.LogInformation("Backup Google Drive ga yuklandi: {Url}", url);
            }
            else
            {
                _logger.LogInformation("Google Drive yuklash o'chirilgan (EnableEmailSend=false). " +
                                       "Fayl saqlanib qoldi: {ZipPath}", zipPath);
            }

            // 4. Temp fayllarni tozalash (faqat Drive ga yuklangandan keyin)
            if (_settings.EnableEmailSend)
            {
                foreach (var file in Directory.GetFiles(backupDir))
                {
                    try   { File.Delete(file); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Faylni o'chirib bo'lmadi: {File}", file); }
                }
                _logger.LogInformation("Tempfiles papkasi tozalandi");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup xatosi: {Message}", ex.Message);
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

    private static string FindPgDump()
    {
        // 1. PATH
        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            var candidate = Path.Combine(dir.Trim(), "pg_dump");
            if (File.Exists(candidate))          return candidate;
            if (File.Exists(candidate + ".exe")) return candidate + ".exe";
        }

        // 2. Windows standart joylari
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

        // 3. Linux/Mac
        string[] unixPaths = ["/usr/bin/pg_dump", "/usr/local/bin/pg_dump"];
        foreach (var path in unixPaths)
            if (File.Exists(path)) return path;

        throw new FileNotFoundException(
            "pg_dump topilmadi. appsettings.json dagi BackupJob.PgDumpPath ni belgilang " +
            "yoki PostgreSQL bin papkasini PATH ga qo'shing.");
    }
}
