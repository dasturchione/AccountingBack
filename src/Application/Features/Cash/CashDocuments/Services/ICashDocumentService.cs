using Application.Common.Pagination;
using Application.Features.CashOperations;
using SharedKernel.Results;

namespace Application.Features.CashDocuments;

public interface ICashDocumentService
{
    Task<Result<PagedResponse<CashOperationListDto>>> GetReceiptOrdersAsync(CashDocumentListFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<CashOperationListDto>>> GetPaymentOrdersAsync(CashDocumentListFilter filter, CancellationToken ct = default);
    Task<Result<CashOperationDto>> GetReceiptOrderByIdAsync(long id, CancellationToken ct = default);
    Task<Result<CashOperationDto>> GetPaymentOrderByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateReceiptOrderAsync(CashDocumentCreateDto dto, CancellationToken ct = default);
    Task<Result<long>> CreatePaymentOrderAsync(CashDocumentCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateReceiptOrderAsync(long id, CashDocumentUpdateDto dto, CancellationToken ct = default);
    Task<Result> UpdatePaymentOrderAsync(long id, CashDocumentUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmReceiptOrderAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmPaymentOrderAsync(long id, CancellationToken ct = default);
    Task<Result> CancelReceiptOrderAsync(long id, CancellationToken ct = default);
    Task<Result> CancelPaymentOrderAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteReceiptOrderAsync(long id, CancellationToken ct = default);
    Task<Result> DeletePaymentOrderAsync(long id, CancellationToken ct = default);
}
