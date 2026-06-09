using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Integration.GoogleDrive.Configs;
using Microsoft.Extensions.Options;

namespace Integration.GoogleDrive.Services;

public class GoogleDriveUploader : IGoogleDriveUploader
{
    private readonly GoogleDriveSettings _settings;

    public GoogleDriveUploader(IOptions<GoogleDriveSettings> options)
    {
        _settings = options.Value;
    }

    // DriveService faqat upload qilayotganda yaratiladi
    private DriveService CreateDriveService()
    {
        // ERP dek: to'liq yo'l = working directory + relative path
        var credentialPath = Path.IsPathRooted(_settings.CredentialsPath)
            ? _settings.CredentialsPath
            : Path.Combine(Directory.GetCurrentDirectory(), _settings.CredentialsPath);

        if (!System.IO.File.Exists(credentialPath))
            throw new FileNotFoundException(
                $"Google Drive credentials fayl topilmadi: {credentialPath}. " +
                $"Faylni shu yo'lga joylashtiring yoki appsettings.json dagi " +
                $"GoogleDrive.CredentialsPath ni to'g'irlang.");

        GoogleCredential credential;
        using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);
        credential = GoogleCredential.FromStream(stream)
            .CreateScoped(DriveService.Scope.DriveFile);

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

        var request = service.Files.Create(fileMetadata, fileStream, mimeType);
        request.Fields = "id";

        var result = await request.UploadAsync();

        if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
            throw new InvalidOperationException(
                $"Google Drive ga yuklashda xatolik: {result.Exception?.Message}");

        return $"https://drive.google.com/file/d/{request.ResponseBody.Id}/view";
    }
}
