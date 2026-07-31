namespace SharedKernel.Constants;

public static class HrTimesheetCategoryConst
{
    public const string Leave = "LEAVE";
    public const string Sick = "SICK";
    public const string Absent = "ABSENT";

    public static readonly string[] All = [Leave, Sick, Absent];
}

public static class HrCalendarStatusConst
{
    public const string NotEmployed = "NOT_EMPLOYED";
    public const string DayOff = "DAY_OFF";
    public const string Worked = "WORKED";
    public const string PlannedWork = "PLANNED_WORK";
}
