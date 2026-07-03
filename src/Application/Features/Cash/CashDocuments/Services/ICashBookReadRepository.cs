namespace Application.Features.CashDocuments;

public interface ICashBookReadRepository
{
    Task<CashBookReadResult> GetAsync(CashBookReadRequest request, CancellationToken ct = default);
}
