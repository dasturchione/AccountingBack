namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public short RegisterTypeId { get; set; }
    public string RegisterTypeCode { get; set; } = null!;
    public string RegisterTypeName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ExternalRegisterId { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? FiscalModuleNumber { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
