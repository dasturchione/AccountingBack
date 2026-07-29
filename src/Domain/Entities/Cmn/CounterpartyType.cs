using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_counterparty_type")]
[Index("Code", Name = "idx_cmn_counterparty_type_code", IsUnique = true)]
public partial class CounterpartyType
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

    [InverseProperty("CounterpartyType")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [ForeignKey("StateId")]
    [InverseProperty("CounterpartyTypes")]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(CounterpartyTypeTranslation.CounterpartyType))]
    public virtual ICollection<CounterpartyTypeTranslation> CounterpartyTypeTranslations { get; set; } = new List<CounterpartyTypeTranslation>();
}
