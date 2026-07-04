using SharedKernel.Constants;

namespace Application.Features.CounterpartyRegisterBalances;

internal static class CounterpartySettlementOperationTypeResolver
{
    public static bool IsCounterpartySettlementAlias(string? aliasCode) =>
        aliasCode is AliasConst.Customer
            or AliasConst.CustomerAdvance
            or AliasConst.Supplier
            or AliasConst.SupplierAdvance;

    public static short Resolve(string aliasCode, short operationTypeId) =>
        (aliasCode, operationTypeId) switch
        {
            (AliasConst.Customer, OperationTypeIdConst.IN) => OperationTypeIdConst.DEBT_DECREASE,
            (AliasConst.Customer, OperationTypeIdConst.OUT) => OperationTypeIdConst.DEBT_INCREASE,
            (AliasConst.Supplier, OperationTypeIdConst.OUT) => OperationTypeIdConst.DEBT_DECREASE,
            (AliasConst.Supplier, OperationTypeIdConst.IN) => OperationTypeIdConst.DEBT_INCREASE,
            (AliasConst.CustomerAdvance, OperationTypeIdConst.IN) => OperationTypeIdConst.DEBT_INCREASE,
            (AliasConst.CustomerAdvance, OperationTypeIdConst.OUT) => OperationTypeIdConst.DEBT_DECREASE,
            (AliasConst.SupplierAdvance, OperationTypeIdConst.OUT) => OperationTypeIdConst.DEBT_INCREASE,
            (AliasConst.SupplierAdvance, OperationTypeIdConst.IN) => OperationTypeIdConst.DEBT_DECREASE,
            _ => throw new ArgumentOutOfRangeException(
                nameof(aliasCode),
                aliasCode,
                $"Unsupported settlement alias '{aliasCode}' for operation type '{operationTypeId}'.")
        };
}
