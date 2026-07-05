namespace SharedKernel.Constants;

/// <summary>
/// Tizim ruxsat kodlari — sys_module.code qiymatlariga mos keladi.
/// </summary>
public static class PermissionCodeConst
{
    #region Role
    public const string RoleView         = "ROLE_VIEW";
    public const string RoleViewDetail   = "ROLE_VIEW_DETAIL";
    public const string RoleCreate       = "ROLE_CREATE";
    public const string RoleUpdate       = "ROLE_UPDATE";
    public const string RoleDelete       = "ROLE_DELETE";
    #endregion

    #region User
    public const string UserView         = "USER_VIEW";
    public const string UserViewDetail   = "USER_VIEW_DETAIL";
    public const string UserCreate       = "USER_CREATE";
    public const string UserUpdate       = "USER_UPDATE";
    public const string UserDelete       = "USER_DELETE";
    #endregion

    #region Organization
    public const string OrganizationView       = "ORGANIZATION_VIEW";
    public const string OrganizationViewDetail = "ORGANIZATION_VIEW_DETAIL";
    public const string OrganizationCreate     = "ORGANIZATION_CREATE";
    public const string OrganizationUpdate     = "ORGANIZATION_UPDATE";
    public const string OrganizationDelete     = "ORGANIZATION_DELETE";
    #endregion

    #region Branch
    public const string BranchView       = "BRANCH_VIEW";
    public const string BranchViewDetail = "BRANCH_VIEW_DETAIL";
    public const string BranchCreate     = "BRANCH_CREATE";
    public const string BranchUpdate     = "BRANCH_UPDATE";
    public const string BranchDelete     = "BRANCH_DELETE";
    #endregion

    #region Department
    public const string DepartmentView       = "DEPARTMENT_VIEW";
    public const string DepartmentViewDetail = "DEPARTMENT_VIEW_DETAIL";
    public const string DepartmentCreate     = "DEPARTMENT_CREATE";
    public const string DepartmentUpdate     = "DEPARTMENT_UPDATE";
    public const string DepartmentDelete     = "DEPARTMENT_DELETE";
    #endregion

    #region Position
    public const string PositionView       = "POSITION_VIEW";
    public const string PositionViewDetail = "POSITION_VIEW_DETAIL";
    public const string PositionCreate     = "POSITION_CREATE";
    public const string PositionUpdate     = "POSITION_UPDATE";
    public const string PositionDelete     = "POSITION_DELETE";
    #endregion

    #region CounterpartyCard
    public const string CounterpartyCardView       = "COUNTERPARTY_CARD_VIEW";
    public const string CounterpartyCardViewDetail = "COUNTERPARTY_CARD_VIEW_DETAIL";
    public const string CounterpartyCardCreate     = "COUNTERPARTY_CARD_CREATE";
    public const string CounterpartyCardUpdate     = "COUNTERPARTY_CARD_UPDATE";
    public const string CounterpartyCardDelete     = "COUNTERPARTY_CARD_DELETE";
    #endregion

    #region CounterpartyBankAccount
    public const string CounterpartyBankAccountView       = "COUNTERPARTY_BANK_ACCOUNT_VIEW";
    public const string CounterpartyBankAccountViewDetail = "COUNTERPARTY_BANK_ACCOUNT_VIEW_DETAIL";
    public const string CounterpartyBankAccountCreate     = "COUNTERPARTY_BANK_ACCOUNT_CREATE";
    public const string CounterpartyBankAccountUpdate     = "COUNTERPARTY_BANK_ACCOUNT_UPDATE";
    public const string CounterpartyBankAccountDelete     = "COUNTERPARTY_BANK_ACCOUNT_DELETE";
    #endregion

    #region CounterpartyContact
    public const string CounterpartyContactView       = "COUNTERPARTY_CONTACT_VIEW";
    public const string CounterpartyContactViewDetail = "COUNTERPARTY_CONTACT_VIEW_DETAIL";
    public const string CounterpartyContactCreate     = "COUNTERPARTY_CONTACT_CREATE";
    public const string CounterpartyContactUpdate     = "COUNTERPARTY_CONTACT_UPDATE";
    public const string CounterpartyContactDelete     = "COUNTERPARTY_CONTACT_DELETE";
    #endregion

    #region ProductGroup
    public const string ProductGroupView       = "PRODUCT_GROUP_VIEW";
    public const string ProductGroupViewDetail = "PRODUCT_GROUP_VIEW_DETAIL";
    public const string ProductGroupCreate     = "PRODUCT_GROUP_CREATE";
    public const string ProductGroupUpdate     = "PRODUCT_GROUP_UPDATE";
    public const string ProductGroupDelete     = "PRODUCT_GROUP_DELETE";
    #endregion

    #region Product
    public const string ProductView       = "PRODUCT_VIEW";
    public const string ProductViewDetail = "PRODUCT_VIEW_DETAIL";
    public const string ProductCreate     = "PRODUCT_CREATE";
    public const string ProductUpdate     = "PRODUCT_UPDATE";
    public const string ProductDelete     = "PRODUCT_DELETE";
    #endregion

    #region ProductPrice
    public const string ProductPriceView       = "PRODUCT_PRICE_VIEW";
    public const string ProductPriceViewDetail = "PRODUCT_PRICE_VIEW_DETAIL";
    public const string ProductPriceCreate     = "PRODUCT_PRICE_CREATE";
    public const string ProductPriceUpdate     = "PRODUCT_PRICE_UPDATE";
    public const string ProductPriceDelete     = "PRODUCT_PRICE_DELETE";
    #endregion

    #region Warehouse
    public const string WarehouseView       = "WAREHOUSE_VIEW";
    public const string WarehouseViewDetail = "WAREHOUSE_VIEW_DETAIL";
    public const string WarehouseCreate     = "WAREHOUSE_CREATE";
    public const string WarehouseUpdate     = "WAREHOUSE_UPDATE";
    public const string WarehouseDelete     = "WAREHOUSE_DELETE";
    #endregion

    #region WarehouseTransfer
    public const string WarehouseTransferView       = "WAREHOUSE_TRANSFER_VIEW";
    public const string WarehouseTransferViewDetail = "WAREHOUSE_TRANSFER_VIEW_DETAIL";
    public const string WarehouseTransferCreate     = "WAREHOUSE_TRANSFER_CREATE";
    public const string WarehouseTransferUpdate     = "WAREHOUSE_TRANSFER_UPDATE";
    public const string WarehouseTransferDelete     = "WAREHOUSE_TRANSFER_DELETE";
    public const string ConfirmWarehouseTransfer    = "CONFIRM_WAREHOUSE_TRANSFER";
    public const string CancelWarehouseTransfer     = "CANCEL_WAREHOUSE_TRANSFER";
    #endregion

    #region InventoryAdjustment
    public const string InventoryAdjustmentView       = "INVENTORY_ADJUSTMENT_VIEW";
    public const string InventoryAdjustmentViewDetail = "INVENTORY_ADJUSTMENT_VIEW_DETAIL";
    public const string InventoryAdjustmentCreate     = "INVENTORY_ADJUSTMENT_CREATE";
    public const string InventoryAdjustmentUpdate     = "INVENTORY_ADJUSTMENT_UPDATE";
    public const string InventoryAdjustmentDelete     = "INVENTORY_ADJUSTMENT_DELETE";
    public const string ConfirmInventoryAdjustment    = "CONFIRM_INVENTORY_ADJUSTMENT";
    public const string CancelInventoryAdjustment     = "CANCEL_INVENTORY_ADJUSTMENT";
    #endregion

    #region InventoryCount
    public const string InventoryCountView       = "INVENTORY_COUNT_VIEW";
    public const string InventoryCountViewDetail = "INVENTORY_COUNT_VIEW_DETAIL";
    public const string InventoryCountCreate     = "INVENTORY_COUNT_CREATE";
    public const string InventoryCountUpdate     = "INVENTORY_COUNT_UPDATE";
    public const string InventoryCountDelete     = "INVENTORY_COUNT_DELETE";
    public const string ConfirmInventoryCount    = "CONFIRM_INVENTORY_COUNT";
    public const string CancelInventoryCount     = "CANCEL_INVENTORY_COUNT";
    #endregion


    #region OrgBankAccount
    public const string OrgBankAccountView       = "ORG_BANK_ACCOUNT_VIEW";
    public const string OrgBankAccountViewDetail = "ORG_BANK_ACCOUNT_VIEW_DETAIL";
    public const string OrgBankAccountCreate     = "ORG_BANK_ACCOUNT_CREATE";
    public const string OrgBankAccountUpdate     = "ORG_BANK_ACCOUNT_UPDATE";
    public const string OrgBankAccountDelete     = "ORG_BANK_ACCOUNT_DELETE";
    #endregion

    #region BankOperation
    public const string BankOperationView       = "BANK_OPERATION_VIEW";
    public const string BankOperationViewDetail = "BANK_OPERATION_VIEW_DETAIL";
    public const string BankOperationCreate     = "BANK_OPERATION_CREATE";
    public const string BankOperationUpdate     = "BANK_OPERATION_UPDATE";
    public const string BankOperationDelete     = "BANK_OPERATION_DELETE";
    public const string ConfirmBankOperation    = "CONFIRM_BANK_OPERATION";
    public const string CancelBankOperation     = "CANCEL_BANK_OPERATION";
    #endregion

    #region BankStatementParser
    public const string BankStatementParse = "BANK_STATEMENT_PARSE";
    #endregion

    #region Bank
    public const string BankView       = "BANK_VIEW";
    public const string BankViewDetail = "BANK_VIEW_DETAIL";
    public const string BankCreate     = "BANK_CREATE";
    public const string BankUpdate     = "BANK_UPDATE";
    public const string BankDelete     = "BANK_DELETE";
    #endregion

    #region CashBox
    public const string CashBoxView       = "CASH_BOX_VIEW";
    public const string CashBoxViewDetail = "CASH_BOX_VIEW_DETAIL";
    public const string CashBoxCreate     = "CASH_BOX_CREATE";
    public const string CashBoxUpdate     = "CASH_BOX_UPDATE";
    public const string CashBoxDelete     = "CASH_BOX_DELETE";
    #endregion

    #region CashOperation
    public const string CashOperationView       = "CASH_OPERATION_VIEW";
    public const string CashOperationViewDetail = "CASH_OPERATION_VIEW_DETAIL";
    public const string CashOperationCreate     = "CASH_OPERATION_CREATE";
    public const string CashOperationUpdate     = "CASH_OPERATION_UPDATE";
    public const string CashOperationDelete     = "CASH_OPERATION_DELETE";
    public const string ConfirmCashOperation = "CONFIRM_CASH_OPERATION";
    public const string CancelCashOperation  = "CANCEL_CASH_OPERATION";
    #endregion

    #region PurchaseDoc
    public const string PurchaseDocView       = "PURCHASE_DOC_VIEW";
    public const string PurchaseDocViewDetail = "PURCHASE_DOC_VIEW_DETAIL";
    public const string PurchaseDocCreate     = "PURCHASE_DOC_CREATE";
    public const string PurchaseDocUpdate     = "PURCHASE_DOC_UPDATE";
    public const string PurchaseDocDelete     = "PURCHASE_DOC_DELETE";
    public const string ConfirmPurchase       = "CONFIRM_PURCHASE";
    public const string CancelPurchase        = "CANCEL_PURCHASE";
    #endregion

    #region PurchaseDocTable
    public const string PurchaseDocTableView       = "PURCHASE_DOC_TABLE_VIEW";
    public const string PurchaseDocTableViewDetail = "PURCHASE_DOC_TABLE_VIEW_DETAIL";
    public const string PurchaseDocTableCreate     = "PURCHASE_DOC_TABLE_CREATE";
    public const string PurchaseDocTableUpdate     = "PURCHASE_DOC_TABLE_UPDATE";
    public const string PurchaseDocTableDelete     = "PURCHASE_DOC_TABLE_DELETE";
    #endregion

    #region PurchaseService
    public const string PurchaseServiceView       = "PURCHASE_SERVICE_VIEW";
    public const string PurchaseServiceViewDetail = "PURCHASE_SERVICE_VIEW_DETAIL";
    public const string PurchaseServiceCreate     = "PURCHASE_SERVICE_CREATE";
    public const string PurchaseServiceUpdate     = "PURCHASE_SERVICE_UPDATE";
    public const string PurchaseServiceDelete     = "PURCHASE_SERVICE_DELETE";
    #endregion

    #region SaleDoc
    public const string SaleDocView       = "SALE_DOC_VIEW";
    public const string SaleDocViewDetail = "SALE_DOC_VIEW_DETAIL";
    public const string SaleDocCreate     = "SALE_DOC_CREATE";
    public const string SaleDocUpdate     = "SALE_DOC_UPDATE";
    public const string SaleDocDelete     = "SALE_DOC_DELETE";
    public const string ConfirmSale       = "CONFIRM_SALE";
    public const string CancelSale        = "CANCEL_SALE";
    #endregion

    #region SaleDocTable
    public const string SaleDocTableView       = "SALE_DOC_TABLE_VIEW";
    public const string SaleDocTableViewDetail = "SALE_DOC_TABLE_VIEW_DETAIL";
    public const string SaleDocTableCreate     = "SALE_DOC_TABLE_CREATE";
    public const string SaleDocTableUpdate     = "SALE_DOC_TABLE_UPDATE";
    public const string SaleDocTableDelete     = "SALE_DOC_TABLE_DELETE";
    #endregion

    #region ChartAccount
    public const string ChartAccountView       = "CHART_ACCOUNT_VIEW";
    public const string ChartAccountViewDetail = "CHART_ACCOUNT_VIEW_DETAIL";
    public const string ChartAccountCreate     = "CHART_ACCOUNT_CREATE";
    public const string ChartAccountUpdate     = "CHART_ACCOUNT_UPDATE";
    public const string ChartAccountDelete     = "CHART_ACCOUNT_DELETE";
    #endregion

    #region AccountingRegisterEntry
    public const string AccRegEntryView       = "ACC_REG_ENTRY_VIEW";
    public const string AccRegEntryViewDetail = "ACC_REG_ENTRY_VIEW_DETAIL";
    public const string AccRegEntryCreate     = "ACC_REG_ENTRY_CREATE";
    public const string AccRegEntryUpdate     = "ACC_REG_ENTRY_UPDATE";
    public const string AccRegEntryDelete     = "ACC_REG_ENTRY_DELETE";
    #endregion

    #region CounterpartyRegisterBalance
    public const string CounterpartyRegBalanceView       = "COUNTERPARTY_REG_BALANCE_VIEW";
    public const string CounterpartyRegBalanceViewDetail = "COUNTERPARTY_REG_BALANCE_VIEW_DETAIL";
    public const string CounterpartyRegBalanceCreate     = "COUNTERPARTY_REG_BALANCE_CREATE";
    public const string CounterpartyRegBalanceUpdate     = "COUNTERPARTY_REG_BALANCE_UPDATE";
    public const string CounterpartyRegBalanceDelete     = "COUNTERPARTY_REG_BALANCE_DELETE";
    #endregion

    #region InventoryRegisterBalance
    public const string InventoryRegBalanceView       = "INVENTORY_REG_BALANCE_VIEW";
    public const string InventoryRegBalanceViewDetail = "INVENTORY_REG_BALANCE_VIEW_DETAIL";
    public const string InventoryRegBalanceCreate     = "INVENTORY_REG_BALANCE_CREATE";
    public const string InventoryRegBalanceUpdate     = "INVENTORY_REG_BALANCE_UPDATE";
    public const string InventoryRegBalanceDelete     = "INVENTORY_REG_BALANCE_DELETE";
    #endregion

    #region MoneyRegisterBalance
    public const string MoneyRegBalanceView       = "MONEY_REG_BALANCE_VIEW";
    public const string MoneyRegBalanceViewDetail = "MONEY_REG_BALANCE_VIEW_DETAIL";
    public const string MoneyRegBalanceCreate     = "MONEY_REG_BALANCE_CREATE";
    public const string MoneyRegBalanceUpdate     = "MONEY_REG_BALANCE_UPDATE";
    public const string MoneyRegBalanceDelete     = "MONEY_REG_BALANCE_DELETE";
    #endregion

    #region Contract
    public const string ContractView       = "CONTRACT_VIEW";
    public const string ContractViewDetail = "CONTRACT_VIEW_DETAIL";
    public const string ContractCreate     = "CONTRACT_CREATE";
    public const string ContractUpdate     = "CONTRACT_UPDATE";
    public const string ContractDelete     = "CONTRACT_DELETE";
    #endregion

    #region ProductTable
    public const string ProductTableView       = "PRODUCT_TABLE_VIEW";
    public const string ProductTableViewDetail = "PRODUCT_TABLE_VIEW_DETAIL";
    #endregion

    #region Manual (ma'lumotnomalar)
    public const string ManualView = "MANUAL_VIEW";
    #endregion

    #region Currency
    public const string CurrencyView       = "CURRENCY_VIEW";
    public const string CurrencyViewDetail = "CURRENCY_VIEW_DETAIL";
    public const string CurrencyCreate     = "CURRENCY_CREATE";
    public const string CurrencyUpdate     = "CURRENCY_UPDATE";
    public const string CurrencyDelete     = "CURRENCY_DELETE";
    #endregion

    #region CurrencyRate
    public const string CurrencyRateView       = "CURRENCY_RATE_VIEW";
    public const string CurrencyRateViewDetail = "CURRENCY_RATE_VIEW_DETAIL";
    public const string CurrencyRateCreate     = "CURRENCY_RATE_CREATE";
    public const string CurrencyRateUpdate     = "CURRENCY_RATE_UPDATE";
    public const string CurrencyRateDelete     = "CURRENCY_RATE_DELETE";
    public const string CurrencyRateImport     = "CURRENCY_RATE_IMPORT";
    public const string CurrencyRateSync       = "CURRENCY_RATE_SYNC";
    #endregion

    #region CurrencyRevaluation
    public const string CurrencyRevaluationView = "CURRENCY_REVALUATION_VIEW";
    public const string CurrencyRevaluationCreate = "CURRENCY_REVALUATION_CREATE";
    public const string CurrencyRevaluationConfirm = "CURRENCY_REVALUATION_CONFIRM";
    public const string CurrencyRevaluationCancel = "CURRENCY_REVALUATION_CANCEL";
    #endregion

    #region Tax
    public const string TaxView = "TAX_VIEW";
    public const string TaxViewDetail = "TAX_VIEW_DETAIL";
    public const string TaxCreate = "TAX_CREATE";
    public const string TaxUpdate = "TAX_UPDATE";
    public const string TaxDelete = "TAX_DELETE";
    #endregion

    #region AuditLog
    public const string AuditLogView = "AUDIT_LOG_VIEW";
    #endregion

    #region Settings
    public const string SettingsManage = "SETTINGS_MANAGE";
    #endregion

    #region Dashboard
    public const string DashboardView = "DASHBOARD_VIEW";
    #endregion
}
