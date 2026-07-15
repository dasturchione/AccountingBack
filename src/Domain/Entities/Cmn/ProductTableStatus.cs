using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("cmn_product_table_status")]
[Index("Code", Name = "cmn_product_table_status_code_key", IsUnique = true)]
public partial class ProductTableStatus
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

    [InverseProperty(nameof(WarehouseProductTable.Status))]
    public virtual ICollection<WarehouseProductTable> WarehouseProductTables { get; set; } = new List<WarehouseProductTable>();

    [ForeignKey("StateId")]
    [InverseProperty("ProductTableStatuses")]
    public virtual State State { get; set; } = null!;
}
