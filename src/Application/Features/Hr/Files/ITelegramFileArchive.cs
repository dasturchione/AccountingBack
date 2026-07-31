namespace Application.Features.Hr.Files;

public sealed record TelegramArchivedFile(string FileId, int MessageId);

public interface ITelegramFileArchive
{
    Task<TelegramArchivedFile> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string caption,
        CancellationToken ct = default);

    Task<Stream> DownloadAsync(string fileId, CancellationToken ct = default);
    Task DeleteMessageAsync(int messageId, CancellationToken ct = default);
}
