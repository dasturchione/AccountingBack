using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google;
using Integration.GoogleDrive.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Integration.GoogleDrive.Services;

public class GoogleDriveUploader : IGoogleDriveUploader
{
    private readonly GoogleDriveSettings _settings;
    private readonly ILogger<GoogleDriveUploader> _logger;

    public GoogleDriveUploader(
        IOptions<GoogleDriveSettings> options,
        ILogger<GoogleDriveUploader> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    // DriveService faqat upload qilayotganda yaratiladi
    private DriveService CreateDriveService()
    {
        if (string.IsNullOrWhiteSpace(_settings.CredentialsPath))
            throw new GoogleDriveConfigurationException("GoogleDrive:CredentialsPath is not configured.");

        if (string.IsNullOrWhiteSpace(_settings.BackupFolderId))
            throw new GoogleDriveConfigurationException("GoogleDrive:BackupFolderId is not configured.");

        GoogleCredential credential;
        var credentialPath = Path.IsPathRooted(_settings.CredentialsPath)
            ? _settings.CredentialsPath
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), _settings.CredentialsPath));

        try
        {
            using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.DriveFile);
        }
        catch (Exception)
        {
            throw new GoogleDriveConfigurationException(
                "GoogleDrive:CredentialsPath does not point to a valid service-account credential.");
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName       = "AccountingBackup"
        });
    }

    public async Task<string> UploadAsync(string filePath, string mimeType)
    {
        var fileName = Path.GetFileName(filePath);
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return await UploadAsync(stream, fileName, mimeType);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string mimeType)
    {
        var service  = CreateDriveService();
        var folderId = _settings.BackupFolderId;

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name    = fileName,
            Parents = [folderId]
        };

        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (fileStream.CanSeek)
                    fileStream.Position = 0;

                var request = service.Files.Create(fileMetadata, fileStream, mimeType);
                request.Fields = "id";

                var result = await request.UploadAsync();
                if (result.Status == Google.Apis.Upload.UploadStatus.Completed &&
                    !string.IsNullOrWhiteSpace(request.ResponseBody?.Id))
                {
                    return $"https://drive.google.com/file/d/{request.ResponseBody.Id}/view";
                }

                var exception = result.Exception;
                if (!IsTransient(exception) || attempt == maxAttempts)
                    throw CreateUploadException(exception);
            }
            catch (GoogleDrivePermissionException)
            {
                throw;
            }
            catch (GoogleDriveUploadException)
            {
                throw;
            }
            catch (Exception exception) when (IsTransient(exception) && attempt < maxAttempts)
            {
                // The safe retry log intentionally excludes exception details and credential data.
            }
            catch (Exception exception)
            {
                throw CreateUploadException(exception);
            }

            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
            _logger.LogWarning(
                "Google Drive upload transient failure. Retrying attempt {Attempt} of {MaxAttempts} after {DelaySeconds} seconds.",
                attempt + 1,
                maxAttempts,
                delay.TotalSeconds);
            await Task.Delay(delay);
        }

        throw new GoogleDriveUploadException(
            "Google Drive upload did not complete after retry attempts.",
            null,
            GoogleDriveUploadSafeReason.TransientNetwork);
    }

    private static GoogleDriveUploadException CreateUploadException(Exception? exception)
    {
        var statusCode = GetStatusCode(exception);
        var safeReason = GetSafeReason(exception, statusCode);

        return safeReason is GoogleDriveUploadSafeReason.Unauthorized or GoogleDriveUploadSafeReason.Forbidden
            ? new GoogleDrivePermissionException(
                "Google Drive access was rejected.",
                statusCode!.Value,
                safeReason)
            : new GoogleDriveUploadException(
                GetSafeMessage(safeReason),
                statusCode,
                safeReason);
    }

    private static HttpStatusCode? GetStatusCode(Exception? exception) =>
        exception is GoogleApiException apiException
            ? apiException.HttpStatusCode
            : null;

    private static GoogleDriveUploadSafeReason GetSafeReason(
        Exception? exception,
        HttpStatusCode? statusCode)
    {
        if (statusCode == HttpStatusCode.Unauthorized)
            return GoogleDriveUploadSafeReason.Unauthorized;

        if (statusCode == HttpStatusCode.Forbidden)
            return GoogleDriveUploadSafeReason.Forbidden;

        if (statusCode == HttpStatusCode.NotFound)
            return GoogleDriveUploadSafeReason.NotFound;

        if (statusCode == (HttpStatusCode)429)
            return GoogleDriveUploadSafeReason.RateLimited;

        if (IsTransient(exception))
            return GoogleDriveUploadSafeReason.TransientNetwork;

        return GoogleDriveUploadSafeReason.Other;
    }

    private static string GetSafeMessage(GoogleDriveUploadSafeReason safeReason) => safeReason switch
    {
        GoogleDriveUploadSafeReason.NotFound => "Google Drive upload target was not found.",
        GoogleDriveUploadSafeReason.RateLimited => "Google Drive upload was rate limited.",
        GoogleDriveUploadSafeReason.TransientNetwork => "Google Drive upload encountered a transient network or server error.",
        _ => "Google Drive upload failed."
    };

    private static bool IsTransient(Exception? exception) =>
        exception is HttpRequestException or IOException ||
        exception is GoogleApiException apiException &&
        (apiException.HttpStatusCode == HttpStatusCode.RequestTimeout ||
         apiException.HttpStatusCode == (HttpStatusCode)429 ||
         (int)apiException.HttpStatusCode >= 500);
}
