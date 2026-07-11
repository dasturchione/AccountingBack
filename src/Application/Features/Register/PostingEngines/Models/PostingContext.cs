namespace Application.Features.Register.PostingEngines
{
    /// <summary>
    /// Полная модель "со всем, что нужно" для формирования проводок:
    /// готовые строки ДТ/КТ, метаданные документа и кандидаты субконто.
    /// </summary>
    public class PostingContext
    {
        public int OrganizationId { get; set; }
        public short DocumentTypeId { get; set; }
        public long DocumentId { get; set; }
        public short CurrencyId { get; set; }
        public DateTime DocDate { get; set; }
        public string? JournalNumber { get; set; }
        public long? SourceLineId { get; set; }

        /// <summary>Учётная политика организации (acc_accounting_policy.id) — НСБУ, IFRS и т.д.</summary>
        public short AccountingPolicyId { get; set; }

        public int? FixedAssetId { get; set; }

        public List<PostingEntryContext> Entries { get; set; } = new();

        // ---- Субконто, которые нужно прикрепить к проводкам этого документа ----
        public List<SubkontoValue> Subkontos { get; set; } = new();
    }
}
