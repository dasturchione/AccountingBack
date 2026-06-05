using Application.Abstractions;
using Application.Abstractions.Authentication;
using Infrastructure.Authentication;
using Infrastructure.Context;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<IRequestContext, RequestContext>();
            services.AddScoped<IUserContext, UserContext>();

            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryBuilderResolver, QueryBuilderResolver>();

            return services;
        }
    }
}
