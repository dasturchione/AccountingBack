using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("OpeningBalanceAccountDetailId", "SubkontoTypeId")]
[Table("acc_opening_balance_account_detail_subkonto")]
public partial class OpeningBalanceAccountDetailSubkonto
{
    [Key]
    [Column("opening_balance_account_detail_id")]
    public long OpeningBalanceAccountDetailId { get; set; }

    [Key]
    [Column("subkonto_type_id")]
    public short SubkontoTypeId { get; set; }

    [Column("subkonto_id")]
    public long SubkontoId { get; set; }

    [Column("sort_order")]
    public short SortOrder { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OpeningBalanceAccountDetailId")]
    [InverseProperty(nameof(OpeningBalanceAccountDetail.OpeningBalanceAccountDetailSubkontos))]
    public virtual OpeningBalanceAccountDetail OpeningBalanceAccountDetail { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty(nameof(SubkontoType.OpeningBalanceAccountDetailSubkontos))]
    public virtual SubkontoType SubkontoType { get; set; } = null!;
}
