using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_account_type")]
[Index("Code", Name = "idx_acc_account_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_acc_account_type_state_id")]
public partial class AccAccountType
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

    [InverseProperty("AccountType")]
    public virtual ICollection<AccAccountTypeTranslation> AccAccountTypeTranslations { get; set; } = new List<AccAccountTypeTranslation>();

    [InverseProperty("AccountType")]
    public virtual ICollection<AccChartAccountPresetAccount> AccChartAccountPresetAccounts { get; set; } = new List<AccChartAccountPresetAccount>();

    [InverseProperty("AccountType")]
    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    [ForeignKey("StateId")]
    [InverseProperty("AccAccountTypes")]
    public virtual CmnState State { get; set; } = null!;
}
