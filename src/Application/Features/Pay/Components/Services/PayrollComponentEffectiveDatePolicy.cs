namespace Application.Features.Pay.Components;

/// <summary>Rules for resolving one component code to an unambiguous effective version.</summary>
public static class PayrollComponentEffectiveDatePolicy
{
    public static bool IsValidRange(DateOnly from, DateOnly? to) => !to.HasValue || to.Value >= from;

    public static bool Overlaps(DateOnly leftFrom, DateOnly? leftTo, DateOnly rightFrom, DateOnly? rightTo) =>
        leftFrom <= (rightTo ?? DateOnly.MaxValue) && rightFrom <= (leftTo ?? DateOnly.MaxValue);
}
