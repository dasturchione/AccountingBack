namespace Application.Features.Reports.Exports;

/// <summary>
/// Exports tabular data to PDF.
/// </summary>
public interface IPdfExporter
{
    /// <summary>
    /// Creates a PDF document.
    /// </summary>
    byte[] Export<T>(string title, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows);
}
