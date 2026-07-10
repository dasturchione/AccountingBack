using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_subkonto_type")]
[Index("Code", Name = "idx_acc_subkonto_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_acc_subkonto_type_state_id")]
public partial class AccSubkontoType
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
    public virtual ICollection<AccChartAccountPresetAccountSubkonto> AccChartAccountPresetAccountSubkontos { get; set; } = new List<AccChartAccountPresetAccountSubkonto>();

    [InverseProperty("SubkontoType")]
    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    [InverseProperty("SubkontoType")]
    public virtual ICollection<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; } = new List<AccRegEntrySubkonto>();

    [InverseProperty("SubkontoType")]
    public virtual ICollection<AccSubkontoTypeTranslation> AccSubkontoTypeTranslations { get; set; } = new List<AccSubkontoTypeTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty("AccSubkontoTypes")]
    public virtual CmnState State { get; set; } = null!;
}
