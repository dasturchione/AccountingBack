using Microsoft.Extensions.Logging;

namespace Application.Features.Reports.Exports;

/// <summary>
/// Default report exporter that can emit Excel or PDF payloads.
/// </summary>
public sealed class ReportExporter : IReportExporter
{
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;
    private readonly ILogger<ReportExporter> _logger;

    public ReportExporter(IExcelExporter excelExporter, IPdfExporter pdfExporter, ILogger<ReportExporter> logger)
    {
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
        _logger = logger;
    }

    public Task<ReportExportResult> ExportAsync<T>(string reportName, ReportExportFormat format, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows, CancellationToken ct = default)
    {
        _ = ct;
        var fileName = string.IsNullOrWhiteSpace(reportName) ? "report" : reportName;
        var startedAt = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Export started for {ReportName} in {Format} with {RowCount} rows", fileName, format, rows.Count);

            var result = format == ReportExportFormat.Pdf
                ? new ReportExportResult
                {
                    FileName = $"{fileName}.pdf",
                    ContentType = "application/pdf",
                    Content = _pdfExporter.Export(fileName, columns, rows)
                }
                : new ReportExportResult
                {
                    FileName = $"{fileName}.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Content = _excelExporter.Export(fileName, columns, rows)
                };

            _logger.LogInformation(
                "Export completed for {ReportName} in {ElapsedMs} ms as {Format}",
                fileName,
                (DateTime.UtcNow - startedAt).TotalMilliseconds,
                format);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Export failed for {ReportName} in {Format} after {ElapsedMs} ms with {RowCount} rows",
                fileName,
                format,
                (DateTime.UtcNow - startedAt).TotalMilliseconds,
                rows.Count);
            throw;
        }
    }
}
