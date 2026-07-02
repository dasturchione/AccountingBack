using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cash_box")]
[Index("BranchId", Name = "idx_cash_box_branch_id")]
[Index("CurrencyId", Name = "idx_cash_box_currency_id")]
[Index("IsMain", Name = "idx_cash_box_is_main")]
[Index("OrganizationId", "Code", Name = "idx_cash_box_org_code", IsUnique = true)]
[Index("OrganizationId", Name = "idx_cash_box_organization_id")]
[Index("ResponsibleUserId", Name = "idx_cash_box_responsible_user_id")]
[Index("StateId", Name = "idx_cash_box_state_id")]
public partial class CashBox
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("is_main")]
    public bool IsMain { get; set; }

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("opening_balance")]
    [Precision(18, 2)]
    public decimal OpeningBalance { get; set; }

    [Column("opening_balance_date")]
    public DateOnly? OpeningBalanceDate { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("CashBoxes")]
    public virtual OrgBranch? Branch { get; set; }

    [InverseProperty("CashBox")]
    public virtual ICollection<CashOperation> CashOperationCashBoxes { get; set; } = new List<CashOperation>();

    [InverseProperty("DestinationCashBox")]
    public virtual ICollection<CashOperation> CashOperationDestinationCashBoxes { get; set; } = new List<CashOperation>();

    [ForeignKey("CurrencyId")]
    [InverseProperty("CashBoxes")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [InverseProperty("CashBox")]
    public virtual ICollection<OrgDefault> OrgDefaults { get; set; } = new List<OrgDefault>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("CashBoxes")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("CashBoxes")]
    public virtual SysUser? ResponsibleUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("CashBoxes")]
    public virtual CmnState State { get; set; } = null!;
}
