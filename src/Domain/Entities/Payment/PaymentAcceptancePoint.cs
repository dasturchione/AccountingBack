using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_payment_acceptance_point")]
[Index(nameof(OrganizationId), nameof(Code), Name = "ux_org_payment_acceptance_point_organization_code", IsUnique = true)]
public sealed class PaymentAcceptancePoint
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("type_id")]
    public short TypeId { get; set; }

    [Column("bank_account_id")]
    public int? BankAccountId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("merchant_id")]
    [StringLength(150)]
    public string? MerchantId { get; set; }

    [Column("external_id")]
    [StringLength(150)]
    public string? ExternalId { get; set; }

    [Column("serial_number")]
    [StringLength(150)]
    public string? SerialNumber { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.PaymentAcceptancePoints))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(TypeId))]
    [InverseProperty(nameof(PaymentAcceptancePointType.PaymentAcceptancePoints))]
    public PaymentAcceptancePointType Type { get; set; } = null!;

    [ForeignKey(nameof(BankAccountId))]
    [InverseProperty(nameof(BankAccount.PaymentAcceptancePoints))]
    public BankAccount? BankAccount { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.PaymentAcceptancePoints))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(PaymentAcceptancePointOperation.PaymentAcceptancePoint))]
    public ICollection<PaymentAcceptancePointOperation> Operations { get; set; } = [];

    [InverseProperty(nameof(RetailSaleDocPayment.PaymentAcceptancePoint))]
    public ICollection<RetailSaleDocPayment> RetailSaleDocPayments { get; set; } = [];
}
