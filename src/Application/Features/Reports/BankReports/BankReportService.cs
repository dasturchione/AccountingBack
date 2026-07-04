using Application.Common.Pagination;
using Application.Features.BankOperations;
using SharedKernel.Results;

namespace Application.Features.Reports.BankReports;

public interface IBankReportService
{
    Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default);
    Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default);
}

public sealed class BankReportService : IBankReportService
{
    private readonly IBankOperationService _bankOperationService;

    public BankReportService(IBankOperationService bankOperationService)
    {
        _bankOperationService = bankOperationService;
    }

    public Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default) =>
        _bankOperationService.GetAllAsync(filter, ct);

    public Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        _bankOperationService.GetByIdAsync(id, ct);
}
