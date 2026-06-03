namespace SharedKernel.Filters
{
    /// <summary>
    /// Represents sorting parameters for queries.
    /// </summary>
    public interface ISortFilter
    {
        /// <summary>
        /// Field name used for sorting.
        /// </summary>
        string? SortBy { get; }

        /// <summary>
        /// Direction of sorting.
        /// </summary>
        SortDirection SortDirection { get; }
    }

    /// <summary>
    /// Defines available sorting directions.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Sorts values in ascending order.
        /// </summary>
        Asc = 0,

        /// <summary>
        /// Sorts values in descending order.
        /// </summary>
        Desc = 1
    }
}
