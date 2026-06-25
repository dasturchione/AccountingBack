namespace Application.Features.Register.PostingEngine
{
    /// <summary>
    /// Одно значение субконто, готовое к записи в RegisterEntrySubkonto.
    /// "Side" не указываем здесь явно — он определяется автоматически:
    /// PostingService применяет один и тот же набор субконто к обеим сторонам
    /// проводки, если не указано иное через AppliesTo.
    /// </summary>
    public class SubkontoValue
    {
        /// <summary>Код типа субконто, например "Product", "Counterparty", "Contract".
        /// Должен соответствовать коду в справочнике subkonto_type.</summary>
        public short SubkontoTypeId { get; set; } 

        /// <summary>Ссылка на конкретную сущность (id товара, id контрагента и т.д.)</summary>
        public long? EntityId { get; set; }

        /// <summary>Человекочитаемое значение на случай, если EntityId не применим
        /// (например, текстовое примечание вместо ссылки на сущность).</summary>
        public string? DisplayValue { get; set; }

        /// <summary>Порядок субконто в рамках одной стороны проводки (1, 2, 3...).</summary>
        public int SortOrder { get; set; }
    }
}
