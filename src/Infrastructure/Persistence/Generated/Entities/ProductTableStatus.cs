using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

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

    [InverseProperty("Status")]
    public virtual ICollection<ProductTable> ProductTables { get; set; } = new List<ProductTable>();

    [ForeignKey("StateId")]
    [InverseProperty("ProductTableStatuses")]
    public virtual State State { get; set; } = null!;
}
