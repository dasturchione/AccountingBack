using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_currency")]
public partial class Currency
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

    [InverseProperty(nameof(OpeningBalanceAccountDetail.Currency))]
    public virtual ICollection<OpeningBalanceAccountDetail> OpeningBalanceAccountDetails { get; set; } = new List<OpeningBalanceAccountDetail>();

    [InverseProperty(nameof(CurrencyTranslation.Currency))]
    public virtual ICollection<CurrencyTranslation> CurrencyTranslations { get; set; } = new List<CurrencyTranslation>();

    [InverseProperty("Currency")]
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();

    [InverseProperty("Currency")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("Currency")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("Currency")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("Currency")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("Currency")]
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();

    [InverseProperty("Currency")]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty("Currency")]
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();

    [InverseProperty("Currency")]
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    [InverseProperty("Currency")]
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty("Currency")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty(nameof(CurrencyRate.BaseCurrency))]
    public virtual ICollection<CurrencyRate> BaseCurrencyRates { get; set; } = new List<CurrencyRate>();

    [InverseProperty(nameof(CurrencyRate.TargetCurrency))]
    public virtual ICollection<CurrencyRate> TargetCurrencyRates { get; set; } = new List<CurrencyRate>();

    [InverseProperty(nameof(CurrencyRevaluationLine.BaseCurrency))]
    public virtual ICollection<CurrencyRevaluationLine> CurrencyRevaluationBaseLines { get; set; } = new List<CurrencyRevaluationLine>();

    [InverseProperty(nameof(CurrencyRevaluationLine.TargetCurrency))]
    public virtual ICollection<CurrencyRevaluationLine> CurrencyRevaluationTargetLines { get; set; } = new List<CurrencyRevaluationLine>();

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.Currencies))]
    public virtual State State { get; set; } = null!;
}
