using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rtl_payment_method")]
public partial class PaymentMethod
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(30)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [InverseProperty(nameof(PaymentMethodTranslation.PaymentMethod))]
    public virtual ICollection<PaymentMethodTranslation> PaymentMethodTranslations { get; set; } = new List<PaymentMethodTranslation>();

    [InverseProperty(nameof(RetailSaleDocPayment.PaymentMethod))]
    public virtual ICollection<RetailSaleDocPayment> RetailSaleDocPayments { get; set; } = new List<RetailSaleDocPayment>();
}
