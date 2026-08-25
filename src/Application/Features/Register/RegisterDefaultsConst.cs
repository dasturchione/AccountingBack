namespace Application.Features.Register;

public static class RegisterDefaultsConst
{
    public const string DefaultDimensionValue = "_default";

    /// <summary>Измерение входного НДС по МПЗ/товарам — субсчёт 4410.3.</summary>
    public const string VatKindGoods = "goods";

    /// <summary>Измерение входного НДС по услугам — субсчёт 4410.4.</summary>
    public const string VatKindServices = "services";

    /// <summary>Измерение входного НДС при приобретении ОС — субсчёт 4410.1.</summary>
    public const string VatKindFixedAsset = "fixedAsset";

    public const string CashOperation = "CASH_OPERATION";
    public const string CashOperationIn = "CASH_OPERATION_IN";
    public const string CashOperationOut = "CASH_OPERATION_OUT";
    public const string CashFiscalTransferCashBox = "CASH_BOX_TRANSFER";
    public const string FiscalCashRegister = "FISCAL_CASH_REGISTER";
    public const string CashBoxBalance = "CASH_BOX";
    public const string SourceCashBoxDisplayPrefix = "SourceCashBox";
    public const string DestinationCashBoxDisplayPrefix = "DestinationCashBox";
}
