using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_accounting_policy")]
[Index("Code", Name = "acc_accounting_policy_code_key", IsUnique = true)]
public partial class AccAccountingPolicy
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }
    [ForeignKey("StateId")]
    [InverseProperty("AccAccountingPolicies")]
    public virtual CmnState State { get; set; } = null!;
}
