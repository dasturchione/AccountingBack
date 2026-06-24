using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pur_service")]
[Index("ServiceTypeId", Name = "idx_pur_service_service_type_id")]
public partial class PurchaseService
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
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [ForeignKey("ServiceTypeId")]
    [InverseProperty("PurchaseServices")]
    public virtual PurchaseServiceType ServiceType { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("PurchaseServices")]
    public virtual State State { get; set; } = null!;
}
