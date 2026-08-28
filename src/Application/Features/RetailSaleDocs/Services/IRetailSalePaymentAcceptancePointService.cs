using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public interface IRetailSalePaymentAcceptancePointService
{
    Task<Result> PostAsync(RetailSaleDoc document, CancellationToken ct = default);
    Task<Result> ReverseAsync(RetailSaleDoc document, CancellationToken ct = default);
}
