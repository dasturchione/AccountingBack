namespace Application.Features.Reports.Exports;

/// <summary>
/// Builds a PDF document representation for a tabular report.
/// </summary>
public interface IPdfReportTemplate
{
    /// <summary>
    /// Renders a report into a PDF byte array.
    /// </summary>
    byte[] Render<T>(string reportTitle, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows);
}
