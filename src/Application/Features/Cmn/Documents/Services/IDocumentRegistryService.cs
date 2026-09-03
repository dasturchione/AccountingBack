using SharedKernel.Results;

namespace Application.Features.Cmn.Documents;

public interface IDocumentRegistryService
{
    Task<Result<List<DocumentRegistryDto>>> GetAllAsync(
        DocumentRegistryListFilter filter,
        CancellationToken ct = default);

    Task<Result<DocumentRegistryDto>> GetByIdAsync(long id, CancellationToken ct = default);
}
