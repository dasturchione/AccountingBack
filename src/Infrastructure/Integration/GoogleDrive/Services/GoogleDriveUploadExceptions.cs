using System.Net;

namespace Integration.GoogleDrive.Services;

public sealed class GoogleDriveConfigurationException(string message) : Exception(message);

public enum GoogleDriveUploadSafeReason
{
    Unauthorized,
    Forbidden,
    NotFound,
    RateLimited,
    TransientNetwork,
    Other
}

public class GoogleDriveUploadException : Exception
{
    public GoogleDriveUploadException(
        string message,
        HttpStatusCode? httpStatusCode = null,
        GoogleDriveUploadSafeReason safeReason = GoogleDriveUploadSafeReason.Other)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        SafeReason = safeReason;
    }

    public HttpStatusCode? HttpStatusCode { get; }

    public GoogleDriveUploadSafeReason SafeReason { get; }
}

public sealed class GoogleDrivePermissionException(
    string message,
    HttpStatusCode httpStatusCode,
    GoogleDriveUploadSafeReason safeReason)
    : GoogleDriveUploadException(message, httpStatusCode, safeReason);
