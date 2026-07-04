namespace Application.Features.Reports.Exports;

/// <summary>
/// Describes a single export column.
/// </summary>
public sealed class ReportExportColumn
{
    public string Header { get; init; } = null!;
    public Func<object, object?> ValueFactory { get; init; } = _ => null;
}

/// <summary>
/// Represents an exported file.
/// </summary>
public sealed class ReportExportResult
{
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public byte[] Content { get; init; } = [];
}
