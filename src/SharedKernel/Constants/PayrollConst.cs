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

public static class PayrollCalculationMethodConst
{
    public const string SalaryProrated = "SALARY_PRORATED";
    public const string Fixed = "FIXED";
    public const string PercentOfGross = "PERCENT_OF_GROSS";
    public const string PerHour = "PER_HOUR";

    public static readonly string[] All = [SalaryProrated, Fixed, PercentOfGross, PerHour];
}

public static class PayrollDocumentKindConst
{
    public const string Regular = "REGULAR";
    public const string Correction = "CORRECTION";
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
