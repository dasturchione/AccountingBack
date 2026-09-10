namespace SharedKernel.Constants;

public static class PayrollPeriodStatusConst
{
    public const string Open = "OPEN";
    public const string Closed = "CLOSED";
}

public static class PayrollComponentTypeConst
{
    public const string Earning = "EARNING";
    public const string Deduction = "DEDUCTION";
    public const string EmployerTax = "EMPLOYER_TAX";
    public const string Reclassification = "RECLASSIFICATION";

    public static readonly string[] All = [Earning, Deduction, EmployerTax, Reclassification];
}

public static class PayrollPeriodDayTypeConst
{
    public const string Normal = "NORMAL";
    public const string Holiday = "HOLIDAY";
    public const string Transferred = "TRANSFERRED";
    public const string Shortened = "SHORTENED";

    public static readonly string[] All = [Normal, Holiday, Transferred, Shortened];
}

public static class PayrollCalculationMethodConst
{
    public const string SalaryProrated = "SALARY_PRORATED";
    public const string Fixed = "FIXED";
    public const string PercentOfGross = "PERCENT_OF_GROSS";
    public const string PerHour = "PER_HOUR";

    public static readonly string[] All = [SalaryProrated, Fixed, PercentOfGross, PerHour];
}

public static class PayrollProrationBasisConst
{
    public const string Days = "DAYS";
    public const string Hours = "HOURS";

    public static readonly string[] All = [Days, Hours];
}

public static class PayrollDocumentKindConst
{
    public const string Regular = "REGULAR";
    public const string Correction = "CORRECTION";
}

public static class PayrollRecalculationStatusConst
{
    public const string Pending = "PENDING";
    public const string Processing = "PROCESSING";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";

    public static readonly string[] All = [Pending, Processing, Completed, Failed];
}

public static class PayrollCorrectionPayoutModeConst
{
    public const string WithSalary = "WITH_SALARY";
    public const string WithAdvance = "WITH_ADVANCE";
    public const string Separate = "SEPARATE";

    public static readonly string[] All = [WithSalary, WithAdvance, Separate];
}

public static class PayrollTaxTypeConst
{
    public const string Withholding = "WITHHOLDING";
    public const string Employer = "EMPLOYER";

    public static readonly string[] All = [Withholding, Employer];
}

public static class PayrollTaxBaseTypeConst
{
    public const string Gross = "GROSS";
    public const string TaxableEarnings = "TAXABLE_EARNINGS";
    public const string Net = "NET";

    public static readonly string[] All = [Gross, TaxableEarnings, Net];
}

public static class PayrollPaymentKindConst
{
    public const string Advance = "ADVANCE";
    public const string Final = "FINAL";
}

public static class PayrollPaymentSourceConst
{
    public const string Bank = "BANK";
    public const string Cash = "CASH";
}

public static class PayrollEmploymentTypeConst
{
    public const string Primary = "PRIMARY";
    public const string PartTime = "PART_TIME";
    public const string Contract = "CONTRACT";

    public static readonly string[] All = [Primary, PartTime, Contract];
}

public static class PayrollAccountRoleCodeConst
{
    public const string SalaryExpense = "salary_expense";
    public const string SalaryPayable = "salary_payable";
    public const string DeductionPayable = "deduction_payable";
    public const string EmployerTaxExpense = "employer_tax_expense";
    public const string EmployerTaxPayable = "employer_tax_payable";
    public const string AdvanceReceivable = "advance_receivable";
}

public static class PayrollDocumentAccountTypeCodeConst
{
    public const string PayrollAccrual = "payroll_accrual";
}
