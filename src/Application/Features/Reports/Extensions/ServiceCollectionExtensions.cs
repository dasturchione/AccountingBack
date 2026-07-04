using Application.Features.Reports.BankReports;
using Application.Features.Reports.CashReports;
using Application.Features.Reports.FinancialReports;
using Application.Features.Reports.PayableReports;
using Application.Features.Reports.PurchaseReports;
using Application.Features.Reports.ReceivableReports;
using Application.Features.Reports.SalesReports;
using Application.Features.Reports.WarehouseReports;
using Application.Features.Reports.Exports;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Application.Features.Reports.Extensions;

/// <summary>
/// Registers the Reports feature foundation.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds report module services and abstractions.
    /// </summary>
    public static IServiceCollection AddReportsModule(this IServiceCollection services)
    {
        services.AddScoped<IExcelExporter, ExcelReportExporter>();
        services.AddScoped<IPdfExporter, PdfReportExporter>();
        services.AddScoped<IReportExporter, ReportExporter>();

        services.Scan(scan => scan
            .FromAssemblyOf<ReportsFeatureRegistration>()
            .AddClasses(c => c.AssignableTo(typeof(Application.Features.Reports.Contracts.IReportQuery<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(Application.Features.Reports.Contracts.IReportBuilder<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(Application.Features.Reports.Contracts.IReportValidator<>)))
                .AsSelf()
                .WithScopedLifetime());

        return services;
    }
}
