using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account_subkonto")]
public partial class ChartAccountSubkonto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("subkonto_type_id")]
    public short SubkontoTypeId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("ChartAccountSubkontos")]
    public virtual ChartAccount Account { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("ChartAccountSubkontos")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty("ChartAccountSubkontos")]
    public virtual SubkontoType SubkontoType { get; set; } = null!;
}
