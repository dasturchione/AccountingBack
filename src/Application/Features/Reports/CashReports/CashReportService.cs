using Application.Common.Pagination;
using Application.Features.CashOperations;
using SharedKernel.Results;

namespace Application.Features.Reports.CashReports;

public interface ICashReportService
{
    Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default);
    Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default);
}

public sealed class CashReportService : ICashReportService
{
    private readonly ICashOperationService _cashOperationService;

    public CashReportService(ICashOperationService cashOperationService)
    {
        _cashOperationService = cashOperationService;
    }

    public Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default) =>
        _cashOperationService.GetAllAsync(filter, ct);

    public Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        _cashOperationService.GetByIdAsync(id, ct);
}
