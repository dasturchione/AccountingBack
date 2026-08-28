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

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.Currencies))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(OpeningBalanceAccountDetail.Currency))]
    public virtual ICollection<OpeningBalanceAccountDetail> OpeningBalanceAccountDetails { get; set; } = new List<OpeningBalanceAccountDetail>();

    [InverseProperty(nameof(CurrencyTranslation.Currency))]
    public virtual ICollection<CurrencyTranslation> CurrencyTranslations { get; set; } = new List<CurrencyTranslation>();

    [InverseProperty(nameof(DocumentRegistry.Currency))]
    public virtual ICollection<DocumentRegistry> DocumentRegistries { get; set; } = new List<DocumentRegistry>();

    [InverseProperty(nameof(AccountingRegisterEntry.Currency))]
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();

    [InverseProperty(nameof(BankOperation.Currency))]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(PaymentAcceptancePointOperation.Currency))]
    public virtual ICollection<PaymentAcceptancePointOperation> PaymentAcceptancePointOperations { get; set; } = [];

    [InverseProperty(nameof(CashBox.Currency))]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty(nameof(CashOperation.Currency))]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty(nameof(CounterpartyBankAccount.Currency))]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty(nameof(CounterpartyRegisterBalance.Currency))]
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();

    [InverseProperty(nameof(ProductPrice.Currency))]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty(nameof(MoneyRegisterBalance.Currency))]
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();

    [InverseProperty(nameof(BankAccount.Currency))]
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    [InverseProperty(nameof(PurchaseDoc.Currency))]
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty(nameof(SaleDoc.Currency))]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty(nameof(CurrencyRate.BaseCurrency))]
    public virtual ICollection<CurrencyRate> BaseCurrencyRates { get; set; } = new List<CurrencyRate>();

    [InverseProperty(nameof(RetailSaleDoc.Currency))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocs { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(CurrencyRate.TargetCurrency))]
    public virtual ICollection<CurrencyRate> TargetCurrencyRates { get; set; } = new List<CurrencyRate>();

    [InverseProperty(nameof(CurrencyRevaluationLine.BaseCurrency))]
    public virtual ICollection<CurrencyRevaluationLine> CurrencyRevaluationBaseLines { get; set; } = new List<CurrencyRevaluationLine>();

    [InverseProperty(nameof(CurrencyRevaluationLine.TargetCurrency))]
    public virtual ICollection<CurrencyRevaluationLine> CurrencyRevaluationTargetLines { get; set; } = new List<CurrencyRevaluationLine>();
}
