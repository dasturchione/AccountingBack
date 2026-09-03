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

    [InverseProperty(nameof(ChartAccountSubkonto.Account))]
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();

    [InverseProperty(nameof(RetailSaleDoc.VatAccount))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocVatAccounts { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(RetailSaleDoc.ReceivableAccount))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocReceivableAccounts { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(RetailSaleDocPayment.DebitAccount))]
    public virtual ICollection<RetailSaleDocPayment> RetailSaleDocPayments { get; set; } = new List<RetailSaleDocPayment>();

    [InverseProperty(nameof(RetailSaleDocProduct.CostAccount))]
    public virtual ICollection<RetailSaleDocProduct> RetailSaleDocProductCostAccounts { get; set; } = new List<RetailSaleDocProduct>();

    [InverseProperty(nameof(RetailSaleDocProduct.IncomeAccount))]
    public virtual ICollection<RetailSaleDocProduct> RetailSaleDocProductIncomeAccounts { get; set; } = new List<RetailSaleDocProduct>();

    [InverseProperty(nameof(RetailSaleDocProduct.InventoryAccount))]
    public virtual ICollection<RetailSaleDocProduct> RetailSaleDocProductInventoryAccounts { get; set; } = new List<RetailSaleDocProduct>();

    [InverseProperty(nameof(DocumentAccountSetting.ChartAccount))]
    public virtual ICollection<DocumentAccountSetting> DocumentAccountSettings { get; set; } = new List<DocumentAccountSetting>();

    [InverseProperty(nameof(OrganizationRegulatedObligationSetting.ChartAccount))]
    public virtual ICollection<OrganizationRegulatedObligationSetting> RegulatedObligationSettings { get; set; } = [];

    [InverseProperty(nameof(OpeningInventoryProduct.DebitAccount))]
    public virtual ICollection<OpeningInventoryProduct> OpeningInventoryProducts { get; set; } = new List<OpeningInventoryProduct>();

    [InverseProperty(nameof(OpeningBalanceAccount.ChartAccount))]
    public virtual ICollection<OpeningBalanceAccount> OpeningBalanceAccounts { get; set; } = new List<OpeningBalanceAccount>();

    [InverseProperty(nameof(AccountingRegisterEntry.CreditAccount))]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryCreditAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.ChartAccounts))]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty(nameof(AccountingRegisterEntry.DebitAccount))]
    public virtual ICollection<AccountingRegisterEntry> RegisterEntryDebitAccounts { get; set; } = new List<AccountingRegisterEntry>();

    [ForeignKey(nameof(AccountTypeId))]
    [InverseProperty(nameof(AccountType.ChartAccounts))]
    public virtual AccountType? AccountType { get; set; }

    [InverseProperty(nameof(ChartAccount.Parent))]
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

    [InverseProperty(nameof(FaAssetAccounting.AccumulatedDepreciationAccount))]
    public virtual ICollection<FaAssetAccounting> FaAssetAccountingAccumulatedDepreciationAccounts { get; set; } = new List<FaAssetAccounting>();

    [InverseProperty(nameof(FaAssetAccounting.AssetAccount))]
    public virtual ICollection<FaAssetAccounting> FaAssetAccountingAssetAccounts { get; set; } = new List<FaAssetAccounting>();

    [InverseProperty(nameof(FaAssetAccounting.DepreciationExpenseAccount))]
    public virtual ICollection<FaAssetAccounting> FaAssetAccountingDepreciationExpenseAccounts { get; set; } = new List<FaAssetAccounting>();

    [InverseProperty(nameof(FaCommissioningDocLine.AccumulatedDepreciationAccount))]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLineAccumulatedDepreciationAccounts { get; set; } = new List<FaCommissioningDocLine>();

    [InverseProperty(nameof(FaCommissioningDocLine.CapitalInvestmentAccount))]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLineCapitalInvestmentAccounts { get; set; } = new List<FaCommissioningDocLine>();

    [InverseProperty(nameof(FaCommissioningDocLine.DepreciationExpenseAccount))]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLineDepreciationExpenseAccounts { get; set; } = new List<FaCommissioningDocLine>();

    [InverseProperty(nameof(FaDepreciationRunLine.ExpenseAccount))]
    public virtual ICollection<FaDepreciationRunLine> FaDepreciationRunLineExpenseAccounts { get; set; } = new List<FaDepreciationRunLine>();

    [InverseProperty(nameof(FaDepreciationRunLine.AccumulatedDepreciationAccount))]
    public virtual ICollection<FaDepreciationRunLine> FaDepreciationRunLineAccumulatedDepreciationAccounts { get; set; } = new List<FaDepreciationRunLine>();

    [InverseProperty(nameof(FaDisposalDoc.DisposalAccount))]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocDisposalAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty(nameof(FaDisposalDoc.CustomerAccount))]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocCustomerAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty(nameof(FaDisposalDoc.VatAccount))]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocVatAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty(nameof(FaDisposalDoc.GainAccount))]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocGainAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty(nameof(FaDisposalDoc.LossAccount))]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocLossAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty(nameof(FaDisposalDocLine.AssetAccount))]
    public virtual ICollection<FaDisposalDocLine> FaDisposalDocLineAssetAccounts { get; set; } = new List<FaDisposalDocLine>();

    [InverseProperty(nameof(FaDisposalDocLine.AccumulatedDepreciationAccount))]
    public virtual ICollection<FaDisposalDocLine> FaDisposalDocLineAccumulatedDepreciationAccounts { get; set; } = new List<FaDisposalDocLine>();

    [InverseProperty(nameof(FaReceiptDoc.SupplierAccount))]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty(nameof(FaReceiptDocAsset.AssetAccount))]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [InverseProperty(nameof(FaReceiptDocLine.CapitalInvestmentAccount))]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLineCapitalInvestmentAccounts { get; set; } = new List<FaReceiptDocLine>();

    [InverseProperty(nameof(FaReceiptDocLine.VatAccount))]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLineVatAccounts { get; set; } = new List<FaReceiptDocLine>();

    [InverseProperty(nameof(FaRevaluationDoc.RevaluationReserveAccount))]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocRevaluationReserveAccounts { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty(nameof(FaRevaluationDoc.RevaluationLossAccount))]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocRevaluationLossAccounts { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty(nameof(FaRevaluationDocLine.AssetAccount))]
    public virtual ICollection<FaRevaluationDocLine> FaRevaluationDocLineAssetAccounts { get; set; } = new List<FaRevaluationDocLine>();

    [InverseProperty(nameof(FaRevaluationDocLine.AccumulatedDepreciationAccount))]
    public virtual ICollection<FaRevaluationDocLine> FaRevaluationDocLineAccumulatedDepreciationAccounts { get; set; } = new List<FaRevaluationDocLine>();

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

    [ForeignKey(nameof(ParentId))]
    [InverseProperty(nameof(ChartAccount.InverseParent))]
    public virtual ChartAccount? Parent { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.ChartAccounts))]
    public virtual State State { get; set; } = null!;
}
