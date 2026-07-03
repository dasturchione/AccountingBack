namespace Application.Features.Ledger;

public interface ILedgerReadRepository
{
    Task<LedgerReadResult> GetAsync(LedgerReadRequest request, CancellationToken ct = default);
}
