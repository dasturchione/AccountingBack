using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_currency")]
[Index("Code", Name = "idx_cmn_currency_code", IsUnique = true)]
public partial class CmnCurrency
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("symbol")]
    [StringLength(10)]
    public string? Symbol { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("Currency")]
    public virtual ICollection<AccOpeningBalanceAccountDetail> AccOpeningBalanceAccountDetails { get; set; } = new List<AccOpeningBalanceAccountDetail>();

    [InverseProperty("Currency")]
    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    [InverseProperty("Currency")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("Currency")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("Currency")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("BaseCurrency")]
    public virtual ICollection<CmnCurrencyRate> CmnCurrencyRateBaseCurrencies { get; set; } = new List<CmnCurrencyRate>();

    [InverseProperty("TargetCurrency")]
    public virtual ICollection<CmnCurrencyRate> CmnCurrencyRateTargetCurrencies { get; set; } = new List<CmnCurrencyRate>();

    [InverseProperty("BaseCurrency")]
    public virtual ICollection<CmnCurrencyRevaluationLine> CmnCurrencyRevaluationLineBaseCurrencies { get; set; } = new List<CmnCurrencyRevaluationLine>();

    [InverseProperty("TargetCurrency")]
    public virtual ICollection<CmnCurrencyRevaluationLine> CmnCurrencyRevaluationLineTargetCurrencies { get; set; } = new List<CmnCurrencyRevaluationLine>();

    [InverseProperty("Currency")]
    public virtual ICollection<CmnCurrencyTranslation> CmnCurrencyTranslations { get; set; } = new List<CmnCurrencyTranslation>();

    [InverseProperty("Currency")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("Currency")]
    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    [InverseProperty("SelectedCurrency")]
    public virtual ICollection<EdoImportCandidate> EdoImportCandidates { get; set; } = new List<EdoImportCandidate>();

    [InverseProperty("Currency")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("Currency")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("Currency")]
    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    [InverseProperty("Currency")]
    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    [InverseProperty("Currency")]
    public virtual ICollection<PayEmployment> PayEmployments { get; set; } = new List<PayEmployment>();

    [InverseProperty("Currency")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatches { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("Currency")]
    public virtual ICollection<PayPayrollDoc> PayPayrollDocs { get; set; } = new List<PayPayrollDoc>();

    [InverseProperty("Currency")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [InverseProperty("Currency")]
    public virtual ICollection<RtlSaleDoc> RtlSaleDocs { get; set; } = new List<RtlSaleDoc>();

    [InverseProperty("Currency")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnCurrencies")]
    public virtual CmnState State { get; set; } = null!;
}
