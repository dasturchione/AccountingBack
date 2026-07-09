using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_subkonto_type")]
[Index("Code", Name = "idx_acc_subkonto_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_acc_subkonto_type_state_id")]
public partial class SubkontoType
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

    [Column("source_table")]
    [StringLength(100)]
    public string SourceTable { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("SubkontoType")]
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();

    [InverseProperty("SubkontoType")]
    public virtual ICollection<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; } = new List<RegisterEntrySubkonto>();

    [InverseProperty(nameof(ChartAccountPresetAccountSubkonto.SubkontoType))]
    public virtual ICollection<ChartAccountPresetAccountSubkonto> ChartAccountPresetAccountSubkontos { get; set; } = new List<ChartAccountPresetAccountSubkonto>();

    [ForeignKey("StateId")]
    [InverseProperty("SubkontoTypes")]
    public virtual State State { get; set; } = null!;
}
