namespace SharedKernel.Constants;

/// <summary>
/// Tizim ruxsat kodlari. Har bir authorize qilingan endpoint uchun bitta unique permission.
/// </summary>
public static class PermissionCodeConst
{
    #region AccountingPeriod
    public const string AccountingPeriodClose  = "ACCOUNTING_PERIOD_CLOSE";
    public const string AccountingPeriodReopen = "ACCOUNTING_PERIOD_REOPEN";
    #endregion

    #region ChartAccount
    public const string ChartAccountView       = "CHART_ACCOUNT_VIEW";
    public const string ChartAccountViewDetail = "CHART_ACCOUNT_VIEW_DETAIL";
    public const string ChartAccountCreate     = "CHART_ACCOUNT_CREATE";
    public const string ChartAccountUpdate     = "CHART_ACCOUNT_UPDATE";
    public const string ChartAccountDelete     = "CHART_ACCOUNT_DELETE";
    #endregion

    #region Opening balance
    public const string OpeningBalanceView = "OPENING_BALANCE_VIEW";
    public const string OpeningBalanceViewDetail = "OPENING_BALANCE_VIEW_DETAIL";
    public const string OpeningBalanceCreate = "OPENING_BALANCE_CREATE";
    public const string OpeningBalanceUpdate = "OPENING_BALANCE_UPDATE";
    public const string OpeningBalanceDelete = "OPENING_BALANCE_DELETE";
    #endregion

    #region BankOperation
    public const string BankOperationView       = "BANK_OPERATION_VIEW";
    public const string BankOperationViewDetail = "BANK_OPERATION_VIEW_DETAIL";
    public const string BankOperationCreate     = "BANK_OPERATION_CREATE";
    public const string BankOperationCreateMany = "BANK_OPERATION_CREATE_MANY";
    public const string BankOperationUpdate     = "BANK_OPERATION_UPDATE";
    public const string ConfirmBankOperation    = "CONFIRM_BANK_OPERATION";
    public const string CancelBankOperation     = "CANCEL_BANK_OPERATION";
    public const string BankOperationDelete     = "BANK_OPERATION_DELETE";
    #endregion

    #region BankStatementParser
    public const string BankStatementParse = "BANK_STATEMENT_PARSE";
    #endregion

    #region OrgBankAccount
    public const string OrgBankAccountView       = "ORG_BANK_ACCOUNT_VIEW";
    public const string OrgBankAccountViewDetail = "ORG_BANK_ACCOUNT_VIEW_DETAIL";
    public const string OrgBankAccountCreate     = "ORG_BANK_ACCOUNT_CREATE";
    public const string OrgBankAccountCreateMany = "ORG_BANK_ACCOUNT_CREATE_MANY";
    public const string OrgBankAccountUpdate     = "ORG_BANK_ACCOUNT_UPDATE";
    public const string OrgBankAccountDelete     = "ORG_BANK_ACCOUNT_DELETE";
    #endregion

    #region BankTerminal
    public const string BankTerminalView       = "BANK_TERMINAL_VIEW";
    public const string BankTerminalViewDetail = "BANK_TERMINAL_VIEW_DETAIL";
    public const string BankTerminalCreate     = "BANK_TERMINAL_CREATE";
    public const string BankTerminalUpdate     = "BANK_TERMINAL_UPDATE";
    public const string BankTerminalDelete     = "BANK_TERMINAL_DELETE";
    #endregion

    #region CashBox
    public const string CashBoxView       = "CASH_BOX_VIEW";
    public const string CashBoxViewDetail = "CASH_BOX_VIEW_DETAIL";
    public const string CashBoxCreate     = "CASH_BOX_CREATE";
    public const string CashBoxUpdate     = "CASH_BOX_UPDATE";
    public const string CashBoxDelete     = "CASH_BOX_DELETE";
    #endregion

    #region FiscalCashRegister
    public const string FiscalCashRegisterView       = "FISCAL_CASH_REGISTER_VIEW";
    public const string FiscalCashRegisterViewDetail = "FISCAL_CASH_REGISTER_VIEW_DETAIL";
    public const string FiscalCashRegisterCreate     = "FISCAL_CASH_REGISTER_CREATE";
    public const string FiscalCashRegisterUpdate     = "FISCAL_CASH_REGISTER_UPDATE";
    public const string FiscalCashRegisterDelete     = "FISCAL_CASH_REGISTER_DELETE";
    #endregion

    #region CashDocument
    public const string CashDocumentGetReceiptOrders    = "CASH_DOCUMENT_GET_RECEIPT_ORDERS";
    public const string CashDocumentGetPaymentOrders    = "CASH_DOCUMENT_GET_PAYMENT_ORDERS";
    public const string CashDocumentGetReceiptOrderById = "CASH_DOCUMENT_GET_RECEIPT_ORDER_BY_ID";
    public const string CashDocumentGetPaymentOrderById = "CASH_DOCUMENT_GET_PAYMENT_ORDER_BY_ID";
    public const string CashDocumentCreateReceiptOrder  = "CASH_DOCUMENT_CREATE_RECEIPT_ORDER";
    public const string CashDocumentCreatePaymentOrder  = "CASH_DOCUMENT_CREATE_PAYMENT_ORDER";
    public const string CashDocumentUpdateReceiptOrder  = "CASH_DOCUMENT_UPDATE_RECEIPT_ORDER";
    public const string CashDocumentUpdatePaymentOrder  = "CASH_DOCUMENT_UPDATE_PAYMENT_ORDER";
    public const string CashDocumentConfirmReceiptOrder = "CASH_DOCUMENT_CONFIRM_RECEIPT_ORDER";
    public const string CashDocumentConfirmPaymentOrder = "CASH_DOCUMENT_CONFIRM_PAYMENT_ORDER";
    public const string CashDocumentCancelReceiptOrder  = "CASH_DOCUMENT_CANCEL_RECEIPT_ORDER";
    public const string CashDocumentCancelPaymentOrder  = "CASH_DOCUMENT_CANCEL_PAYMENT_ORDER";
    public const string CashDocumentDeleteReceiptOrder  = "CASH_DOCUMENT_DELETE_RECEIPT_ORDER";
    public const string CashDocumentDeletePaymentOrder  = "CASH_DOCUMENT_DELETE_PAYMENT_ORDER";
    #endregion

    #region CashOperation
    public const string CashOperationView       = "CASH_OPERATION_VIEW";
    public const string CashOperationViewDetail = "CASH_OPERATION_VIEW_DETAIL";
    public const string CashOperationCreate     = "CASH_OPERATION_CREATE";
    public const string ConfirmCashOperation    = "CONFIRM_CASH_OPERATION";
    public const string CancelCashOperation     = "CANCEL_CASH_OPERATION";
    public const string CashOperationUpdate     = "CASH_OPERATION_UPDATE";
    public const string CashOperationDelete     = "CASH_OPERATION_DELETE";
    #endregion

    #region CashFiscalTransfer
    public const string CashFiscalTransferView = "CASH_FISCAL_TRANSFER_VIEW";
    public const string CashFiscalTransferViewDetail = "CASH_FISCAL_TRANSFER_VIEW_DETAIL";
    public const string CashFiscalTransferCreate = "CASH_FISCAL_TRANSFER_CREATE";
    public const string CashFiscalTransferUpdate = "CASH_FISCAL_TRANSFER_UPDATE";
    public const string CashFiscalTransferDelete = "CASH_FISCAL_TRANSFER_DELETE";
    public const string ConfirmCashFiscalTransfer = "CONFIRM_CASH_FISCAL_TRANSFER";
    public const string CancelCashFiscalTransfer = "CANCEL_CASH_FISCAL_TRANSFER";
    #endregion

    #region CashCollection
    public const string CashCollectionView = "CASH_COLLECTION_VIEW";
    public const string CashCollectionViewDetail = "CASH_COLLECTION_VIEW_DETAIL";
    public const string CashCollectionCreate = "CASH_COLLECTION_CREATE";
    public const string CashCollectionUpdate = "CASH_COLLECTION_UPDATE";
    public const string CashCollectionDelete = "CASH_COLLECTION_DELETE";
    public const string CashCollectionSendToBank = "CASH_COLLECTION_SEND_TO_BANK";
    public const string CashCollectionCancel = "CASH_COLLECTION_CANCEL";
    #endregion

    #region DocumentAccountSetting
    public const string DocumentAccountSettingView = "DOCUMENT_ACCOUNT_SETTING_VIEW";
    public const string DocumentAccountSettingViewDetail = "DOCUMENT_ACCOUNT_SETTING_VIEW_DETAIL";
    public const string DocumentAccountSettingSave = "DOCUMENT_ACCOUNT_SETTING_SAVE";
    #endregion

    #region Bank
    public const string BankView       = "BANK_VIEW";
    public const string BankViewDetail = "BANK_VIEW_DETAIL";
    public const string BankCreate     = "BANK_CREATE";
    public const string BankUpdate     = "BANK_UPDATE";
    public const string BankDelete     = "BANK_DELETE";
    #endregion

    #region Barcode
    public const string BarcodeGenerateQr      = "BARCODE_GENERATE_QR";
    public const string BarcodeGenerateCode128 = "BARCODE_GENERATE_CODE128";
    public const string BarcodeGenerateEan13   = "BARCODE_GENERATE_EAN13";
    #endregion

    #region Contract
    public const string ContractView       = "CONTRACT_VIEW";
    public const string ContractViewDetail = "CONTRACT_VIEW_DETAIL";
    public const string ContractCreate     = "CONTRACT_CREATE";
    public const string ContractUpdate     = "CONTRACT_UPDATE";
    public const string ContractDelete     = "CONTRACT_DELETE";
    #endregion

    #region Currency
    public const string CurrencyView       = "CURRENCY_VIEW";
    public const string CurrencyViewDetail = "CURRENCY_VIEW_DETAIL";
    public const string CurrencyCreate     = "CURRENCY_CREATE";
    public const string CurrencyUpdate     = "CURRENCY_UPDATE";
    public const string CurrencyDelete     = "CURRENCY_DELETE";
    #endregion

    #region CurrencyRates
    public const string CurrencyRateView             = "CURRENCY_RATE_VIEW";
    public const string CurrencyRateViewDetail       = "CURRENCY_RATE_VIEW_DETAIL";
    public const string CurrencyRatesGetLatest       = "CURRENCY_RATES_GET_LATEST";
    public const string CurrencyRatesGetHistory      = "CURRENCY_RATES_GET_HISTORY";
    public const string CurrencyRateCreate           = "CURRENCY_RATE_CREATE";
    public const string CurrencyRateUpdate           = "CURRENCY_RATE_UPDATE";
    public const string CurrencyRateDelete           = "CURRENCY_RATE_DELETE";
    public const string CurrencyRatesGetProviders    = "CURRENCY_RATES_GET_PROVIDERS";
    public const string CurrencyRatesGetImportStatus = "CURRENCY_RATES_GET_IMPORT_STATUS";
    public const string CurrencyRateImport           = "CURRENCY_RATE_IMPORT";
    public const string CurrencyRateSync             = "CURRENCY_RATE_SYNC";
    #endregion

    #region CurrencyRevaluations
    public const string CurrencyRevaluationView       = "CURRENCY_REVALUATION_VIEW";
    public const string CurrencyRevaluationViewDetail = "CURRENCY_REVALUATION_VIEW_DETAIL";
    public const string CurrencyRevaluationPreview    = "CURRENCY_REVALUATION_PREVIEW";
    public const string CurrencyRevaluationCreate     = "CURRENCY_REVALUATION_CREATE";
    public const string CurrencyRevaluationConfirm    = "CURRENCY_REVALUATION_CONFIRM";
    public const string CurrencyRevaluationCancel     = "CURRENCY_REVALUATION_CANCEL";
    #endregion

    #region Manual
    public const string ManualGetStates                   = "MANUAL_GET_STATES";
    public const string ManualGetRegions                  = "MANUAL_GET_REGIONS";
    public const string ManualGetDistricts                = "MANUAL_GET_DISTRICTS";
    public const string ManualGetCurrencies               = "MANUAL_GET_CURRENCIES";
    public const string ManualGetUnits                    = "MANUAL_GET_UNITS";
    public const string ManualGetDocumentStatuses         = "MANUAL_GET_DOCUMENT_STATUSES";
    public const string ManualGetPaymentTypes             = "MANUAL_GET_PAYMENT_TYPES";
    public const string ManualGetInventoryAdjustmentTypes = "MANUAL_GET_INVENTORY_ADJUSTMENT_TYPES";
    public const string ManualGetFaGroups                 = "MANUAL_GET_FA_GROUPS";
    public const string ManualGetFaOkofs                  = "MANUAL_GET_FA_OKOFS";
    public const string ManualGetFaDepreciationMethods    = "MANUAL_GET_FA_DEPRECIATION_METHODS";
    public const string ManualGetPriceRoundingMethods     = "MANUAL_GET_PRICE_ROUNDING_METHODS";
    public const string ManualGetPricingMethods           = "MANUAL_GET_PRICING_METHODS";
    public const string ManualGetCostingMethods           = "MANUAL_GET_COSTING_METHODS";
    public const string ManualGetBanks                    = "MANUAL_GET_BANKS";
    public const string ManualGetDocumentTypes            = "MANUAL_GET_DOCUMENT_TYPES";
    public const string ManualGetOperationTypes           = "MANUAL_GET_OPERATION_TYPES";
    public const string ManualGetMovementDirections      = "MANUAL_GET_MOVEMENT_DIRECTIONS";
    public const string ManualGetTaxTypes                 = "MANUAL_GET_TAX_TYPES";
    public const string ManualGetVatRates                 = "MANUAL_GET_VAT_RATES";
    public const string ManualGetContractTypes            = "MANUAL_GET_CONTRACT_TYPES";
    public const string ManualGetRoles                    = "MANUAL_GET_ROLES";
    public const string ManualGetUsers                    = "MANUAL_GET_USERS";
    public const string ManualGetOrganizations            = "MANUAL_GET_ORGANIZATIONS";
    public const string ManualGetBranches                 = "MANUAL_GET_BRANCHES";
    public const string ManualGetDepartments              = "MANUAL_GET_DEPARTMENTS";
    public const string ManualGetPositions                = "MANUAL_GET_POSITIONS";
    public const string ManualGetContracts                = "MANUAL_GET_CONTRACTS";
    public const string ManualGetCounterparties           = "MANUAL_GET_COUNTERPARTIES";
    public const string ManualGetProductGroups            = "MANUAL_GET_PRODUCT_GROUPS";
    public const string ManualGetProducts                 = "MANUAL_GET_PRODUCTS";
    public const string ManualGetWarehouses               = "MANUAL_GET_WAREHOUSES";
    public const string ManualGetChartAccounts            = "MANUAL_GET_CHART_ACCOUNTS";
    public const string ManualGetAccountingPolicies       = "MANUAL_GET_ACCOUNTING_POLICIES";
    public const string ManualGetOrgBankAccounts          = "MANUAL_GET_ORG_BANK_ACCOUNTS";
    public const string ManualGetCounterpartyBankAccounts = "MANUAL_GET_COUNTERPARTY_BANK_ACCOUNTS";
    public const string ManualGetCashBoxes                = "MANUAL_GET_CASH_BOXES";
    public const string ManualGetCashOperations           = "MANUAL_GET_CASH_OPERATIONS";
    public const string ManualGetLanguages                = "MANUAL_GET_LANGUAGES";
    public const string ManualGetModuleSubGroups          = "MANUAL_GET_MODULE_SUB_GROUPS";
    public const string ManualGetBankTerminals            = "MANUAL_GET_BANK_TERMINALS";
    public const string ManualGetPaymentMethods           = "MANUAL_GET_PAYMENT_METHODS";
    public const string ManualGetFiscalCashRegisters      = "MANUAL_GET_FISCAL_CASH_REGISTERS";
    public const string ManualGetFiscalCashRegisterTypes  = "MANUAL_GET_FISCAL_CASH_REGISTER_TYPES";
    #endregion

    #region PricingCondition
    public const string PricingConditionView       = "PRICING_CONDITION_VIEW";
    public const string PricingConditionGetNow     = "PRICING_CONDITION_GET_NOW";
    public const string PricingConditionViewDetail = "PRICING_CONDITION_VIEW_DETAIL";
    public const string PricingConditionCreate     = "PRICING_CONDITION_CREATE";
    public const string PricingConditionDelete     = "PRICING_CONDITION_DELETE";
    #endregion

    #region Tax
    public const string TaxView              = "TAX_VIEW";
    public const string TaxViewDetail        = "TAX_VIEW_DETAIL";
    public const string TaxCreate            = "TAX_CREATE";
    public const string TaxUpdate            = "TAX_UPDATE";
    public const string TaxDelete            = "TAX_DELETE";
    public const string TaxCalculate         = "TAX_CALCULATE";
    public const string TaxResolve           = "TAX_RESOLVE";
    public const string TaxGetProviders      = "TAX_GET_PROVIDERS";
    public const string TaxGetProviderStatus = "TAX_GET_PROVIDER_STATUS";
    public const string TaxSearchMxik        = "TAX_SEARCH_MXIK";
    public const string TaxGetMxikByCode     = "TAX_GET_MXIK_BY_CODE";
    public const string TaxSearchSoliq       = "TAX_SEARCH_SOLIQ";
    public const string TaxSubmitEFaktura    = "TAX_SUBMIT_E_FAKTURA";
    public const string TaxGetEFakturaStatus = "TAX_GET_E_FAKTURA_STATUS";
    public const string TaxCancelEFaktura    = "TAX_CANCEL_E_FAKTURA";
    #endregion

    #region CounterpartyBankAccount
    public const string CounterpartyBankAccountView       = "COUNTERPARTY_BANK_ACCOUNT_VIEW";
    public const string CounterpartyBankAccountViewDetail = "COUNTERPARTY_BANK_ACCOUNT_VIEW_DETAIL";
    public const string CounterpartyBankAccountCreate     = "COUNTERPARTY_BANK_ACCOUNT_CREATE";
    public const string CounterpartyBankAccountUpdate     = "COUNTERPARTY_BANK_ACCOUNT_UPDATE";
    public const string CounterpartyBankAccountDelete     = "COUNTERPARTY_BANK_ACCOUNT_DELETE";
    #endregion

    #region CounterpartyCard
    public const string CounterpartyCardView       = "COUNTERPARTY_CARD_VIEW";
    public const string CounterpartyCardViewDetail = "COUNTERPARTY_CARD_VIEW_DETAIL";
    public const string CounterpartyCardCreate     = "COUNTERPARTY_CARD_CREATE";
    public const string CounterpartyCardCreateMany = "COUNTERPARTY_CARD_CREATE_MANY";
    public const string CounterpartyCardUpdate     = "COUNTERPARTY_CARD_UPDATE";
    public const string CounterpartyCardDelete     = "COUNTERPARTY_CARD_DELETE";
    #endregion

    #region CounterpartyContact
    public const string CounterpartyContactView       = "COUNTERPARTY_CONTACT_VIEW";
    public const string CounterpartyContactViewDetail = "COUNTERPARTY_CONTACT_VIEW_DETAIL";
    public const string CounterpartyContactCreate     = "COUNTERPARTY_CONTACT_CREATE";
    public const string CounterpartyContactUpdate     = "COUNTERPARTY_CONTACT_UPDATE";
    public const string CounterpartyContactDelete     = "COUNTERPARTY_CONTACT_DELETE";
    #endregion

    #region FaAsset
    public const string FaAssetView       = "FA_ASSET_VIEW";
    public const string FaAssetViewDetail = "FA_ASSET_VIEW_DETAIL";
    public const string FaAssetUpdate     = "FA_ASSET_UPDATE";
    public const string FaAssetDelete     = "FA_ASSET_DELETE";
    #endregion

    #region FaCommissioning
    public const string FaCommissioningView       = "FA_COMMISSIONING_VIEW";
    public const string FaCommissioningViewDetail = "FA_COMMISSIONING_VIEW_DETAIL";
    public const string FaCommissioningCreate     = "FA_COMMISSIONING_CREATE";
    public const string FaCommissioningUpdate     = "FA_COMMISSIONING_UPDATE";
    public const string ConfirmFaCommissioning    = "FA_COMMISSIONING_CONFIRM";
    public const string CancelFaCommissioning     = "FA_COMMISSIONING_CANCEL";
    #endregion

    #region FaDepreciation
    public const string FaDepreciationView       = "FA_DEPRECIATION_VIEW";
    public const string FaDepreciationViewDetail = "FA_DEPRECIATION_VIEW_DETAIL";
    public const string FaDepreciationRun        = "FA_DEPRECIATION_RUN";
    public const string CancelFaDepreciation     = "FA_DEPRECIATION_CANCEL";
    #endregion

    #region FaDisposal
    public const string FaDisposalView       = "FA_DISPOSAL_VIEW";
    public const string FaDisposalViewDetail = "FA_DISPOSAL_VIEW_DETAIL";
    public const string FaDisposalCreate     = "FA_DISPOSAL_CREATE";
    public const string FaDisposalUpdate     = "FA_DISPOSAL_UPDATE";
    public const string ConfirmFaDisposal    = "FA_DISPOSAL_CONFIRM";
    public const string CancelFaDisposal     = "FA_DISPOSAL_CANCEL";
    #endregion

    #region FaMovement
    public const string FaMovementView       = "FA_MOVEMENT_VIEW";
    public const string FaMovementViewDetail = "FA_MOVEMENT_VIEW_DETAIL";
    public const string FaMovementCreate     = "FA_MOVEMENT_CREATE";
    public const string FaMovementUpdate     = "FA_MOVEMENT_UPDATE";
    public const string ConfirmFaMovement    = "FA_MOVEMENT_CONFIRM";
    public const string CancelFaMovement     = "FA_MOVEMENT_CANCEL";
    #endregion

    #region FaReceipt
    public const string FaReceiptView       = "FA_RECEIPT_VIEW";
    public const string FaReceiptViewDetail = "FA_RECEIPT_VIEW_DETAIL";
    public const string FaReceiptCreate     = "FA_RECEIPT_CREATE";
    public const string FaReceiptUpdate     = "FA_RECEIPT_UPDATE";
    public const string ConfirmFaReceipt    = "FA_RECEIPT_CONFIRM";
    public const string CancelFaReceipt     = "FA_RECEIPT_CANCEL";
    public const string FaReceiptDelete     = "FA_RECEIPT_DELETE";
    #endregion

    #region FaRevaluation
    public const string FaRevaluationView       = "FA_REVALUATION_VIEW";
    public const string FaRevaluationViewDetail = "FA_REVALUATION_VIEW_DETAIL";
    public const string FaRevaluationCreate     = "FA_REVALUATION_CREATE";
    public const string FaRevaluationUpdate     = "FA_REVALUATION_UPDATE";
    public const string ConfirmFaRevaluation    = "FA_REVALUATION_CONFIRM";
    public const string CancelFaRevaluation     = "FA_REVALUATION_CANCEL";
    #endregion

    #region InventoryAdjustment
    public const string InventoryAdjustmentView                  = "INVENTORY_ADJUSTMENT_VIEW";
    public const string InventoryAdjustmentViewDetail            = "INVENTORY_ADJUSTMENT_VIEW_DETAIL";
    public const string InventoryAdjustmentCreate                = "INVENTORY_ADJUSTMENT_CREATE";
    public const string InventoryAdjustmentUpdate                = "INVENTORY_ADJUSTMENT_UPDATE";
    public const string InventoryAdjustmentDelete                = "INVENTORY_ADJUSTMENT_DELETE";
    public const string ConfirmInventoryAdjustment               = "CONFIRM_INVENTORY_ADJUSTMENT";
    public const string CancelInventoryAdjustment                = "CANCEL_INVENTORY_ADJUSTMENT";
    public const string InventoryAdjustmentGetPostingBatches     = "INVENTORY_ADJUSTMENT_GET_POSTING_BATCHES";
    public const string InventoryAdjustmentGetInventoryMovements = "INVENTORY_ADJUSTMENT_GET_INVENTORY_MOVEMENTS";
    #endregion

    #region InventoryCount
    public const string InventoryCountView                  = "INVENTORY_COUNT_VIEW";
    public const string InventoryCountViewDetail            = "INVENTORY_COUNT_VIEW_DETAIL";
    public const string InventoryCountCreate                = "INVENTORY_COUNT_CREATE";
    public const string InventoryCountUpdate                = "INVENTORY_COUNT_UPDATE";
    public const string InventoryCountDelete                = "INVENTORY_COUNT_DELETE";
    public const string ConfirmInventoryCount               = "CONFIRM_INVENTORY_COUNT";
    public const string CancelInventoryCount                = "CANCEL_INVENTORY_COUNT";
    public const string InventoryCountGetPostingBatches     = "INVENTORY_COUNT_GET_POSTING_BATCHES";
    public const string InventoryCountGetInventoryMovements = "INVENTORY_COUNT_GET_INVENTORY_MOVEMENTS";
    public const string InventoryCountGetDifferences        = "INVENTORY_COUNT_GET_DIFFERENCES";
    #endregion

    #region Product
    public const string ProductView       = "PRODUCT_VIEW";
    public const string ProductViewDetail = "PRODUCT_VIEW_DETAIL";
    public const string ProductCreate     = "PRODUCT_CREATE";
    public const string ProductCreateMany = "PRODUCT_CREATE_MANY";
    public const string ProductUpdate     = "PRODUCT_UPDATE";
    public const string ProductDelete     = "PRODUCT_DELETE";
    #endregion

    #region ProductGroup
    public const string ProductGroupView       = "PRODUCT_GROUP_VIEW";
    public const string ProductGroupViewDetail = "PRODUCT_GROUP_VIEW_DETAIL";
    public const string ProductGroupCreate     = "PRODUCT_GROUP_CREATE";
    public const string ProductGroupUpdate     = "PRODUCT_GROUP_UPDATE";
    public const string ProductGroupDelete     = "PRODUCT_GROUP_DELETE";
    #endregion

    #region ProductImport
    public const string ProductImportImport = "PRODUCT_IMPORT_IMPORT";
    #endregion

    #region ProductPrice
    public const string ProductPriceView                       = "PRODUCT_PRICE_VIEW";
    public const string ProductPriceViewDetail                 = "PRODUCT_PRICE_VIEW_DETAIL";
    public const string ProductPriceGetPriceDetailsByProductId = "PRODUCT_PRICE_GET_PRICE_DETAILS_BY_PRODUCT_ID";
    public const string ProductPriceCreate                     = "PRODUCT_PRICE_CREATE";
    public const string ProductPriceUpdate                     = "PRODUCT_PRICE_UPDATE";
    public const string ProductPriceDelete                     = "PRODUCT_PRICE_DELETE";
    #endregion

    #region ProductStock
    public const string ProductStockGetByMarkingNumber     = "PRODUCT_STOCK_GET_BY_MARKING_NUMBER";
    public const string ProductStockGetProductGroupSummary = "PRODUCT_STOCK_GET_PRODUCT_GROUP_SUMMARY";
    public const string ProductStockGetProductSummary      = "PRODUCT_STOCK_GET_PRODUCT_SUMMARY";
    public const string ProductStockGetProductTableSummary = "PRODUCT_STOCK_GET_PRODUCT_TABLE_SUMMARY";
    #endregion

    #region Warehouse
    public const string WarehouseView       = "WAREHOUSE_VIEW";
    public const string WarehouseViewDetail = "WAREHOUSE_VIEW_DETAIL";
    public const string WarehouseCreate     = "WAREHOUSE_CREATE";
    public const string WarehouseUpdate     = "WAREHOUSE_UPDATE";
    public const string WarehouseDelete     = "WAREHOUSE_DELETE";
    #endregion

    #region WarehouseTransfer
    public const string WarehouseTransferView                  = "WAREHOUSE_TRANSFER_VIEW";
    public const string WarehouseTransferViewDetail            = "WAREHOUSE_TRANSFER_VIEW_DETAIL";
    public const string WarehouseTransferCreate                = "WAREHOUSE_TRANSFER_CREATE";
    public const string WarehouseTransferUpdate                = "WAREHOUSE_TRANSFER_UPDATE";
    public const string WarehouseTransferDelete                = "WAREHOUSE_TRANSFER_DELETE";
    public const string ConfirmWarehouseTransfer               = "CONFIRM_WAREHOUSE_TRANSFER";
    public const string CancelWarehouseTransfer                = "CANCEL_WAREHOUSE_TRANSFER";
    public const string WarehouseTransferGetPostingBatches     = "WAREHOUSE_TRANSFER_GET_POSTING_BATCHES";
    public const string WarehouseTransferGetInventoryMovements = "WAREHOUSE_TRANSFER_GET_INVENTORY_MOVEMENTS";
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

    #region Organization
    public const string OrganizationView       = "ORGANIZATION_VIEW";
    public const string OrganizationViewDetail = "ORGANIZATION_VIEW_DETAIL";
    public const string OrganizationCreate     = "ORGANIZATION_CREATE";
    public const string OrganizationUpdate     = "ORGANIZATION_UPDATE";
    public const string OrganizationDelete     = "ORGANIZATION_DELETE";
    #endregion

    #region Setup
    public const string SetupGet                    = "SETUP_GET";
    public const string SetupUpdateCompanyProfile   = "SETUP_UPDATE_COMPANY_PROFILE";
    public const string SetupUpdateTaxSettings      = "SETUP_UPDATE_TAX_SETTINGS";
    public const string SetupUpdateAccountingPolicy = "SETUP_UPDATE_ACCOUNTING_POLICY";
    public const string SetupUpdateDefaults         = "SETUP_UPDATE_DEFAULTS";
    public const string SetupUpdateUsers            = "SETUP_UPDATE_USERS";
    public const string SetupComplete               = "SETUP_COMPLETE";
    #endregion

    #region Platform
    public const string PlatformGetDashboard               = "PLATFORM_GET_DASHBOARD";
    public const string PlatformGetTenants                 = "PLATFORM_GET_TENANTS";
    public const string PlatformGetTenantById              = "PLATFORM_GET_TENANT_BY_ID";
    public const string PlatformCreateTenant               = "PLATFORM_CREATE_TENANT";
    public const string PlatformUpdateTenant               = "PLATFORM_UPDATE_TENANT";
    public const string PlatformActivateTenant             = "PLATFORM_ACTIVATE_TENANT";
    public const string PlatformDeactivateTenant           = "PLATFORM_DEACTIVATE_TENANT";
    public const string PlatformGetUsers                   = "PLATFORM_GET_USERS";
    public const string PlatformGetUserById                = "PLATFORM_GET_USER_BY_ID";
    public const string PlatformCreateUser                 = "PLATFORM_CREATE_USER";
    public const string PlatformUpdateUser                 = "PLATFORM_UPDATE_USER";
    public const string PlatformBlockUser                  = "PLATFORM_BLOCK_USER";
    public const string PlatformUnblockUser                = "PLATFORM_UNBLOCK_USER";
    public const string PlatformGetOrganizations           = "PLATFORM_GET_ORGANIZATIONS";
    public const string PlatformGetOrganizationById        = "PLATFORM_GET_ORGANIZATION_BY_ID";
    public const string PlatformUpdateOrganization         = "PLATFORM_UPDATE_ORGANIZATION";
    public const string PlatformActivateOrganization       = "PLATFORM_ACTIVATE_ORGANIZATION";
    public const string PlatformDeactivateOrganization     = "PLATFORM_DEACTIVATE_ORGANIZATION";
    public const string PlatformArchiveOrganization        = "PLATFORM_ARCHIVE_ORGANIZATION";
    public const string PlatformAttachUserToOrganization   = "PLATFORM_ATTACH_USER_TO_ORGANIZATION";
    public const string PlatformUpdateUserOrganization     = "PLATFORM_UPDATE_USER_ORGANIZATION";
    public const string PlatformRemoveUserFromOrganization = "PLATFORM_REMOVE_USER_FROM_ORGANIZATION";
    public const string PlatformGetRoles                   = "PLATFORM_GET_ROLES";
    public const string PlatformGetRoleById                = "PLATFORM_GET_ROLE_BY_ID";
    public const string PlatformCreateRole                 = "PLATFORM_CREATE_ROLE";
    public const string PlatformUpdateRole                 = "PLATFORM_UPDATE_ROLE";
    public const string PlatformDeleteRole                 = "PLATFORM_DELETE_ROLE";
    public const string PlatformSetUserPassword            = "PLATFORM_SET_USER_PASSWORD";
    public const string PlatformGetAuditLogs               = "PLATFORM_GET_AUDIT_LOGS";
    #endregion

    #region PurchaseDoc
    public const string PurchaseDocView       = "PURCHASE_DOC_VIEW";
    public const string PurchaseDocViewDetail = "PURCHASE_DOC_VIEW_DETAIL";
    public const string PurchaseDocCreate     = "PURCHASE_DOC_CREATE";
    public const string PurchaseDocUpdate     = "PURCHASE_DOC_UPDATE";
    public const string ConfirmPurchase       = "CONFIRM_PURCHASE";
    public const string CancelPurchase        = "CANCEL_PURCHASE";
    public const string PurchaseDocDelete     = "PURCHASE_DOC_DELETE";
    #endregion

    #region PurchaseDocTable
    public const string PurchaseDocTableView       = "PURCHASE_DOC_TABLE_VIEW";
    public const string PurchaseDocTableViewDetail = "PURCHASE_DOC_TABLE_VIEW_DETAIL";
    public const string PurchaseDocTableCreate     = "PURCHASE_DOC_TABLE_CREATE";
    public const string PurchaseDocTableUpdate     = "PURCHASE_DOC_TABLE_UPDATE";
    public const string PurchaseDocTableDelete     = "PURCHASE_DOC_TABLE_DELETE";
    #endregion

    #region AccountingRegisterEntry
    public const string AccountingRegisterEntryGetPostings      = "ACCOUNTING_REGISTER_ENTRY_GET_POSTINGS";
    public const string AccountingRegisterEntryGetDailyPostings = "ACCOUNTING_REGISTER_ENTRY_GET_DAILY_POSTINGS";
    #endregion

    #region CashBook
    public const string CashBookGet = "CASH_BOOK_GET";
    #endregion

    #region CounterpartyRegisterBalance
    public const string CounterpartyRegBalanceView       = "COUNTERPARTY_REG_BALANCE_VIEW";
    public const string CounterpartyRegBalanceViewDetail = "COUNTERPARTY_REG_BALANCE_VIEW_DETAIL";
    public const string CounterpartyRegBalanceCreate     = "COUNTERPARTY_REG_BALANCE_CREATE";
    public const string CounterpartyRegBalanceUpdate     = "COUNTERPARTY_REG_BALANCE_UPDATE";
    public const string CounterpartyRegBalanceDelete     = "COUNTERPARTY_REG_BALANCE_DELETE";
    #endregion

    #region Ledger
    public const string LedgerGet = "LEDGER_GET";
    #endregion

    #region MoneyRegisterBalance
    public const string MoneyRegBalanceView       = "MONEY_REG_BALANCE_VIEW";
    public const string MoneyRegBalanceViewDetail = "MONEY_REG_BALANCE_VIEW_DETAIL";
    public const string MoneyRegBalanceCreate     = "MONEY_REG_BALANCE_CREATE";
    public const string MoneyRegBalanceUpdate     = "MONEY_REG_BALANCE_UPDATE";
    public const string MoneyRegBalanceDelete     = "MONEY_REG_BALANCE_DELETE";
    #endregion

    #region Repost
    public const string RepostRepost = "REPOST_REPOST";
    #endregion

    #region TrialBalance
    public const string TrialBalanceGet = "TRIAL_BALANCE_GET";
    #endregion

    #region AccountingReport
    public const string AccountingReportGetBalanceSheet    = "ACCOUNTING_REPORT_GET_BALANCE_SHEET";
    public const string AccountingReportGetIncomeStatement = "ACCOUNTING_REPORT_GET_INCOME_STATEMENT";
    public const string AccountingReportGetCashFlow        = "ACCOUNTING_REPORT_GET_CASH_FLOW";
    public const string AccountingReportGetAccountTurnover = "ACCOUNTING_REPORT_GET_ACCOUNT_TURNOVER";
    public const string AccountingReportGetAccountCard     = "ACCOUNTING_REPORT_GET_ACCOUNT_CARD";
    public const string AccountingReportGetJournal         = "ACCOUNTING_REPORT_GET_JOURNAL";
    #endregion

    #region BankReport
    public const string BankReportOperations       = "BANK_REPORT_OPERATIONS";
    public const string BankReportOperationById    = "BANK_REPORT_OPERATION_BY_ID";
    public const string BankReportExportOperations = "BANK_REPORT_EXPORT_OPERATIONS";
    #endregion

    #region CashReport
    public const string CashReportOperations       = "CASH_REPORT_OPERATIONS";
    public const string CashReportOperationById    = "CASH_REPORT_OPERATION_BY_ID";
    public const string CashReportExportOperations = "CASH_REPORT_EXPORT_OPERATIONS";
    #endregion

    #region FinancialReport
    public const string FinancialReportBalanceSheet          = "FINANCIAL_REPORT_BALANCE_SHEET";
    public const string FinancialReportExportBalanceSheet    = "FINANCIAL_REPORT_EXPORT_BALANCE_SHEET";
    public const string FinancialReportIncomeStatement       = "FINANCIAL_REPORT_INCOME_STATEMENT";
    public const string FinancialReportExportIncomeStatement = "FINANCIAL_REPORT_EXPORT_INCOME_STATEMENT";
    public const string FinancialReportCashFlow              = "FINANCIAL_REPORT_CASH_FLOW";
    public const string FinancialReportExportCashFlow        = "FINANCIAL_REPORT_EXPORT_CASH_FLOW";
    public const string FinancialReportTurnover              = "FINANCIAL_REPORT_TURNOVER";
    public const string FinancialReportExportTurnover        = "FINANCIAL_REPORT_EXPORT_TURNOVER";
    public const string FinancialReportCard                  = "FINANCIAL_REPORT_CARD";
    public const string FinancialReportExportCard            = "FINANCIAL_REPORT_EXPORT_CARD";
    public const string FinancialReportJournal               = "FINANCIAL_REPORT_JOURNAL";
    public const string FinancialReportExportJournal         = "FINANCIAL_REPORT_EXPORT_JOURNAL";
    #endregion

    #region PayableReport
    public const string PayableReportBalances       = "PAYABLE_REPORT_BALANCES";
    public const string PayableReportBalanceById    = "PAYABLE_REPORT_BALANCE_BY_ID";
    public const string PayableReportExportBalances = "PAYABLE_REPORT_EXPORT_BALANCES";
    #endregion

    #region PurchaseReport
    public const string PurchaseReportGetAll  = "PURCHASE_REPORT_GET_ALL";
    public const string PurchaseReportGetById = "PURCHASE_REPORT_GET_BY_ID";
    public const string PurchaseReportExport  = "PURCHASE_REPORT_EXPORT";
    #endregion

    #region ReceivableReport
    public const string ReceivableReportBalances       = "RECEIVABLE_REPORT_BALANCES";
    public const string ReceivableReportBalanceById    = "RECEIVABLE_REPORT_BALANCE_BY_ID";
    public const string ReceivableReportExportBalances = "RECEIVABLE_REPORT_EXPORT_BALANCES";
    #endregion

    #region SalesReport
    public const string SalesReportGetAll  = "SALES_REPORT_GET_ALL";
    public const string SalesReportGetById = "SALES_REPORT_GET_BY_ID";
    public const string SalesReportExport  = "SALES_REPORT_EXPORT";
    #endregion

    #region WarehouseReport
    public const string WarehouseReportTransfers       = "WAREHOUSE_REPORT_TRANSFERS";
    public const string WarehouseReportTransferById    = "WAREHOUSE_REPORT_TRANSFER_BY_ID";
    public const string WarehouseReportExportTransfers = "WAREHOUSE_REPORT_EXPORT_TRANSFERS";
    public const string WarehouseReportCounts          = "WAREHOUSE_REPORT_COUNTS";
    public const string WarehouseReportCountById       = "WAREHOUSE_REPORT_COUNT_BY_ID";
    public const string WarehouseReportExportCounts    = "WAREHOUSE_REPORT_EXPORT_COUNTS";
    #endregion

    #region SaleCondition
    public const string SaleConditionView       = "SALE_CONDITION_VIEW";
    public const string SaleConditionGetNow     = "SALE_CONDITION_GET_NOW";
    public const string SaleConditionViewDetail = "SALE_CONDITION_VIEW_DETAIL";
    public const string SaleConditionCreate     = "SALE_CONDITION_CREATE";
    public const string SaleConditionDelete     = "SALE_CONDITION_DELETE";
    #endregion

    #region SaleDoc
    public const string SaleDocView             = "SALE_DOC_VIEW";
    public const string SaleDocViewDetail       = "SALE_DOC_VIEW_DETAIL";
    public const string SaleDocCreate           = "SALE_DOC_CREATE";
    public const string SaleDocUpdate           = "SALE_DOC_UPDATE";
    public const string SaleDocAssembly         = "SALE_DOC_WAREHOUSE_CONFIRM";
    public const string ConfirmSale             = "CONFIRM_SALE";
    public const string CancelSale              = "CANCEL_SALE";
    public const string SaleDocDelete           = "SALE_DOC_DELETE";
    #endregion

    #region RetailSaleDoc
    public const string RetailSaleDocView       = "RETAIL_SALE_DOC_VIEW";
    public const string RetailSaleDocViewDetail = "RETAIL_SALE_DOC_VIEW_DETAIL";
    public const string RetailSaleDocCreate     = "RETAIL_SALE_DOC_CREATE";
    public const string RetailSaleDocUpdate     = "RETAIL_SALE_DOC_UPDATE";
    public const string RetailSaleDocDelete     = "RETAIL_SALE_DOC_DELETE";
    public const string ConfirmRetailSale       = "CONFIRM_RETAIL_SALE";
    public const string CancelRetailSale        = "CANCEL_RETAIL_SALE";
    #endregion

    #region SaleDocTable
    public const string SaleDocTableView       = "SALE_DOC_TABLE_VIEW";
    public const string SaleDocTableViewDetail = "SALE_DOC_TABLE_VIEW_DETAIL";
    public const string SaleDocTableCreate     = "SALE_DOC_TABLE_CREATE";
    public const string SaleDocTableUpdate     = "SALE_DOC_TABLE_UPDATE";
    public const string SaleDocTableDelete     = "SALE_DOC_TABLE_DELETE";
    #endregion

    #region AuditLog
    public const string AuditLogView = "AUDIT_LOG_VIEW";
    #endregion

    #region Auth
    public const string AuthCheckToken = "AUTH_CHECK_TOKEN";
    #endregion

    #region Dashboard
    public const string DashboardView = "DASHBOARD_VIEW";
    #endregion

    #region Notifications
    public const string NotificationsGetForCurrentUser = "NOTIFICATIONS_GET_FOR_CURRENT_USER";
    public const string NotificationsGetUnreadCount    = "NOTIFICATIONS_GET_UNREAD_COUNT";
    public const string NotificationsMarkAsRead        = "NOTIFICATIONS_MARK_AS_READ";
    public const string NotificationsMarkAllAsRead     = "NOTIFICATIONS_MARK_ALL_AS_READ";
    #endregion

    #region Role
    public const string RoleView       = "ROLE_VIEW";
    public const string RoleViewDetail = "ROLE_VIEW_DETAIL";
    public const string RoleCreate     = "ROLE_CREATE";
    public const string RoleUpdate     = "ROLE_UPDATE";
    public const string RoleDelete     = "ROLE_DELETE";
    #endregion

    #region Settings
    public const string SettingsGetAll    = "SETTINGS_GET_ALL";
    public const string SettingsGetByCode = "SETTINGS_GET_BY_CODE";
    public const string SettingsUpdate    = "SETTINGS_UPDATE";
    #endregion

    #region User
    public const string UserView       = "USER_VIEW";
    public const string UserViewDetail = "USER_VIEW_DETAIL";
    public const string UserCreate     = "USER_CREATE";
    public const string UserUpdate     = "USER_UPDATE";
    public const string UserDelete     = "USER_DELETE";
    #endregion

    #region OpeningInventory
    public const string OpeningInventoryView       = "OPENING_INVENTORY_VIEW";
    public const string OpeningInventoryViewDetail = "OPENING_INVENTORY_VIEW_DETAIL";
    public const string OpeningInventoryCreate     = "OPENING_INVENTORY_CREATE";
    public const string OpeningInventoryUpdate     = "OPENING_INVENTORY_UPDATE";
    public const string OpeningInventoryDelete     = "OPENING_INVENTORY_DELETE";
    #endregion

    #region HR
    public const string HrView             = "HR_VIEW";
    public const string HrEmployeeView     = "HR_EMPLOYEE_VIEW";
    public const string HrEmployeeCreate   = "HR_EMPLOYEE_CREATE";
    public const string HrEmployeeUpdate   = "HR_EMPLOYEE_UPDATE";
    public const string HrEmployeeDelete   = "HR_EMPLOYEE_DELETE";
    public const string HrScheduleView     = "HR_SCHEDULE_VIEW";
    public const string HrScheduleManage   = "HR_SCHEDULE_MANAGE";
    public const string HrAbsenceView      = "HR_ABSENCE_VIEW";
    public const string HrAbsenceCreate    = "HR_ABSENCE_CREATE";
    public const string HrAbsenceUpdate    = "HR_ABSENCE_UPDATE";
    public const string HrAbsenceDelete    = "HR_ABSENCE_DELETE";
    public const string HrCalendarView     = "HR_CALENDAR_VIEW";
    #endregion

    #region Payroll
    public const string PayrollView             = "PAYROLL_VIEW";
    public const string PayrollEmployeeView     = "PAYROLL_EMPLOYEE_VIEW";
    public const string PayrollEmployeeCreate   = "PAYROLL_EMPLOYEE_CREATE";
    public const string PayrollEmployeeUpdate   = "PAYROLL_EMPLOYEE_UPDATE";
    public const string PayrollEmployeeDelete   = "PAYROLL_EMPLOYEE_DELETE";
    public const string PayrollComponentView    = "PAYROLL_COMPONENT_VIEW";
    public const string PayrollComponentCreate  = "PAYROLL_COMPONENT_CREATE";
    public const string PayrollComponentUpdate  = "PAYROLL_COMPONENT_UPDATE";
    public const string PayrollComponentDelete  = "PAYROLL_COMPONENT_DELETE";
    public const string PayrollPeriodView       = "PAYROLL_PERIOD_VIEW";
    public const string PayrollPeriodManage     = "PAYROLL_PERIOD_MANAGE";
    public const string PayrollTimesheetView    = "PAYROLL_TIMESHEET_VIEW";
    public const string PayrollTimesheetCreate  = "PAYROLL_TIMESHEET_CREATE";
    public const string PayrollTimesheetUpdate  = "PAYROLL_TIMESHEET_UPDATE";
    public const string PayrollTimesheetConfirm = "PAYROLL_TIMESHEET_CONFIRM";
    public const string PayrollTimesheetCancel  = "PAYROLL_TIMESHEET_CANCEL";
    public const string PayrollDocumentView     = "PAYROLL_DOCUMENT_VIEW";
    public const string PayrollDocumentCalculate = "PAYROLL_DOCUMENT_CALCULATE";
    public const string PayrollDocumentConfirm  = "PAYROLL_DOCUMENT_CONFIRM";
    public const string PayrollDocumentCancel   = "PAYROLL_DOCUMENT_CANCEL";
    public const string PayrollDocumentDelete   = "PAYROLL_DOCUMENT_DELETE";
    public const string PayrollPaymentView      = "PAYROLL_PAYMENT_VIEW";
    public const string PayrollPaymentCreate    = "PAYROLL_PAYMENT_CREATE";
    public const string PayrollPaymentConfirm   = "PAYROLL_PAYMENT_CONFIRM";
    public const string PayrollPaymentCancel    = "PAYROLL_PAYMENT_CANCEL";
    public const string PayrollReportView       = "PAYROLL_REPORT_VIEW";
    #endregion
}
