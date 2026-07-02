using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_defaults")]
[Index("BankAccountId", Name = "idx_org_defaults_bank_account_id")]
[Index("BranchId", Name = "idx_org_defaults_branch_id")]
[Index("CashBoxId", Name = "idx_org_defaults_cash_box_id")]
[Index("WarehouseId", Name = "idx_org_defaults_warehouse_id")]
[Index("OrganizationId", Name = "org_defaults_organization_id_key", IsUnique = true)]
public partial class OrganizationDefault
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
}
