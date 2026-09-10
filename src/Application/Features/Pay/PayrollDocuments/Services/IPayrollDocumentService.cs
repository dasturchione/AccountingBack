using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.PayrollDocuments;

public interface IPayrollDocumentService
{
    Task<Result<PagedResponse<PayrollDocumentListDto>>> GetAllAsync(PayrollDocumentListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollDocumentDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CalculateAsync(PayrollCalculateDto dto, CancellationToken ct = default);
    Task<Result<long>> RecalculateAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
