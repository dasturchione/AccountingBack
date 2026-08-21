using Application.Features.Reports.Exports;
using SharedKernel.Results;

namespace Application.Features.Documents;

public interface IDocumentPdfService
{
    Task<Result<ReportExportResult>> GetPurchasePdfAsync(long id, CancellationToken ct = default);
    Task<Result<ReportExportResult>> GetSalePdfAsync(long id, CancellationToken ct = default);
}
