using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_account_resolve_rule")]
public partial class AccAccountResolveRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("policy_id")]
    public short PolicyId { get; set; }

    [Column("alias")]
    [StringLength(250)]
    public string Alias { get; set; } = null!;

    [Column("dimension_key")]
    [StringLength(250)]
    public string DimensionKey { get; set; } = null!;

    [Column("dimension_value")]
    [StringLength(250)]
    public string DimensionValue { get; set; } = null!;

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("AccAccountResolveRules")]
    public virtual AccChartAccount Account { get; set; } = null!;

    [ForeignKey("PolicyId")]
    [InverseProperty("AccAccountResolveRules")]
    public virtual AccAccountingPolicy Policy { get; set; } = null!;
}
