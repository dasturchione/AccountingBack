namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterBaseDto
{
    public int? WarehouseId { get; set; }
    public short RegisterTypeId { get; set; }
    public string Name { get; set; } = null!;
    public string? ExternalRegisterId { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? FiscalModuleNumber { get; set; }
}
