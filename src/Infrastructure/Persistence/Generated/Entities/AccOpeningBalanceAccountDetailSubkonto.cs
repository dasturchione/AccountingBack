using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("OpeningBalanceAccountDetailId", "SubkontoTypeId")]
[Table("acc_opening_balance_account_detail_subkonto")]
[Index("OpeningBalanceAccountDetailId", "SortOrder", Name = "acc_opening_balance_account_d_opening_balance_account_detai_key", IsUnique = true)]
[Index("SubkontoTypeId", "SubkontoId", Name = "ix_acc_opening_balance_detail_subkonto")]
public partial class AccOpeningBalanceAccountDetailSubkonto
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
    [InverseProperty("AccOpeningBalanceAccountDetailSubkontos")]
    public virtual AccOpeningBalanceAccountDetail OpeningBalanceAccountDetail { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty("AccOpeningBalanceAccountDetailSubkontos")]
    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
