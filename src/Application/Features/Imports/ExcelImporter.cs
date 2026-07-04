using Application.Abstractions.Import;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Application.Features.Imports;

/// <summary>
/// Excel (.xlsx) va CSV importi. .xlsx uchun ClosedXML (BankStatementParserService namunasi),
/// CSV uchun ichki minimal parser (qo'shimcha paketsiz). Reflection orqali tipli map.
/// </summary>
public sealed class ExcelImporter : IExcelImporter
{
    private readonly ILogger<ExcelImporter> _logger;

    public ExcelImporter(ILogger<ExcelImporter> logger)
    {
        _logger = logger;
    }

    public Task<Result<List<Dictionary<string, string>>>> ImportRawAsync(Stream fileStream, ExcelImportOptions options, CancellationToken ct = default)
    {
        var read = ReadRows(fileStream, options);
        if (!read.IsSuccess)
            return Task.FromResult(Result.Failure<List<Dictionary<string, string>>>(read.Error));

        var rows = read.Value.Rows.Select(x => x.Cells).ToList();
        return Task.FromResult(Result.Success(rows));
    }

    public Task<Result<ExcelImportResult<T>>> ImportAsync<T>(Stream fileStream, ExcelImportOptions options, CancellationToken ct = default) where T : new()
    {
        var read = ReadRows(fileStream, options);
        if (!read.IsSuccess)
            return Task.FromResult(Result.Failure<ExcelImportResult<T>>(read.Error));

        var headers = read.Value.Headers;
        var rows = read.Value.Rows;

        // Property -> Excel ustun nomi (ColumnMapping: header -> propertyName).
        var propertyColumns = new List<(PropertyInfo Property, string Header)>();
        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite)
                continue;

            var header = ResolveHeader(property.Name, options.ColumnMapping);
            propertyColumns.Add((property, header));
        }

        // Hech qaysi kutilgan ustun sheet'da topilmasa — bu noto'g'ri fayl.
        var headerSet = new HashSet<string>(headers, StringComparer.OrdinalIgnoreCase);
        if (!propertyColumns.Any(pc => headerSet.Contains(pc.Header)))
        {
            return Task.FromResult(Result.Failure<ExcelImportResult<T>>(
                new Error("Import.NoMatchingColumns", "Faylда mos ustunlar topilmadi.", ErrorType.Validation)));
        }

        var result = new ExcelImportResult<T> { TotalDataRows = rows.Count };

        foreach (var row in rows)
        {
            try
            {
                var item = new T();
                foreach (var (property, header) in propertyColumns)
                {
                    if (!row.Cells.TryGetValue(header, out var raw))
                        continue;

                    var value = ConvertValue(raw, property.PropertyType);
                    property.SetValue(item, value);
                }

                result.Items.Add(item);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ExcelRowError { RowNumber = row.RowNumber, Message = ex.Message });
            }
        }

        return Task.FromResult(Result.Success(result));
    }

    private Result<ReadData> ReadRows(Stream fileStream, ExcelImportOptions options)
    {
        try
        {
            using var buffer = new MemoryStream();
            fileStream.CopyTo(buffer);
            buffer.Position = 0;

            if (buffer.Length == 0)
                return Result.Failure<ReadData>(new Error("Import.EmptyFile", "Fayl bo'sh.", ErrorType.Validation));

            // .xlsx = ZIP ("PK" = 0x50 0x4B); aks holda CSV deb qaraladi.
            var isXlsx = buffer.Length > 2 && buffer.GetBuffer()[0] == 0x50 && buffer.GetBuffer()[1] == 0x4B;
            buffer.Position = 0;

            return isXlsx ? ReadXlsx(buffer, options) : ReadCsv(buffer, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import faylini o'qishда xato");
            return Result.Failure<ReadData>(new Error("Import.ReadFailed", ex.Message, ErrorType.Validation));
        }
    }

    private static Result<ReadData> ReadXlsx(Stream stream, ExcelImportOptions options)
    {
        using var workbook = new XLWorkbook(stream);

        IXLWorksheet worksheet;
        if (string.IsNullOrWhiteSpace(options.SheetName))
        {
            worksheet = workbook.Worksheets.First();
        }
        else if (!workbook.Worksheets.TryGetWorksheet(options.SheetName, out worksheet!))
        {
            return Result.Failure<ReadData>(new Error("Import.SheetNotFound", $"Varaq topilmadi: {options.SheetName}.", ErrorType.Validation));
        }

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        var headers = new List<string>();
        for (var col = 1; col <= lastCol; col++)
            headers.Add(worksheet.Cell(options.HeaderRow, col).GetFormattedString().Trim());

        var rows = new List<RawRow>();
        for (var row = options.DataStartRow; row <= lastRow; row++)
        {
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var allEmpty = true;

            for (var col = 1; col <= lastCol; col++)
            {
                var header = headers[col - 1];
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                var value = worksheet.Cell(row, col).GetFormattedString().Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    allEmpty = false;

                cells[header] = value;
            }

            if (allEmpty && options.SkipEmptyRows)
                continue;

            rows.Add(new RawRow(row, cells));
        }

        return Result.Success(new ReadData(headers.Where(h => !string.IsNullOrWhiteSpace(h)).ToList(), rows));
    }

    private static Result<ReadData> ReadCsv(Stream stream, ExcelImportOptions options)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
            lines.Add(line);

        if (lines.Count < options.HeaderRow)
            return Result.Failure<ReadData>(new Error("Import.NoHeader", "Sarlavha qatori topilmadi.", ErrorType.Validation));

        var headerLine = lines[options.HeaderRow - 1];
        var delimiter = DetectDelimiter(headerLine);
        var headers = ParseCsvLine(headerLine, delimiter).Select(h => h.Trim()).ToList();

        var rows = new List<RawRow>();
        for (var i = options.DataStartRow - 1; i < lines.Count; i++)
        {
            var fields = ParseCsvLine(lines[i], delimiter);
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var allEmpty = true;

            for (var col = 0; col < headers.Count; col++)
            {
                var header = headers[col];
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                var value = col < fields.Count ? fields[col].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    allEmpty = false;

                cells[header] = value;
            }

            if (allEmpty && options.SkipEmptyRows)
                continue;

            rows.Add(new RawRow(i + 1, cells));
        }

        return Result.Success(new ReadData(headers.Where(h => !string.IsNullOrWhiteSpace(h)).ToList(), rows));
    }

    private static char DetectDelimiter(string headerLine)
    {
        var candidates = new[] { ',', ';', '\t' };
        return candidates
            .OrderByDescending(c => headerLine.Count(ch => ch == c))
            .First();
    }

    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(ch);
                }
            }
            else if (ch == '"')
            {
                inQuotes = true;
            }
            else if (ch == delimiter)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    private static string ResolveHeader(string propertyName, Dictionary<string, string>? mapping)
    {
        if (mapping is not null)
        {
            foreach (var kvp in mapping)
            {
                if (string.Equals(kvp.Value, propertyName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
        }

        return propertyName;
    }

    private static object? ConvertValue(string raw, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var isReferenceOrNullable = Nullable.GetUnderlyingType(targetType) is not null || !targetType.IsValueType;

        if (string.IsNullOrWhiteSpace(raw))
            return isReferenceOrNullable ? null : Activator.CreateInstance(targetType);

        var trimmed = raw.Trim();

        if (underlying == typeof(string))
            return trimmed;
        if (underlying == typeof(bool))
            return ParseBool(trimmed);
        if (underlying.IsEnum)
            return Enum.Parse(underlying, trimmed, ignoreCase: true);
        if (underlying == typeof(Guid))
            return Guid.Parse(trimmed);
        if (underlying == typeof(DateTime))
            return ParseDateTime(trimmed);
        if (underlying == typeof(DateOnly))
            return DateOnly.FromDateTime(ParseDateTime(trimmed));

        var normalized = trimmed.Replace(" ", string.Empty).Replace(" ", string.Empty).Replace(",", ".");
        var ci = CultureInfo.InvariantCulture;

        if (underlying == typeof(int)) return int.Parse(normalized, NumberStyles.Any, ci);
        if (underlying == typeof(long)) return long.Parse(normalized, NumberStyles.Any, ci);
        if (underlying == typeof(short)) return short.Parse(normalized, NumberStyles.Any, ci);
        if (underlying == typeof(byte)) return byte.Parse(normalized, NumberStyles.Any, ci);
        if (underlying == typeof(decimal)) return decimal.Parse(normalized, NumberStyles.Any, ci);
        if (underlying == typeof(double)) return double.Parse(normalized, NumberStyles.Any, ci);
        if (underlying == typeof(float)) return float.Parse(normalized, NumberStyles.Any, ci);

        return Convert.ChangeType(trimmed, underlying, ci);
    }

    private static bool ParseBool(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "ha" or "y" => true,
            "0" or "false" or "no" or "yo'q" or "yoq" or "n" => false,
            _ => bool.Parse(value)
        };
    }

    private static DateTime ParseDateTime(string value)
    {
        string[] formats = ["dd.MM.yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "dd.MM.yyyy HH:mm", "yyyy-MM-dd HH:mm"];
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;

        return DateTime.Parse(value, CultureInfo.InvariantCulture);
    }

    private sealed record RawRow(int RowNumber, Dictionary<string, string> Cells);

    private sealed record ReadData(List<string> Headers, List<RawRow> Rows);
}
