namespace Application.Features.Register.PostingEngine
{
    /// <summary>
    /// Полная модель "со всем, что нужно" для формирования проводки: суммы для резолва
    /// AmountSource, измерения для резолва AccountAlias (категория, вид услуги и т.д.),
    /// и субконто, которые должны попасть в RegisterEntrySubkonto.
    ///
    /// Один экземпляр строится под один документ через IPostingContextBuilder.
    /// </summary>
    public class PostingContext
    {
        // ---- Идентификация документа (попадает напрямую в acc_reg_entry) ----
        public int OrganizationId { get; set; }
        public short DocumentTypeId { get; set; }
        public long DocumentId { get; set; }
        public short CurrencyId { get; set; }
        public DateTime DocDate { get; set; }
        public string? JournalNumber { get; set; }

        /// <summary>Учётная политика организации (acc_accounting_policy.id) — НСБУ, IFRS и т.д.</summary>
        public short AccountingPolicyId { get; set; }

        // ---- Измерения для резолва AccountAlias ----
        public string? ProductCategory { get; set; }
        public string? ServiceType { get; set; }
        public string? PaymentMethod { get; set; }
        public string? AssetType { get; set; }
        public int? FixedAssetId { get; set; }

        // ---- Суммы для AmountSource ----
        public Dictionary<string, decimal> Amounts { get; set; } = new();

        /// <summary>Количество по дебету/кредиту, если у операции есть количественный учёт (товар).</summary>
        public decimal? DebitQuantity { get; set; }
        public decimal? CreditQuantity { get; set; }

        // ---- Субконто, которые нужно прикрепить к проводкам этого документа ----
        public List<SubkontoValue> Subkontos { get; set; } = new();

        public Dictionary<string, string> Extra { get; set; } = new();
    }
}
