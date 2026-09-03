namespace Application.Features.Dashboard.DTOs;

public sealed class OverviewFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<short> CurrencyIds { get; set; } = [];
}

public sealed class CashFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<short> CurrencyIds { get; set; } = [];
}

public sealed class ReceivablesPayablesFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<short> CurrencyIds { get; set; } = [];
}

public sealed class ElectronicDocumentsFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<string> DocumentTypes { get; set; } = [];
    public List<short> StatusIds { get; set; } = [];

    public bool IncludesDocumentType(string value) =>
        DocumentTypes.Count == 0 ||
        DocumentTypes.Any(x => string.Equals(x?.Trim(), value, StringComparison.OrdinalIgnoreCase));
}

public sealed class TaxSummaryFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<short> CurrencyIds { get; set; } = [];
    public List<string> DocumentTypes { get; set; } = [];

    public bool IncludesDocumentType(string value) =>
        DocumentTypes.Count == 0 ||
        DocumentTypes.Any(x => string.Equals(x?.Trim(), value, StringComparison.OrdinalIgnoreCase));
}

public sealed class TaskCalendarFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
