using System.Text;

namespace Application.Features.Reports.Exports;

/// <summary>
/// Enterprise PDF export implementation with reusable template rendering.
/// </summary>
public sealed class PdfReportExporter : IPdfExporter
{
    private readonly IPdfReportTemplate _template;

    public PdfReportExporter(IPdfReportTemplate template)
    {
        _template = template;
    }

    public byte[] Export<T>(string title, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows)
        => _template.Render(title, columns, rows);
}

/// <summary>
/// Default PDF report template with header, footer, table layout and pagination.
/// </summary>
public sealed class PdfReportTemplate : IPdfReportTemplate
{
    private const string CompanyName = "Accounting ERP";
    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float MarginLeft = 36f;
    private const float MarginRight = 36f;
    private const float MarginTop = 48f;
    private const float MarginBottom = 42f;
    private const float HeaderHeight = 56f;
    private const float FooterHeight = 28f;
    private const float RowHeight = 18f;
    private const float TableHeaderHeight = 20f;

    public byte[] Render<T>(string reportTitle, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows)
    {
        var title = string.IsNullOrWhiteSpace(reportTitle) ? "Report" : reportTitle;
        var generatedAt = DateTime.UtcNow;
        var pages = BuildPages(title, columns, rows, generatedAt);
        return BuildPdfDocument(pages);
    }

    private static List<PdfPageModel> BuildPages<T>(string title, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows, DateTime generatedAt)
    {
        var pages = new List<PdfPageModel>();
        var columnWidths = CalculateColumnWidths(columns);
        var maxRowsPerPage = Math.Max(1, (int)Math.Floor((PageHeight - MarginTop - MarginBottom - HeaderHeight - FooterHeight - TableHeaderHeight) / RowHeight));
        var totalPages = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)maxRowsPerPage));

        var currentRows = rows
            .Select((row, index) => new PdfRowModel(index + 1, columns.Select(column => FormatValue(column.ValueFactory(row!))).ToArray()))
            .ToList();

        for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            var pageRows = currentRows.Skip(pageIndex * maxRowsPerPage).Take(maxRowsPerPage).ToList();
            pages.Add(new PdfPageModel(title, generatedAt, pageIndex + 1, totalPages, columns.Select(c => c.Header).ToArray(), columnWidths, pageRows));
        }

        if (pages.Count == 0)
            pages.Add(new PdfPageModel(title, generatedAt, 1, 1, columns.Select(c => c.Header).ToArray(), columnWidths, []));

        return pages;
    }

    private static float[] CalculateColumnWidths(IReadOnlyCollection<ReportExportColumn> columns)
    {
        if (columns.Count == 0)
            return [1f];

        var widths = columns.Select(column => Math.Max(48f, Math.Min(150f, column.Header.Length * 6f + 18f))).ToArray();
        var total = widths.Sum();
        var usable = PageWidth - MarginLeft - MarginRight;
        if (total <= 0)
            return Enumerable.Repeat(usable / columns.Count, columns.Count).ToArray();

        return widths.Select(width => width * usable / total).ToArray();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm"),
            DateOnly dateOnly => dateOnly.ToString("yyyy-MM-dd"),
            decimal decimalValue => decimalValue.ToString("N2"),
            double doubleValue => doubleValue.ToString("N2"),
            float floatValue => floatValue.ToString("N2"),
            _ => Convert.ToString(value) ?? string.Empty
        };
    }

    private static byte[] BuildPdfDocument(IReadOnlyCollection<PdfPageModel> pages)
    {
        var objects = new List<string>();
        objects.Add("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n");

        var pageRefs = new List<int>();
        var pageObjectIndex = 4;
        var contentObjectIndex = 5;

        objects.Add("2 0 obj << /Type /Pages /Kids [] /Count 0 >> endobj\n");
        objects.Add("3 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n");

        foreach (var page in pages)
        {
            var content = BuildContent(page);
            var contentBytes = Encoding.ASCII.GetBytes(content);

            objects.Add($"{pageObjectIndex} 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth} {PageHeight}] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObjectIndex} 0 R >> endobj\n");
            objects.Add($"{contentObjectIndex} 0 obj << /Length {contentBytes.Length} >> stream\n{content}\nendstream endobj\n");
            pageRefs.Add(pageObjectIndex);

            pageObjectIndex += 2;
            contentObjectIndex += 2;
        }

        var kids = string.Join(" ", pageRefs.Select(x => $"{x} 0 R"));
        objects[1] = $"2 0 obj << /Type /Pages /Kids [{kids}] /Count {pages.Count} >> endobj\n";

        return ComposePdf(objects);
    }

    private static string BuildContent(PdfPageModel page)
    {
        var content = new StringBuilder();
        var startY = PageHeight - MarginTop;

        content.Append("BT /F1 11 Tf ");
        content.Append($"{MarginLeft} {startY} Td ");
        content.Append($"({Escape(ToPdfSafeText(CompanyName, 48))}) Tj ");

        content.Append($"0 -16 Td ");
        content.Append($"({Escape(ToPdfSafeText(page.Title, 56))}) Tj ");

        content.Append("0 -14 Td ");
        content.Append($"(Generated: {Escape(ToPdfSafeText(page.GeneratedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), 56))}) Tj ");

        content.Append("ET\n");
        content.Append(DrawTable(page));
        return content.ToString();
    }

    private static string DrawTable(PdfPageModel page)
    {
        var content = new StringBuilder();
        var top = PageHeight - MarginTop - HeaderHeight;
        var tableWidth = PageWidth - MarginLeft - MarginRight;
        var xPositions = new float[page.ColumnHeaders.Length + 1];
        xPositions[0] = MarginLeft;
        for (var i = 0; i < page.ColumnWidths.Length; i++)
            xPositions[i + 1] = xPositions[i] + page.ColumnWidths[i];

        content.AppendLine("q");
        content.AppendLine("0.95 g");
        content.AppendLine($"{MarginLeft} {top} {tableWidth} {TableHeaderHeight} re f");
        content.AppendLine("0 G");
        content.AppendLine($"{MarginLeft} {top} {tableWidth} {TableHeaderHeight} re S");

        for (var i = 0; i < page.ColumnHeaders.Length; i++)
        {
            content.AppendLine(DrawText(page.ColumnHeaders[i], xPositions[i] + 4, top + 6, 8.5f, false, page.ColumnWidths[i]));
            if (i < page.ColumnHeaders.Length - 1)
                content.AppendLine($"{xPositions[i + 1]} {top} m {xPositions[i + 1]} {top + TableHeaderHeight} l S");
        }

        var y = top - RowHeight;
        for (var rowIndex = 0; rowIndex < page.Rows.Count; rowIndex++)
        {
            if (rowIndex % 2 == 0)
            {
                content.AppendLine("0.98 g");
                content.AppendLine($"{MarginLeft} {y} {tableWidth} {RowHeight} re f");
                content.AppendLine("0 G");
            }

            content.AppendLine($"{MarginLeft} {y} {tableWidth} {RowHeight} re S");
            for (var i = 0; i < page.ColumnHeaders.Length; i++)
            {
                var cell = page.Rows[rowIndex].Values[i];
                var alignRight = IsNumericLike(cell);
                var textX = alignRight ? xPositions[i + 1] - 4 : xPositions[i] + 4;
                content.AppendLine(DrawText(cell, textX, y + 5, 8f, alignRight, page.ColumnWidths[i]));
                if (i < page.ColumnHeaders.Length - 1)
                    content.AppendLine($"{xPositions[i + 1]} {y} m {xPositions[i + 1]} {y + RowHeight} l S");
            }

            y -= RowHeight;
        }

        content.AppendLine("Q");
        content.AppendLine(DrawFooter(page));
        return content.ToString();
    }

    private static string DrawFooter(PdfPageModel page)
    {
        var footerY = MarginBottom - 4;
        var lineY = MarginBottom + 6;
        var content = new StringBuilder();
        content.AppendLine($"36 {lineY} m {PageWidth - 36} {lineY} l S");
        content.AppendLine(DrawText("Generated by Accounting ERP", MarginLeft, footerY, 7.5f));
        content.AppendLine(DrawText($"Page {page.PageNumber} of {page.TotalPages}", PageWidth - MarginRight - 90, footerY, 7.5f));
        return content.ToString();
    }

    private static string DrawText(string text, float x, float y, float fontSize, bool alignRight = false, float columnWidth = 0)
    {
        var safeText = columnWidth > 0 ? TruncateToWidth(text, columnWidth, fontSize) : text;
        var escaped = Escape(ToPdfSafeText(safeText, 120));
        var command = new StringBuilder("BT /F1 ");
        command.Append(fontSize.ToString("0.##")).Append(" Tf ");
        if (alignRight)
            command.Append($"1 0 0 1 {x} {y} Tm ({escaped}) Tj ET");
        else
            command.Append($"{x} {y} Td ({escaped}) Tj ET");
        return command.ToString();
    }

    private static bool IsNumericLike(string value) =>
        decimal.TryParse(value.Replace(",", string.Empty), out _);

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static string ToPdfSafeText(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(ch <= 0x7F ? ch : '?');
        }

        var safe = builder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        return safe.Length <= maxLength ? safe : safe[..Math.Max(0, maxLength - 3)] + "...";
    }

    private static string TruncateToWidth(string value, float columnWidth, float fontSize)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var approxChars = Math.Max(6, (int)Math.Floor(columnWidth / (fontSize * 0.55f)));
        return value.Length <= approxChars ? value : value[..Math.Max(0, approxChars - 3)] + "...";
    }

    private static byte[] ComposePdf(IReadOnlyCollection<string> objects)
    {
        var bytes = new List<byte>();
        bytes.AddRange(Encoding.ASCII.GetBytes("%PDF-1.4\n"));

        var offsets = new List<int> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(bytes.Count);
            bytes.AddRange(Encoding.ASCII.GetBytes(obj));
        }

        var xrefStart = bytes.Count;
        bytes.AddRange(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n"));
        for (var i = 1; i < offsets.Count; i++)
            bytes.AddRange(Encoding.ASCII.GetBytes($"{offsets[i]:0000000000} 00000 n \n"));

        bytes.AddRange(Encoding.ASCII.GetBytes($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefStart}\n%%EOF"));
        return bytes.ToArray();
    }

    private sealed record PdfPageModel(
        string Title,
        DateTime GeneratedAt,
        int PageNumber,
        int TotalPages,
        string[] ColumnHeaders,
        float[] ColumnWidths,
        List<PdfRowModel> Rows);

    private sealed record PdfRowModel(int Index, string[] Values);
}
