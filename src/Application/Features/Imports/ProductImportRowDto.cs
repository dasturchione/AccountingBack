namespace Application.Features.Imports;

/// <summary>
/// Import mexanizmini ko'rsatish uchun NAMUNA DTO (Product import).
/// Ustun nomlari property nomiga (yoki ExcelImportOptions.ColumnMapping'ga) qarab moslanadi.
/// </summary>
public sealed class ProductImportRowDto
{
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public int? ProductGroupId { get; set; }
}
