namespace SharedKernel.Filters
{
    /// <summary>
    /// Represents a text-based search filter.
    /// </summary>
    public interface ISearchFilter
    {
        /// <summary>
        /// Search text used for filtering results.
        /// </summary>
        string? Search { get; }
    }
}
