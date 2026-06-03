namespace Application.Params
{
    public class PagedQueryParams<TEntity> : QueryParams<TEntity>, IPaginationParams
    {
        public int Skip { get; set; }

        public int? Take { get; set; }
    }

    public class PagedQueryParams<TEntity, TResult> : QueryParams<TEntity, TResult>, IPaginationParams
    {
        public int Skip { get; set; }

        public int? Take { get; set; }
    }
}
