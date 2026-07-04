namespace Application.Features.Reports.Exports;

/// <summary>
/// Exports a report payload into a file representation.
/// </summary>
public interface IReportExporter
{
    /// <summary>
    /// Exports a report into the specified format.
    /// </summary>
    Task<ReportExportResult> ExportAsync<T>(string reportName, ReportExportFormat format, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows, CancellationToken ct = default);
}
