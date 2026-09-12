using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.PayrollDocuments;

public interface IPayrollDocumentService
{
    Task<Result<PagedResponse<PayrollDocumentListDto>>> GetAllAsync(PayrollDocumentListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollDocumentDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CalculateAsync(PayrollCalculateDto dto, CancellationToken ct = default);
    Task<Result<long>> RecalculateAsync(long id, CancellationToken ct = default);

    /// <summary>Tuzatish uchun asos: manba posted hujjatdagi joriy summalar (UI "eski qiymat"ni ko'rsatadi).</summary>
    Task<Result<PayrollCorrectionBasisDto>> GetCorrectionBasisAsync(long sourceDocId, CancellationToken ct = default);

    /// <summary>DRAFT/PENDING hujjatda provodka schyotlari va komponent/soliq summalarini tahrirlash.</summary>
    Task<Result> UpdateDraftAsync(long id, PayrollDraftUpdateDto dto, CancellationToken ct = default);

    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
