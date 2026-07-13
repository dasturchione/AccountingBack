namespace Application.Features.Register.PostingEngines
{
    /// <summary>
    /// Одно значение субконто-кандидата. PostingService выберет только те типы,
    /// которые указаны в ChartAccount.ChartAccountSubkontos конкретного счёта.
    /// </summary>
    public class SubkontoValue
    {
        public short SubkontoTypeId { get; set; }
        public long? EntityId { get; set; }
        public string? DisplayValue { get; set; }
        public int SortOrder { get; set; }

        /// <summary>
        /// Если заполнено, субконто применяется только к указанному chart_account.id.
        /// Нужно для операций, где один тип субконто встречается несколько раз,
        /// например касса-источник и касса-получатель.
        /// </summary>
        public int? AppliesToAccountId { get; set; }
    }
}
