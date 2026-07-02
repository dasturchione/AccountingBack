using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_payment_type")]
[Index("Code", Name = "idx_cmn_payment_type_code", IsUnique = true)]
public partial class PaymentType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("PaymentType")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("PaymentType")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [ForeignKey("StateId")]
    [InverseProperty("PaymentTypes")]
    public virtual State State { get; set; } = null!;
}
