using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

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
/// PDFsharp (MIT) asosidagi PDF report shabloni. To'liq Unicode qo'llab-quvvatlaydi
/// (Kirill: ў,ғ,қ,ҳ,ъ; Lotin diakritika) — embedded DejaVu Sans TTF shrifti bilan.
/// Sarlavha, jadval, sahifalash saqlangan.
/// </summary>
public sealed class PdfReportTemplate : IPdfReportTemplate
{
    private const string CompanyName = "Accounting ERP";
    private const string FontFamily = "DejaVuSans";

    private const double PageWidth = 595.28;   // A4
    private const double PageHeight = 841.89;
    private const double MarginLeft = 36;
    private const double MarginRight = 36;
    private const double MarginTop = 40;
    private const double MarginBottom = 40;
    private const double HeaderBlockHeight = 58;
    private const double TableHeaderHeight = 20;
    private const double RowHeight = 18;
    private const double FooterHeight = 24;

    private static readonly XColor HeaderFill = XColor.FromArgb(242, 242, 242);
    private static readonly XColor RowFill = XColor.FromArgb(250, 250, 250);
    private static readonly XColor BorderColor = XColor.FromArgb(0, 0, 0);

    public byte[] Render<T>(string reportTitle, IReadOnlyCollection<ReportExportColumn> columns, IReadOnlyCollection<T> rows)
    {
        UnicodeFontResolver.EnsureRegistered();

        var title = string.IsNullOrWhiteSpace(reportTitle) ? "Report" : reportTitle;
        var generatedAt = DateTime.UtcNow;
        var headers = columns.Select(c => c.Header).ToArray();
        var columnWidths = CalculateColumnWidths(columns);

        var dataRows = rows
            .Select(row => columns.Select(column => FormatValue(column.ValueFactory(row!))).ToArray())
            .ToList();

        var usableHeight = PageHeight - MarginTop - MarginBottom - HeaderBlockHeight - TableHeaderHeight - FooterHeight;
        var rowsPerPage = Math.Max(1, (int)Math.Floor(usableHeight / RowHeight));
        var totalPages = Math.Max(1, (int)Math.Ceiling(dataRows.Count / (double)rowsPerPage));

        using var document = new PdfDocument();

        var titleFont = new XFont(FontFamily, 13, XFontStyleEx.Bold);
        var companyFont = new XFont(FontFamily, 10, XFontStyleEx.Regular);
        var metaFont = new XFont(FontFamily, 8, XFontStyleEx.Regular);
        var headerFont = new XFont(FontFamily, 8.5, XFontStyleEx.Bold);
        var cellFont = new XFont(FontFamily, 8, XFontStyleEx.Regular);
        var footerFont = new XFont(FontFamily, 7.5, XFontStyleEx.Regular);

        for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(PageWidth);
            page.Height = XUnit.FromPoint(PageHeight);

            using var gfx = XGraphics.FromPdfPage(page);

            DrawHeader(gfx, title, generatedAt, titleFont, companyFont, metaFont);

            var pageRows = dataRows.Skip(pageIndex * rowsPerPage).Take(rowsPerPage).ToList();
            DrawTable(gfx, headers, columnWidths, pageRows, headerFont, cellFont);

            DrawFooter(gfx, pageIndex + 1, totalPages, footerFont);
        }

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }

    private static void DrawHeader(XGraphics gfx, string title, DateTime generatedAt, XFont titleFont, XFont companyFont, XFont metaFont)
    {
        var y = MarginTop;
        gfx.DrawString(CompanyName, companyFont, XBrushes.Gray, new XRect(MarginLeft, y, PageWidth - MarginLeft - MarginRight, 14), XStringFormats.TopLeft);
        y += 16;
        gfx.DrawString(title, titleFont, XBrushes.Black, new XRect(MarginLeft, y, PageWidth - MarginLeft - MarginRight, 18), XStringFormats.TopLeft);
        y += 20;
        gfx.DrawString($"Generated: {generatedAt.ToLocalTime():yyyy-MM-dd HH:mm}", metaFont, XBrushes.Gray, new XRect(MarginLeft, y, PageWidth - MarginLeft - MarginRight, 12), XStringFormats.TopLeft);
    }

    private static void DrawTable(XGraphics gfx, string[] headers, double[] columnWidths, List<string[]> rows, XFont headerFont, XFont cellFont)
    {
        var tableWidth = PageWidth - MarginLeft - MarginRight;
        var top = MarginTop + HeaderBlockHeight;

        var xPositions = new double[headers.Length + 1];
        xPositions[0] = MarginLeft;
        for (var i = 0; i < columnWidths.Length; i++)
            xPositions[i + 1] = xPositions[i] + columnWidths[i];

        var borderPen = new XPen(BorderColor, 0.5);

        // Header row
        gfx.DrawRectangle(new XSolidBrush(HeaderFill), MarginLeft, top, tableWidth, TableHeaderHeight);
        gfx.DrawRectangle(borderPen, MarginLeft, top, tableWidth, TableHeaderHeight);
        for (var i = 0; i < headers.Length; i++)
        {
            var cellRect = new XRect(xPositions[i] + 3, top, columnWidths[i] - 6, TableHeaderHeight);
            gfx.DrawString(Fit(gfx, headers[i], headerFont, columnWidths[i] - 6), headerFont, XBrushes.Black, cellRect, XStringFormats.CenterLeft);
            if (i < headers.Length - 1)
                gfx.DrawLine(borderPen, xPositions[i + 1], top, xPositions[i + 1], top + TableHeaderHeight);
        }

        // Data rows
        var y = top + TableHeaderHeight;
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            if (rowIndex % 2 == 0)
                gfx.DrawRectangle(new XSolidBrush(RowFill), MarginLeft, y, tableWidth, RowHeight);

            gfx.DrawRectangle(borderPen, MarginLeft, y, tableWidth, RowHeight);

            for (var i = 0; i < headers.Length; i++)
            {
                var alignRight = IsNumericLike(row[i]);
                var cellRect = new XRect(xPositions[i] + 3, y, columnWidths[i] - 6, RowHeight);
                var format = alignRight ? XStringFormats.CenterRight : XStringFormats.CenterLeft;
                gfx.DrawString(Fit(gfx, row[i], cellFont, columnWidths[i] - 6), cellFont, XBrushes.Black, cellRect, format);

                if (i < headers.Length - 1)
                    gfx.DrawLine(borderPen, xPositions[i + 1], y, xPositions[i + 1], y + RowHeight);
            }

            y += RowHeight;
        }
    }

    private static void DrawFooter(XGraphics gfx, int pageNumber, int totalPages, XFont footerFont)
    {
        var y = PageHeight - MarginBottom - FooterHeight + 8;
        var lineY = y - 4;
        gfx.DrawLine(new XPen(BorderColor, 0.5), MarginLeft, lineY, PageWidth - MarginRight, lineY);
        gfx.DrawString("Generated by Accounting ERP", footerFont, XBrushes.Gray, new XRect(MarginLeft, y, 300, 12), XStringFormats.TopLeft);
        gfx.DrawString($"Page {pageNumber} of {totalPages}", footerFont, XBrushes.Gray, new XRect(PageWidth - MarginRight - 120, y, 120, 12), XStringFormats.TopRight);
    }

    private static double[] CalculateColumnWidths(IReadOnlyCollection<ReportExportColumn> columns)
    {
        var usable = PageWidth - MarginLeft - MarginRight;
        if (columns.Count == 0)
            return [usable];

        var widths = columns.Select(column => Math.Max(48d, Math.Min(150d, column.Header.Length * 6d + 18d))).ToArray();
        var total = widths.Sum();
        if (total <= 0)
            return Enumerable.Repeat(usable / columns.Count, columns.Count).ToArray();

        return widths.Select(width => width * usable / total).ToArray();
    }

    private static string Fit(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
            return text;

        if (gfx.MeasureString(text, font).Width <= maxWidth)
            return text;

        const string ellipsis = "...";
        var result = text;
        while (result.Length > 1 && gfx.MeasureString(result + ellipsis, font).Width > maxWidth)
            result = result[..^1];

        return result + ellipsis;
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

    private static bool IsNumericLike(string value) =>
        !string.IsNullOrWhiteSpace(value) && decimal.TryParse(value.Replace(",", string.Empty).Replace(" ", string.Empty), out _);
}

/// <summary>
/// PDFsharp uchun embedded Unicode (DejaVu Sans) shrift resolveri.
/// Barcha oila/uslub so'rovlariga bitta Unicode TTF qaytaradi — Kirill+Lotin qamrovi kafolatlanadi.
/// </summary>
internal sealed class UnicodeFontResolver : IFontResolver
{
    private const string FaceName = "DejaVuSans";
    private static readonly object Gate = new();
    private static bool _registered;
    private static byte[]? _fontData;

    public static void EnsureRegistered()
    {
        if (_registered)
            return;

        lock (Gate)
        {
            if (_registered)
                return;

            _fontData = LoadFontData();
            GlobalFontSettings.FontResolver = new UnicodeFontResolver();
            _registered = true;
        }
    }

    public byte[]? GetFont(string faceName) => _fontData;

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        => new FontResolverInfo(FaceName);

    private static byte[] LoadFontData()
    {
        var assembly = typeof(UnicodeFontResolver).Assembly;
        var resourceName = Array.Find(
            assembly.GetManifestResourceNames(),
            n => n.EndsWith("DejaVuSans.ttf", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("DejaVuSans.ttf embedded resource topilmadi.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
