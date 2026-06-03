namespace SharedKernel.Filters
{
    /// <summary>
    /// Represents a filter for a temporal range of values.
    /// </summary>
    /// <typeparam name="T">
    /// Temporal type such as DateOnly, DateTime, DateTimeOffset or TimeOnly.
    /// </typeparam>
    public interface ITimeRangeFilter<T>
    {
        /// <summary>
        /// Start value of the range (inclusive or business-defined).
        /// </summary>
        T? From { get; }

        /// <summary>
        /// End value of the range (inclusive or business-defined).
        /// </summary>
        T? To { get; }
    }
}
