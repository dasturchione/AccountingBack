namespace Integration.GoogleDrive.Services;

public interface IGoogleDriveUploader
{
    Task<string> UploadAsync(string filePath, string mimeType);
    Task<string> UploadAsync(Stream fileStream, string fileName, string mimeType);
}
