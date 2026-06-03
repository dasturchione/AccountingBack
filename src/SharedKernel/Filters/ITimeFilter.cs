namespace SharedKernel.Filters
{
    /// Represents a filter by a specific temporal value.
    /// </summary>
    /// <typeparam name="T">
    /// Temporal type such as DateOnly, DateTime, DateTimeOffset or TimeOnly.
    /// </typeparam>
    public interface ITimeFilter<T>
    {
        /// <summary>
        /// Exact point in time used for filtering.
        /// </summary>
        T? At { get; }
    }
}
