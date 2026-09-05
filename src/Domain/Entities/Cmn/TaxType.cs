using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_tax_type")]
[Index("Code", Name = "idx_cmn_tax_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_tax_type_state_id")]
public partial class TaxType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("TaxTypes")]
    public virtual State State { get; set; } = null!;
}
