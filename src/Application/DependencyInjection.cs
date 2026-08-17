using Application.Common.Markers;
using Application.Features.Cmn.Currencies.Extensions;
using Application.Features.Cmn.CurrencyRates.Extensions;
using Application.Features.Cmn.CurrencyRevaluations.Extensions;
using Application.Features.Cmn.Taxes.Extensions;
using Application.Features.Imports;
using Application.Features.Notifications.Extensions;
using Application.Features.Reports.Contracts;
using Application.Features.Reports.Extensions;
using Application.Features.Settings.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using SharedKernel.Query;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>(includeInternalTypes: true);

        services.Scan(scan => scan
            .FromAssemblyOf<ApplicationAssemblyMarker>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICriteriaBuilder<,>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IProjectionBuilder<,>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IOrderByBuilder<,>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IReportQuery<,>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IReportBuilder<,>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IReportValidator<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsSelf()
                .WithScopedLifetime());

        services.AddReportsModule();
        services.AddImportModule();
        services.AddCurrencyModule();
        services.AddCurrencyRateModule();
        services.AddCurrencyRevaluationModule();
        services.AddTaxModule();
        services.AddSettingsModule();
        services.AddNotificationsModule();

        return services;
    }
}
