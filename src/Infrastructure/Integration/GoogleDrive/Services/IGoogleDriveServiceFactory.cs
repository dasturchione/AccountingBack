using Google.Apis.Drive.v3;

namespace Integration.GoogleDrive.Services;

public interface IGoogleDriveServiceFactory
{
    Task<DriveService> CreateAsync(CancellationToken cancellationToken = default);
}
