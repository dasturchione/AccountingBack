using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_chart_account")]
[Index("AccountTypeId", Name = "idx_acc_chart_account_account_type_id")]
[Index("Number", Name = "idx_acc_chart_account_number")]
[Index("ParentId", Name = "idx_acc_chart_account_parent_id")]
[Index("StateId", Name = "idx_acc_chart_account_state_id")]
[Index("OrganizationId", "Number", Name = "ux_acc_chart_account_organization_number", IsUnique = true)]
public partial class AccChartAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("account_type_id")]
    public short? AccountTypeId { get; set; }

    [Column("is_quantity")]
    public bool IsQuantity { get; set; }

    [Column("is_currency")]
    public bool IsCurrency { get; set; }

    [Column("number")]
    [StringLength(50)]
    public string Number { get; set; } = null!;

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("is_department")]
    public bool IsDepartment { get; set; }

    [Column("is_tax_accounting")]
    public bool IsTaxAccounting { get; set; }

    [Column("is_off_balance")]
    public bool IsOffBalance { get; set; }
    [InverseProperty("Account")]
    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<AccRegEntry> AccRegEntryCreditAccounts { get; set; } = new List<AccRegEntry>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<AccRegEntry> AccRegEntryDebitAccounts { get; set; } = new List<AccRegEntry>();

    [ForeignKey("AccountTypeId")]
    [InverseProperty("AccChartAccounts")]
    public virtual AccAccountType? AccountType { get; set; }

    [InverseProperty("BankChartAccount")]
    public virtual ICollection<BankOperation> BankOperationBankChartAccounts { get; set; } = new List<BankOperation>();

    [InverseProperty("OffsetAccount")]
    public virtual ICollection<BankOperation> BankOperationOffsetAccounts { get; set; } = new List<BankOperation>();

    [InverseProperty("CashChartAccount")]
    public virtual ICollection<CashOperation> CashOperationCashChartAccounts { get; set; } = new List<CashOperation>();

    [InverseProperty("OffsetAccount")]
    public virtual ICollection<CashOperation> CashOperationOffsetAccounts { get; set; } = new List<CashOperation>();

    [InverseProperty("Parent")]
    public virtual ICollection<AccChartAccount> InverseParent { get; set; } = new List<AccChartAccount>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccChartAccounts")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual AccChartAccount? Parent { get; set; }

    [InverseProperty("DebitAccount")]
    public virtual ICollection<PurDocProduct> PurDocProductDebitAccounts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("VatAccount")]
    public virtual ICollection<PurDocProduct> PurDocProductVatAccounts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("SupplierAccount")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [InverseProperty("CustomerAccount")]
    public virtual ICollection<SaleDoc> SaleDocCustomerAccounts { get; set; } = new List<SaleDoc>();

    [InverseProperty("CostAccount")]
    public virtual ICollection<SaleDocProduct> SaleDocProductCostAccounts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty("IncomeAccount")]
    public virtual ICollection<SaleDocProduct> SaleDocProductIncomeAccounts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty("InventoryAccount")]
    public virtual ICollection<SaleDocProduct> SaleDocProductInventoryAccounts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty("VatAccount")]
    public virtual ICollection<SaleDoc> SaleDocVatAccounts { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("AccChartAccounts")]
    public virtual CmnState State { get; set; } = null!;
}
