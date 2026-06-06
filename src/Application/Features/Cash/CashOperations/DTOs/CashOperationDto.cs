namespace Application.Features.CashOperations;

public class CashOperationDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int CashBoxId { get; set; }
    public string CashBoxName { get; set; } = null!;
    public short OperationTypeId { get; set; }
    public string OperationTypeName { get; set; } = null!;
    public short? PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
