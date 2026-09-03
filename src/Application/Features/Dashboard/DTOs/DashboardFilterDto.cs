namespace Application.Features.Dashboard.DTOs;

public sealed class DashboardFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<short> CurrencyIds { get; set; } = [];
    public List<int> WarehouseIds { get; set; } = [];
    public List<string> DocumentTypes { get; set; } = [];
    public List<short> StatusIds { get; set; } = [];

    public bool HasCurrencyFilter => CurrencyIds.Count > 0;
    public bool HasWarehouseFilter => WarehouseIds.Count > 0;
    public bool HasDocumentTypeFilter => DocumentTypes.Count > 0;
    public bool HasStatusFilter => StatusIds.Count > 0;

    public bool IncludesDocumentType(string value) =>
        !HasDocumentTypeFilter || DocumentTypes.Any(x => string.Equals(x?.Trim(), value, StringComparison.OrdinalIgnoreCase));
}
