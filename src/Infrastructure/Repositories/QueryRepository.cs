using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Query.Includes;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class QueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly AppDbContext _context;
        public QueryRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(predicate);
            return await query.AnyAsync(ct);
        }

        public async Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            var query = specification.IgnoreQueryFilters
                ? _dbSet.IgnoreQueryFilters().AsNoTracking().AsQueryable()
                : _dbSet.AsNoTracking().AsQueryable();

            if (specification.Includes.Any())
                query = ApplyIncludes(query, specification.Includes);

            if (specification.Criteria is not null)
                query = query.Where(specification.Criteria);

            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);

            return await query.ToListAsync(ct);
        }

        public async Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            var baseQuery = specification.IgnoreQueryFilters
                ? _dbSet.IgnoreQueryFilters().AsNoTracking()
                : _dbSet.AsNoTracking();

            var query = baseQuery
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .AsQueryable();

            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);

            if (specification.ResultCriteria is not null)
                query = query.Where(specification.ResultCriteria);

            return await query.ToListAsync(ct);
        }

        public async Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            var query = specification.IgnoreQueryFilters
                ? _dbSet.IgnoreQueryFilters().AsQueryable()
                : _dbSet.AsQueryable();

            if (specification.Includes.Any())
                query = ApplyIncludes(query, specification.Includes);

            if (specification.Criteria is not null)
                query = query.Where(specification.Criteria);

            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            var baseQuery = specification.IgnoreQueryFilters
                ? _dbSet.IgnoreQueryFilters().AsNoTracking()
                : _dbSet.AsNoTracking();

            var query = baseQuery
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .AsQueryable();

            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            var query = specification.IgnoreQueryFilters
                ? _dbSet.IgnoreQueryFilters().AsNoTracking().AsQueryable()
                : _dbSet.AsNoTracking().AsQueryable();

            if (specification.Includes.Any())
                query = ApplyIncludes(query, specification.Includes);

            if (specification.Criteria is not null)
                query = query.Where(specification.Criteria);

            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);

            var totalCount = await query.CountAsync(ct);

            if (specification.Take.HasValue)
                query = query.Skip(specification.Skip).Take(specification.Take.Value);

            var items = await query.ToListAsync(ct);

            return new PagedList<TEntity>(items, totalCount);
        }

        public async Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            var baseQuery = specification.IgnoreQueryFilters
                ? _dbSet.IgnoreQueryFilters().AsNoTracking()
                : _dbSet.AsNoTracking();

            var query = baseQuery
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .AsQueryable();

            if (specification.ResultCriteria is not null)
                query = query.Where(specification.ResultCriteria);

            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);

            var totalCount = await query.CountAsync(ct);

            if (specification.Take.HasValue)
                query = query.Skip(specification.Skip).Take(specification.Take.Value);

            var items = await query.ToListAsync(ct);

            return new PagedList<TResult>(items, totalCount);
        }

        #region Apply Includes
        private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, IReadOnlyList<IncludeEntry<TEntity>> includes)
        {
            var efType = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions);

            foreach (var include in includes)
            {
                var includeExpr = include.NavigationExpression;

                var includeMethod = efType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .First(m => m.Name == "Include" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

                var genericInclude = includeMethod.MakeGenericMethod(typeof(TEntity), includeExpr.ReturnType);
                var current = genericInclude.Invoke(null, new object[] { query, includeExpr });

                var next = include.Next;
                // Walk ThenIncludes and try appropriate overloads for collection vs reference navigations
                while (next is not null)
                {
                    var thenExpr = next.NavigationExpression;

                    var thenCandidates = efType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "ThenInclude" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2)
                        .ToArray();

                    var invoked = false;

                    foreach (var candidate in thenCandidates)
                    {
                        var genArgs = candidate.GetGenericArguments().Length;

                        try
                        {
                            // Try to infer generic arguments: (TEntity, TPrevious, TNext)
                            Type prevType = includeExpr.ReturnType;

                            // If previous is a collection, extract the element type for ThenInclude's TPrevious generic
                            if (prevType != typeof(string))
                            {
                                var ienum = prevType.GetInterfaces()
                                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                                if (ienum is not null)
                                    prevType = ienum.GetGenericArguments()[0];
                            }

                            var genericThen = candidate.MakeGenericMethod(typeof(TEntity), prevType, thenExpr.ReturnType);
                            current = genericThen.Invoke(null, new object[] { current!, thenExpr });
                            invoked = true;
                            break;
                        }
                        catch
                        {
                            // try next candidate
                        }
                    }

                    if (!invoked)
                        break;

                    includeExpr = thenExpr;
                    next = next.Next;
                }

                if (current is IQueryable<TEntity> q)
                    query = q;
            }

            return query;
        }

        private static string BuildPath(IncludeEntry<TEntity>? include)
        {
            var parts = new List<string>();

            while (include is not null)
            {
                parts.Add(GetMemberName(include.NavigationExpression));

                include = include.Next;
            }

            return string.Join(".", parts);
        }

        private static string GetMemberName(LambdaExpression expression)
        {
            Expression body = expression.Body;

            if (body is UnaryExpression unary)
                body = unary.Operand;

            return ((MemberExpression)body).Member.Name;
        }
        #endregion
    }
}
