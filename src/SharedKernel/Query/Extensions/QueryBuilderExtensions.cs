using SharedKernel.Query.Options;
using SharedKernel.Query.Specifications;

namespace SharedKernel.Query.Extensions
{
    public static class QueryBuilderExtensions
    {
        public static QuerySpecification<TEntity> ById<TEntity>(
            this IQueryBuilder<TEntity> builder, short id)
            where TEntity : class =>
            builder.Build(new GetByIdOptions<short>(id));

        public static QuerySpecification<TEntity> ById<TEntity>(
            this IQueryBuilder<TEntity> builder, int id)
            where TEntity : class =>
            builder.Build(new GetByIdOptions<int>(id));

        public static QuerySpecification<TEntity> ById<TEntity>(
            this IQueryBuilder<TEntity> builder, long id)
            where TEntity : class =>
            builder.Build(new GetByIdOptions<long>(id));

        public static QuerySpecification<TEntity> ById<TEntity>(
            this IQueryBuilder<TEntity> builder, Guid id)
            where TEntity : class =>
            builder.Build(new GetByIdOptions<Guid>(id));

        public static QuerySpecification<TEntity, TResult> ById<TEntity, TResult>(
            this IQueryBuilder<TEntity> builder, short id)
            where TEntity : class =>
            builder.Build<TResult, GetByIdOptions<short>>(new GetByIdOptions<short>(id));

        public static QuerySpecification<TEntity, TResult> ById<TEntity, TResult>(
            this IQueryBuilder<TEntity> builder, int id)
            where TEntity : class =>
            builder.Build<TResult, GetByIdOptions<int>>(new GetByIdOptions<int>(id));

        public static QuerySpecification<TEntity, TResult> ById<TEntity, TResult>(
            this IQueryBuilder<TEntity> builder, long id)
            where TEntity : class =>
            builder.Build<TResult, GetByIdOptions<long>>(new GetByIdOptions<long>(id));

        public static QuerySpecification<TEntity, TResult> ById<TEntity, TResult>(
            this IQueryBuilder<TEntity> builder, Guid id)
            where TEntity : class =>
            builder.Build<TResult, GetByIdOptions<Guid>>(new GetByIdOptions<Guid>(id));
    }
}
