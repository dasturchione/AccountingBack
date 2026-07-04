using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRevaluations;

public interface ICurrencyRevaluationService
{
    Task<Result<PagedResponse<CurrencyRevaluationListDto>>> GetAllAsync(CurrencyRevaluationListFilter filter, CancellationToken ct = default);
    Task<Result<CurrencyRevaluationDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<CurrencyRevaluationDto>> PreviewAsync(CurrencyRevaluationPreviewDto dto, CancellationToken ct = default);
    Task<Result<CurrencyRevaluationDto>> CreateAsync(CurrencyRevaluationCreateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
