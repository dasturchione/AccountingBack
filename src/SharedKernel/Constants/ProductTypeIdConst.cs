namespace SharedKernel.Constants
{
    public class ProductTypeIdConst
    {
        /// <summary>Товар. Счета: 2900, 2910</summary>
        public const short Good = 1;

        /// <summary>Товар в розничной торговле. Счета: 2920, 2920.1, 2920.2, 2920.3</summary>
        public const short GoodRetail = 2;

        /// <summary>Прочий товар. Счета: 2930 (на выставке), 2950 (тара под товаром), 2960/2960.1/2960.2 (отгруженные), 2970/2970.1/2970.2 (в пути), 2990 (прочие)</summary>
        public const short GoodOther = 3;

        /// <summary>Сырьё и материалы. Счёт: 1010</summary>
        public const short MaterialRaw = 4;

        /// <summary>Покупные полуфабрикаты и комплектующие. Счёт: 1020</summary>
        public const short MaterialComponent = 5;

        /// <summary>Запасные части. Счёт: 1040</summary>
        public const short MaterialSparePart = 6;

        /// <summary>Строительные материалы. Счёт: 1050</summary>
        public const short MaterialConstruction = 7;

        /// <summary>Тара и тарные материалы. Счёт: 1060</summary>
        public const short MaterialPackaging = 8;

        /// <summary>Прочие материалы. Счета: 1070 (переданные в переработку), 1090 (прочие)</summary>
        public const short MaterialOther = 9;

        /// <summary>Полуфабрикат собственного производства. Счета: 2100, 2110</summary>
        public const short SemiFinished = 10;

        /// <summary>Готовая продукция. Счета: 2800, 2810, 2820 (на выставке), 2830/2830.1/2830.2 (отгруженная)</summary>
        public const short FinishedGoods = 11;

        /// <summary>Услуга (основная деятельность). Затраты: 2000, 2010. Доход: 9030. Себестоимость: 9130</summary>
        public const short ServiceMain = 12;

        /// <summary>Переработка давальческого сырья. Счёт: 2020</summary>
        public const short ServiceToll = 13;

        /// <summary>Вспомогательная услуга. Счета: 2300, 2310</summary>
        public const short ServiceAuxiliary = 14;

        /// <summary>Обслуживающее производство/хозяйство. Счета: 2700, 2710</summary>
        public const short ServiceMaintenance = 15;

        /// <summary>Услуга проката. Счёт: 2940 (Предметы проката)</summary>
        public const short ServiceRental = 16;

        /// <summary>Административные расходы. Счёт: 9420</summary>
        public const short ExpenseAdministrative = 17;

        /// <summary>Прочие операционные расходы. Счёт: 9430</summary>
        public const short ExpenseOperating = 18;
    }
}
