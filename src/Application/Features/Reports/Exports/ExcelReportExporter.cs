using System.Globalization;
using ClosedXML.Excel;

namespace Application.Features.Reports.Exports;

/// <summary>
/// Excel export implementation based on ClosedXML.
/// </summary>
public sealed class ExcelReportExporter : IExcelExporter
{
    public byte[] Export<T>(string sheetName, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Report" : sheetName);

        for (var i = 0; i < columns.Count; i++)
            worksheet.Cell(1, i + 1).Value = columns.ElementAt(i).Header;

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                var value = columns.ElementAt(i).ValueFactory(row!);
                var cell = worksheet.Cell(rowIndex, i + 1);
                if (value is null)
                {
                    cell.Value = string.Empty;
                }
                else
                {
                    switch (value)
                    {
                        case DateTime dateTime:
                            cell.Value = dateTime;
                            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                            break;
                        case DateOnly dateOnly:
                            cell.Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                            cell.Style.DateFormat.Format = "yyyy-MM-dd";
                            break;
                        case decimal decimalValue:
                            cell.Value = decimalValue;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            break;
                        case double doubleValue:
                            cell.Value = doubleValue;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            break;
                        case float floatValue:
                            cell.Value = Convert.ToDouble(floatValue, CultureInfo.InvariantCulture);
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            break;
                        default:
                            cell.Value = value.ToString();
                            break;
                    }
                }
            }

            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
