using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_payment_purpose")]
[Index("Code", Name = "acc_payment_purpose_code_key", IsUnique = true)]
public partial class PaymentPurpose
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("alias_id")]
    public short AliasId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("requires_counterparty")]
    public bool RequiresCounterparty { get; set; }
}
