using Application.Common.Markers;
using Application.Features.Auth;
using Application.Features.Manual;
using Application.Features.Users;
using Application.Features.Users.Services;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IManualService, ManualService>();

            services.Scan(scan => scan
                .FromAssemblies(typeof(ApplicationAssemblyMarker).Assembly)
                .AddClasses(c => c.AssignableTo(typeof(ICriteriaBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IProjectionBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return services;
        }
    }
}
