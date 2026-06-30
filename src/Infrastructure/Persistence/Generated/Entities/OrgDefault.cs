using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_defaults")]
[Index("BankAccountId", Name = "idx_org_defaults_bank_account_id")]
[Index("BranchId", Name = "idx_org_defaults_branch_id")]
[Index("CashBoxId", Name = "idx_org_defaults_cash_box_id")]
[Index("WarehouseId", Name = "idx_org_defaults_warehouse_id")]
[Index("OrganizationId", Name = "org_defaults_organization_id_key", IsUnique = true)]
public partial class OrgDefault
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }

    [Column("warehouse_id")]
    public int? WarehouseId { get; set; }

    [Column("cash_box_id")]
    public int? CashBoxId { get; set; }

    [Column("bank_account_id")]
    public int? BankAccountId { get; set; }

    [Column("receivable_account_id")]
    public int? ReceivableAccountId { get; set; }

    [Column("payable_account_id")]
    public int? PayableAccountId { get; set; }

    [Column("inventory_account_id")]
    public int? InventoryAccountId { get; set; }

    [Column("cash_account_id")]
    public int? CashAccountId { get; set; }

    [Column("bank_accounting_account_id")]
    public int? BankAccountingAccountId { get; set; }

    [Column("revenue_account_id")]
    public int? RevenueAccountId { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("cogs_account_id")]
    public int? CogsAccountId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BankAccountId")]
    [InverseProperty("OrgDefaults")]
    public virtual OrgBankAccount? BankAccount { get; set; }

    [ForeignKey("BankAccountingAccountId")]
    [InverseProperty("OrgDefaultBankAccountingAccounts")]
    public virtual AccChartAccount? BankAccountingAccount { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("OrgDefaults")]
    public virtual OrgBranch? Branch { get; set; }

    [ForeignKey("CashAccountId")]
    [InverseProperty("OrgDefaultCashAccounts")]
    public virtual AccChartAccount? CashAccount { get; set; }

    [ForeignKey("CashBoxId")]
    [InverseProperty("OrgDefaults")]
    public virtual CashBox? CashBox { get; set; }

    [ForeignKey("CogsAccountId")]
    [InverseProperty("OrgDefaultCogsAccounts")]
    public virtual AccChartAccount? CogsAccount { get; set; }

    [ForeignKey("ExpenseAccountId")]
    [InverseProperty("OrgDefaultExpenseAccounts")]
    public virtual AccChartAccount? ExpenseAccount { get; set; }

    [ForeignKey("InventoryAccountId")]
    [InverseProperty("OrgDefaultInventoryAccounts")]
    public virtual AccChartAccount? InventoryAccount { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrgDefault")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PayableAccountId")]
    [InverseProperty("OrgDefaultPayableAccounts")]
    public virtual AccChartAccount? PayableAccount { get; set; }

    [ForeignKey("ReceivableAccountId")]
    [InverseProperty("OrgDefaultReceivableAccounts")]
    public virtual AccChartAccount? ReceivableAccount { get; set; }

    [ForeignKey("RevenueAccountId")]
    [InverseProperty("OrgDefaultRevenueAccounts")]
    public virtual AccChartAccount? RevenueAccount { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty("OrgDefaults")]
    public virtual InvWarehouse? Warehouse { get; set; }
}
