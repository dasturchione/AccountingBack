using SharedKernel.Results;

namespace Application.Features.CashDocuments;

public interface ICashBookService
{
    Task<Result<CashBookDto>> GetAsync(CashBookFilter filter, CancellationToken ct = default);
}
