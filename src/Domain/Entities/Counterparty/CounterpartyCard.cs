namespace Domain.Entities;

public partial class CounterpartyCard
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public short CounterpartyTypeId { get; set; }
    public string ShortName { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Inn { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual CounterpartyType CounterpartyType { get; set; } = null!;
    public virtual Region? Region { get; set; }
    public virtual District? District { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();
}
