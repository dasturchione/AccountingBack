using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_didox_origin")]
public partial class CmnDidoxOrigin
{
    [Key]
    [Column("code")]
    public short Code { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("StateId")]
    public virtual CmnState State { get; set; } = null!;
}
