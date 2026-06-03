namespace SharedKernel.Filters
{
    /// <summary>
     /// Represents pagination parameters for paged queries.
     /// </summary>
    public interface IPaginationFilter
    {
        /// <summary>
        /// Page number starting from 1.
        /// </summary>
        int Page { get; } 

        /// <summary>
        /// Number of items returned per page.
        /// </summary>
        int? PageSize { get; }
    }
}
