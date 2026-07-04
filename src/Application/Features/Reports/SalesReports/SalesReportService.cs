using Application.Common.Pagination;
using Application.Features.SaleDocs;
using SharedKernel.Results;

namespace Application.Features.Reports.SalesReports;

public interface ISalesReportService
{
    Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default);
    Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default);
}

public sealed class SalesReportService : ISalesReportService
{
    private readonly ISaleDocService _saleDocService;

    public SalesReportService(ISaleDocService saleDocService)
    {
        _saleDocService = saleDocService;
    }

    public Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default) =>
        _saleDocService.GetAllAsync(filter, ct);

    public Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        _saleDocService.GetByIdAsync(id, ct);
}
