namespace SharedKernel.Constants;

public enum AccountingPolicySourceStatus
{
    CONFIRMED_FROM_PROJECT,
    CONFIRMED_FROM_LIVE_DATA,
    PARTIAL,
    NOT_CONFIGURED,
    MISSING,
    NEEDS_BUSINESS_DECISION,
    BLOCKED,
    AVAILABLE,
    NOT_AVAILABLE,
    OUT_OF_SCOPE
}

public static class AccountingPolicySourceStatusConst
{
    public const string CONFIRMED_FROM_PROJECT = nameof(AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT);
    public const string CONFIRMED_FROM_LIVE_DATA = nameof(AccountingPolicySourceStatus.CONFIRMED_FROM_LIVE_DATA);
    public const string PARTIAL = nameof(AccountingPolicySourceStatus.PARTIAL);
    public const string NOT_CONFIGURED = nameof(AccountingPolicySourceStatus.NOT_CONFIGURED);
    public const string MISSING = nameof(AccountingPolicySourceStatus.MISSING);
    public const string NEEDS_BUSINESS_DECISION = nameof(AccountingPolicySourceStatus.NEEDS_BUSINESS_DECISION);
    public const string BLOCKED = nameof(AccountingPolicySourceStatus.BLOCKED);
    public const string AVAILABLE = nameof(AccountingPolicySourceStatus.AVAILABLE);
    public const string NOT_AVAILABLE = nameof(AccountingPolicySourceStatus.NOT_AVAILABLE);
    public const string OUT_OF_SCOPE = nameof(AccountingPolicySourceStatus.OUT_OF_SCOPE);
}

public static class AccountingPolicyVatTaxPeriodConst
{
    public const string MONTH = "MONTH";
}

public static class AccountingPolicyVatBaseMomentConst
{
    public const string SHIPMENT = "SHIPMENT";
}

public static class AccountingPolicyClosedPeriodPolicyConst
{
    public const string PROTECT_CLOSED_PERIOD = "PROTECT_CLOSED_PERIOD";
}

public static class AccountingPolicyErrorCodeConst
{
    public const string INVALID_VALUATION_METHOD = "ACCOUNTING_POLICY_INVALID_VALUATION_METHOD";
    public const string INVALID_BASE_CURRENCY = "ACCOUNTING_POLICY_INVALID_BASE_CURRENCY";
    public const string INVALID_VAT_PERIOD = "ACCOUNTING_POLICY_INVALID_VAT_PERIOD";
    public const string INVALID_VAT_BASE_MOMENT = "ACCOUNTING_POLICY_INVALID_VAT_BASE_MOMENT";
    public const string TAX_TYPE_REQUIRED = "ACCOUNTING_POLICY_TAX_TYPE_REQUIRED";
    public const string OUT_OF_SCOPE = "ACCOUNTING_POLICY_OUT_OF_SCOPE";
    public const string POLICY_VERSION_OVERLAP = "ACCOUNTING_POLICY_VERSION_OVERLAP";
    public const string CLOSED_PERIOD_PROTECTED = "ACCOUNTING_POLICY_CLOSED_PERIOD_PROTECTED";
    public const string ORGANIZATION_SCOPE_REQUIRED = "ACCOUNTING_POLICY_ORGANIZATION_SCOPE_REQUIRED";
    public const string INVALID_EFFECTIVE_DATE = "ACCOUNTING_POLICY_INVALID_EFFECTIVE_DATE";
    public const string USER_SCOPE_REQUIRED = "ACCOUNTING_POLICY_USER_SCOPE_REQUIRED";
    public const string INVALID_CLOSED_PERIOD_POLICY = "ACCOUNTING_POLICY_INVALID_CLOSED_PERIOD_POLICY";
}
