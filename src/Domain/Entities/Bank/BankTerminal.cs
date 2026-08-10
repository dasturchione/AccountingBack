using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("bank_terminal")]
public partial class BankTerminal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("bank_account_id")]
    public int? BankAccountId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("merchant_id")]
    [StringLength(100)]
    public string? MerchantId { get; set; }

    [Column("external_terminal_id")]
    [StringLength(100)]
    public string? ExternalTerminalId { get; set; }

    [Column("serial_number")]
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(BankAccountId))]
    [InverseProperty(nameof(BankAccount.BankTerminals))]
    public virtual BankAccount? BankAccount { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.BankTerminals))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.BankTerminals))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(RetailSaleDocPayment.BankTerminal))]
    public virtual ICollection<RetailSaleDocPayment> RetailSaleDocPayments { get; set; } = null!;
}
