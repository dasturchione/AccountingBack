namespace Application.Features.Reports.Exports;

/// <summary>
/// Exports tabular data to Excel.
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// Creates an Excel workbook.
    /// </summary>
    byte[] Export<T>(string sheetName, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows);
}
