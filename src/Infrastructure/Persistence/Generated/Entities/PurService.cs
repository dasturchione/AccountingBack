using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pur_service")]
[Index("ServiceTypeId", Name = "idx_pur_service_service_type_id")]
public partial class PurService
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("service_type_id")]
    public int ServiceTypeId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Service")]
    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    [ForeignKey("ServiceTypeId")]
    [InverseProperty("PurServices")]
    public virtual CmnPurServiceType ServiceType { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("PurServices")]
    public virtual CmnState State { get; set; } = null!;
}
