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

    [InverseProperty("ChartAccount")]
    public virtual ICollection<AccDocumentAccountSetting> AccDocumentAccountSettings { get; set; } = new List<AccDocumentAccountSetting>();

    [InverseProperty("ChartAccount")]
    public virtual ICollection<AccOpeningBalanceAccount> AccOpeningBalanceAccounts { get; set; } = new List<AccOpeningBalanceAccount>();

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

    [InverseProperty("AccumulatedDepreciationAccount")]
    public virtual ICollection<FaAsset> FaAssetAccumulatedDepreciationAccounts { get; set; } = new List<FaAsset>();

    [InverseProperty("AssetAccount")]
    public virtual ICollection<FaAsset> FaAssetAssetAccounts { get; set; } = new List<FaAsset>();

    [InverseProperty("DepreciationExpenseAccount")]
    public virtual ICollection<FaAsset> FaAssetDepreciationExpenseAccounts { get; set; } = new List<FaAsset>();

    [InverseProperty("AccumulatedDepreciationAccount")]
    public virtual ICollection<FaDepreciationRunLine> FaDepreciationRunLineAccumulatedDepreciationAccounts { get; set; } = new List<FaDepreciationRunLine>();

    [InverseProperty("ExpenseAccount")]
    public virtual ICollection<FaDepreciationRunLine> FaDepreciationRunLineExpenseAccounts { get; set; } = new List<FaDepreciationRunLine>();

    [InverseProperty("CustomerAccount")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocCustomerAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("DisposalAccount")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocDisposalAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("GainAccount")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocGainAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("AccumulatedDepreciationAccount")]
    public virtual ICollection<FaDisposalDocLine> FaDisposalDocLineAccumulatedDepreciationAccounts { get; set; } = new List<FaDisposalDocLine>();

    [InverseProperty("AssetAccount")]
    public virtual ICollection<FaDisposalDocLine> FaDisposalDocLineAssetAccounts { get; set; } = new List<FaDisposalDocLine>();

    [InverseProperty("LossAccount")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocLossAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("VatAccount")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocVatAccounts { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("AccumulatedDepreciationAccount")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssetAccumulatedDepreciationAccounts { get; set; } = new List<FaReceiptDocAsset>();

    [InverseProperty("AssetAccount")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssetAssetAccounts { get; set; } = new List<FaReceiptDocAsset>();

    [InverseProperty("DepreciationExpenseAccount")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssetDepreciationExpenseAccounts { get; set; } = new List<FaReceiptDocAsset>();

    [InverseProperty("CapitalInvestmentAccount")]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLineCapitalInvestmentAccounts { get; set; } = new List<FaReceiptDocLine>();

    [InverseProperty("VatAccount")]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLineVatAccounts { get; set; } = new List<FaReceiptDocLine>();

    [InverseProperty("SupplierAccount")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("AccumulatedDepreciationAccount")]
    public virtual ICollection<FaRevaluationDocLine> FaRevaluationDocLineAccumulatedDepreciationAccounts { get; set; } = new List<FaRevaluationDocLine>();

    [InverseProperty("AssetAccount")]
    public virtual ICollection<FaRevaluationDocLine> FaRevaluationDocLineAssetAccounts { get; set; } = new List<FaRevaluationDocLine>();

    [InverseProperty("RevaluationLossAccount")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocRevaluationLossAccounts { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("RevaluationReserveAccount")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocRevaluationReserveAccounts { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<InvOpeningInventoryProduct> InvOpeningInventoryProducts { get; set; } = new List<InvOpeningInventoryProduct>();

    [InverseProperty("Parent")]
    public virtual ICollection<AccChartAccount> InverseParent { get; set; } = new List<AccChartAccount>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccChartAccounts")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual AccChartAccount? Parent { get; set; }

    [InverseProperty("ExpenseAccount")]
    public virtual ICollection<PayComponent> PayComponentExpenseAccounts { get; set; } = new List<PayComponent>();

    [InverseProperty("LiabilityAccount")]
    public virtual ICollection<PayComponent> PayComponentLiabilityAccounts { get; set; } = new List<PayComponent>();

    [InverseProperty("ExpenseAccount")]
    public virtual ICollection<PayEmployment> PayEmployments { get; set; } = new List<PayEmployment>();

    [InverseProperty("OffsetAccount")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatchOffsetAccounts { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("SourceChartAccount")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatchSourceChartAccounts { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<PurDocProduct> PurDocProductDebitAccounts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("VatAccount")]
    public virtual ICollection<PurDocProduct> PurDocProductVatAccounts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("SupplierAccount")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [InverseProperty("DebitAccount")]
    public virtual ICollection<RtlSaleDocPayment> RtlSaleDocPayments { get; set; } = new List<RtlSaleDocPayment>();

    [InverseProperty("CostAccount")]
    public virtual ICollection<RtlSaleDocProduct> RtlSaleDocProductCostAccounts { get; set; } = new List<RtlSaleDocProduct>();

    [InverseProperty("IncomeAccount")]
    public virtual ICollection<RtlSaleDocProduct> RtlSaleDocProductIncomeAccounts { get; set; } = new List<RtlSaleDocProduct>();

    [InverseProperty("InventoryAccount")]
    public virtual ICollection<RtlSaleDocProduct> RtlSaleDocProductInventoryAccounts { get; set; } = new List<RtlSaleDocProduct>();

    [InverseProperty("ReceivableAccount")]
    public virtual ICollection<RtlSaleDoc> RtlSaleDocReceivableAccounts { get; set; } = new List<RtlSaleDoc>();

    [InverseProperty("VatAccount")]
    public virtual ICollection<RtlSaleDoc> RtlSaleDocVatAccounts { get; set; } = new List<RtlSaleDoc>();

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
