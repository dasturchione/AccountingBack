namespace Application.Abstractions;

public interface IDocumentPostingLock
{
    Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default);
    Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default);
}
