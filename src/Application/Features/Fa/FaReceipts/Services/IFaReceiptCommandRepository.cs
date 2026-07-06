using Domain.Entities;

namespace Application.Features.FaReceipts;

public interface IFaReceiptCommandRepository
{
    Task CreateAsync(FaReceiptDoc entity, CancellationToken ct = default);
    Task UpdateAsync(FaReceiptDoc entity, CancellationToken ct = default);
    Task DeleteLinesAsync(IEnumerable<FaReceiptDocLine> entities, CancellationToken ct = default);
}
