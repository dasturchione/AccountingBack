namespace Application.Features.SaleDocs;

public class SaleDocListDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public int? CustomerAccountId { get; set; }
    public string? CustomerAccountNumber { get; set; }
    public string? CustomerAccountName { get; set; }
    public int? VatAccountId { get; set; }
    public string? VatAccountNumber { get; set; }
    public string? VatAccountName { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public long? ContractId { get; set; }
    public string? ContractNumber { get; set; }
}
