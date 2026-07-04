using SharedKernel.Results;

namespace Application.Abstractions.Import;

public sealed class ExcelImportOptions
{
    /// <summary>Sarlavha (header) qatori — 1-based. Default 1.</summary>
    public int HeaderRow { get; init; } = 1;

    /// <summary>Ma'lumot boshlanadigan qator — 1-based. Default 2.</summary>
    public int DataStartRow { get; init; } = 2;

    /// <summary>Varaq nomi (null bo'lsa birinchi varaq). CSV uchun e'tiborsiz.</summary>
    public string? SheetName { get; init; }

    /// <summary>Excel ustun nomi → obyekt property nomi. Null bo'lsa property nomiga qarab moslanadi.</summary>
    public Dictionary<string, string>? ColumnMapping { get; init; }

    /// <summary>Butunlay bo'sh qatorlarni o'tkazib yuborish. Default true.</summary>
    public bool SkipEmptyRows { get; init; } = true;
}

public sealed class ExcelRowError
{
    public int RowNumber { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ExcelImportResult<T>
{
    public List<T> Items { get; init; } = [];
    public List<ExcelRowError> Errors { get; init; } = [];
    public int TotalDataRows { get; init; }
    public int SuccessCount => Items.Count;
    public int ErrorCount => Errors.Count;
}

public interface IExcelImporter
{
    /// <summary>Excel/CSV streamdan tipli obyektlar ro'yxati + qator-darajasidagi xatolar.</summary>
    Task<Result<ExcelImportResult<T>>> ImportAsync<T>(
        Stream fileStream,
        ExcelImportOptions options,
        CancellationToken ct = default) where T : new();

    /// <summary>Xom qatorlar (ustun nomi → qiymat). Tiplashsiz oddiy variant.</summary>
    Task<Result<List<Dictionary<string, string>>>> ImportRawAsync(
        Stream fileStream,
        ExcelImportOptions options,
        CancellationToken ct = default);
}
