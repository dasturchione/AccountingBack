namespace Infrastructure.Repositories;

/// <summary>
/// acc_reg_entry.doc_date is a timestamp and documents keep whatever time of day they
/// were saved with (18:05:43, 14:59:15, ...). A report bound of "30.06.2026" therefore has
/// to cover the whole day: comparing with <c>&lt;=</c> against 30.06.2026 00:00 silently
/// dropped every entry posted later on that day, so the closing balance of the last day in
/// a range came out wrong. Register read repositories normalise both ends through these
/// helpers, turning a range into [start of DateFrom, start of the day after DateTo).
/// </summary>
internal static class RegisterDateRange
{
    /// <summary>Inclusive lower bound: midnight of the requested day.</summary>
    public static DateTime InclusiveStart(DateTime value) => value.Date;

    /// <summary>Exclusive upper bound: midnight of the day after the requested day.</summary>
    public static DateTime ExclusiveEnd(DateTime value) => value.Date.AddDays(1);
}
