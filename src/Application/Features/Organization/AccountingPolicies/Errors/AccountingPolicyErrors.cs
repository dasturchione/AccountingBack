using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.AccountingPolicies.Errors;

public static class AccountingPolicyErrors
{
    public static Error InvalidValuationMethod() =>
        Error.Validation(AccountingPolicyErrorCodeConst.INVALID_VALUATION_METHOD, "The accounting policy valuation method is invalid.");

    public static Error InvalidBaseCurrency() =>
        Error.Validation(AccountingPolicyErrorCodeConst.INVALID_BASE_CURRENCY, "The accounting policy base currency is invalid.");

    public static Error InvalidVatPeriod() =>
        Error.Validation(AccountingPolicyErrorCodeConst.INVALID_VAT_PERIOD, "The accounting policy VAT period is invalid.");

    public static Error InvalidVatBaseMoment() =>
        Error.Validation(AccountingPolicyErrorCodeConst.INVALID_VAT_BASE_MOMENT, "The accounting policy VAT base moment is invalid.");

    public static Error TaxTypeRequired() =>
        Error.Business(AccountingPolicyErrorCodeConst.TAX_TYPE_REQUIRED, "A tax type is required before the policy can be activated.");

    public static Error OutOfScope() =>
        Error.Business(AccountingPolicyErrorCodeConst.OUT_OF_SCOPE, "The requested accounting policy capability is outside the approved scope.");

    public static Error PolicyVersionOverlap() =>
        Error.Conflict(AccountingPolicyErrorCodeConst.POLICY_VERSION_OVERLAP, "The accounting policy effective period overlaps an existing version.");

    public static Error ClosedPeriodProtected() =>
        Error.Conflict(AccountingPolicyErrorCodeConst.CLOSED_PERIOD_PROTECTED, "The accounting policy update affects a closed accounting period.");

    public static Error OrganizationScopeRequired() =>
        Error.Validation(AccountingPolicyErrorCodeConst.ORGANIZATION_SCOPE_REQUIRED, "An authenticated organization scope is required.");

    public static Error InvalidEffectiveDate() =>
        Error.Validation(AccountingPolicyErrorCodeConst.INVALID_EFFECTIVE_DATE, "The accounting policy effective interval is invalid.");

    public static Error UserScopeRequired() =>
        Error.Validation(AccountingPolicyErrorCodeConst.USER_SCOPE_REQUIRED, "An authenticated user scope is required for policy history.");

    public static Error InvalidClosedPeriodPolicy() =>
        Error.Validation(AccountingPolicyErrorCodeConst.INVALID_CLOSED_PERIOD_POLICY, "The accounting policy must protect closed accounting periods.");
}
