using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.PostingTemplateViews;
using Infrastructure.Authentication;
using Infrastructure.Context;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Integration.Faktura.Configs;
using Integration.GoogleDrive.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager config)
        {
            services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<IRequestContext, RequestContext>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IPermissionChecker, PermissionChecker>();
            services.AddScoped<IPostingTemplateViewService, PostingTemplateViewService>();

            services.AddScoped<IDocNumberGenerator, DocNumberGenerator>();
            services.AddScoped<IProductTableReservationService, ProductTableReservationService>();

            services.AddScoped<IQueryBuilder, QueryBuilder>();
            services.AddScoped<IQueryBuilderResolver, QueryBuilderResolver>();

            services.AddMemoryCache();

            services.AddFaktura(config);
            services.AddGoogleDriveIntegration(config);

            return services;
        }
    }
}
