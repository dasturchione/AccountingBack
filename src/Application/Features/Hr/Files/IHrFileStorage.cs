namespace Application.Features.Hr.Files;

public sealed record HrFileUpload(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);

public sealed record HrStoredFile(
    string RelativePath,
    string AbsolutePath,
    string StoredFileName);

public interface IHrFileStorage
{
    Task<HrStoredFile> SaveAsync(
        int organizationId,
        long absenceId,
        HrFileUpload file,
        CancellationToken ct = default);

    Task RestoreAsync(string relativePath, Stream content, CancellationToken ct = default);
    Task<Stream?> OpenAsync(string relativePath, CancellationToken ct = default);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
    bool Exists(string relativePath);
}
