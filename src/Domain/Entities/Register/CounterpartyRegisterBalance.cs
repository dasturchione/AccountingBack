namespace Domain.Entities;

public partial class CounterpartyRegisterBalance
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int CounterpartyId { get; set; }
    public short OperationTypeId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual CounterpartyCard Counterparty { get; set; } = null!;
    public virtual Currency Currency { get; set; } = null!;
    public virtual DocumentType DocumentType { get; set; } = null!;
    public virtual OperationType OperationType { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
}
