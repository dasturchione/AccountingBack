using SharedKernel.Results;

namespace Application.Features.DocumentNumbers;

public sealed record DocumentNumberResult(
    long SequenceNumber,
    string DocumentNumber,
    DateTime DocumentDate);

public interface IDocumentNumberService
{
    Task<Result<DocumentNumberResult>> GetNextAsync(
        int organizationId,
        short documentTypeId,
        DateTime documentDate,
        CancellationToken ct = default);

    Task<Result<DocumentNumberResult>> GetNextHistoricalAsync(
        int organizationId,
        short documentTypeId,
        DateTime documentDate,
        CancellationToken ct = default);
}
