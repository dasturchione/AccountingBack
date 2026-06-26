using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_purchase_item_type")]
[Index("Code", Name = "idx_cmn_purchase_item_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_purchase_item_type_state_id")]
public partial class PurchaseItemType
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

    [InverseProperty("ItemType")]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProducts { get; set; } = new List<PurchaseDocProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("PurchaseItemTypes")]
    public virtual State State { get; set; } = null!;
}
