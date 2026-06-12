namespace Domain.Entities;

public partial class CashOperation
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public int CashBoxId { get; set; }
    public short OperationTypeId { get; set; }
    public short? PaymentTypeId { get; set; }
    public int? CounterpartyId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public short StatusId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual CashBox CashBox { get; set; } = null!;
    public virtual CounterpartyCard? Counterparty { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    public virtual OperationType OperationType { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual PaymentType? PaymentType { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual DocumentStatus Status { get; set; } = null!;
}
