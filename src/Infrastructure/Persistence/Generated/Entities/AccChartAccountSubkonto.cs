using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_chart_account_subkonto")]
[Index("AccountId", Name = "idx_acc_chart_account_subkonto_account_id")]
[Index("SubkontoTypeId", Name = "idx_acc_chart_account_subkonto_type_id")]
public partial class AccChartAccountSubkonto
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
    [InverseProperty("AccChartAccountSubkontos")]
    public virtual AccChartAccount Account { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("AccChartAccountSubkontos")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty("AccChartAccountSubkontos")]
    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
