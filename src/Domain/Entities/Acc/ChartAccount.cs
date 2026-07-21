using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account")]
public partial class ChartAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    [Column("number")]
    [StringLength(50)]
    public string Number { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [Column("account_type_id")]
    public short? AccountTypeId { get; set; }

    [Column("is_quantity")]
    public bool IsQuantity { get; set; }

    [Column("is_currency")]
    public bool IsCurrency { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("is_department")]
    public bool IsDepartment { get; set; }

    [Column("is_tax_accounting")]
    public bool IsTaxAccounting { get; set; }

    [Column("is_off_balance")]
    public bool IsOffBalance { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();

    [InverseProperty(nameof(DocumentAccountSetting.ChartAccount))]
    public virtual ICollection<DocumentAccountSetting> DocumentAccountSettings { get; set; } = new List<DocumentAccountSetting>();

    [InverseProperty(nameof(OpeningBalanceAccount.ChartAccount))]
    public virtual ICollection<OpeningBalanceAccount> OpeningBalanceAccounts { get; set; } = new List<OpeningBalanceAccount>();

    [InverseProperty("CreditAccount")]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryCreditAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("ChartAccounts")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("DebitAccount")]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryDebitAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [ForeignKey("AccountTypeId")]
    [InverseProperty("ChartAccounts")]
    public virtual AccountType? AccountType { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<ChartAccount> InverseParent { get; set; } = new List<ChartAccount>();

    [InverseProperty(nameof(SaleDoc.CustomerAccount))]
    public virtual ICollection<SaleDoc> SaleDocCustomerAccounts { get; set; } = new List<SaleDoc>();

    [InverseProperty(nameof(SaleDoc.VatAccount))]
    public virtual ICollection<SaleDoc> SaleDocVatAccounts { get; set; } = new List<SaleDoc>();

    [InverseProperty(nameof(SaleDocProduct.CostAccount))]
    public virtual ICollection<SaleDocProduct> SaleDocProductCostAccounts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty(nameof(SaleDocProduct.IncomeAccount))]
    public virtual ICollection<SaleDocProduct> SaleDocProductIncomeAccounts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty(nameof(SaleDocProduct.InventoryAccount))]
    public virtual ICollection<SaleDocProduct> SaleDocProductInventoryAccounts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty(nameof(PurchaseDoc.SupplierAccount))]
    public virtual ICollection<PurchaseDoc> PurchaseDocSupplierAccounts { get; set; } = new List<PurchaseDoc>();

    [InverseProperty(nameof(PurchaseDocProduct.DebitAccount))]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProductDebitAccounts { get; set; } = new List<PurchaseDocProduct>();

    [InverseProperty(nameof(PurchaseDocProduct.VatAccount))]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProductVatAccounts { get; set; } = new List<PurchaseDocProduct>();

    [InverseProperty(nameof(BankOperation.OffsetAccount))]
    public virtual ICollection<BankOperation> BankOperationOffsetAccounts { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(BankOperation.BankChartAccount))]
    public virtual ICollection<BankOperation> BankOperationBankChartAccounts { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(CashOperation.CashChartAccount))]
    public virtual ICollection<CashOperation> CashOperationCashChartAccounts { get; set; } = new List<CashOperation>();

    [InverseProperty(nameof(CashOperation.OffsetAccount))]
    public virtual ICollection<CashOperation> CashOperationOffsetAccounts { get; set; } = new List<CashOperation>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual ChartAccount? Parent { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("ChartAccounts")]
    public virtual State State { get; set; } = null!;
}
