using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_opening_balance")]
[Index("OrganizationId", Name = "acc_opening_balance_organization_id_key", IsUnique = true)]
[Index("OrganizationId", Name = "ix_acc_opening_balance_organization")]
public partial class AccOpeningBalance
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("balance_date")]
    public DateOnly BalanceDate { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("OpeningBalance")]
    public virtual ICollection<AccOpeningBalanceAccount> AccOpeningBalanceAccounts { get; set; } = new List<AccOpeningBalanceAccount>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccOpeningBalance")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("AccOpeningBalances")]
    public virtual CmnState State { get; set; } = null!;
}
