using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("rtl_payment_method")]
[Index("Code", Name = "rtl_payment_method_code_key", IsUnique = true)]
public partial class RtlPaymentMethod
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

    [InverseProperty("PaymentMethod")]
    public virtual ICollection<RtlPaymentMethodTranslation> RtlPaymentMethodTranslations { get; set; } = new List<RtlPaymentMethodTranslation>();
}
