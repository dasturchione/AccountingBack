using System.Reflection;
using System.Text.Json;
using Application.Features.Dashboard.DTOs;
using Application.Features.Dashboard.Services;

namespace UnitTests;

public sealed class DashboardReadOnlyApiTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Application.Abstractions.IQueryRepository<>).Assembly;

    [Fact]
    public void Task_calendar_has_explicit_unavailable_contract()
    {
        var dtoType = ApplicationAssembly.GetType(
            "Application.Features.Dashboard.DTOs.TaskCalendarDto");

        Assert.NotNull(dtoType);
        var dto = Activator.CreateInstance(dtoType!);
        Assert.NotNull(dto);

        Assert.Equal("NOT_AVAILABLE", dtoType!.GetProperty("SourceStatus")!.GetValue(dto));
        Assert.Equal(0, dtoType.GetProperty("Total")!.GetValue(dto));
        Assert.Equal(0, dtoType.GetProperty("Completed")!.GetValue(dto));
        Assert.Equal(0, dtoType.GetProperty("Pending")!.GetValue(dto));
        Assert.Equal(0, dtoType.GetProperty("Overdue")!.GetValue(dto));
        Assert.Empty((System.Collections.IEnumerable)dtoType.GetProperty("Items")!.GetValue(dto)!);
    }

    [Fact]
    public void Dashboard_filter_exposes_all_requested_filters()
    {
        var dtoType = ApplicationAssembly.GetType(
            "Application.Features.Dashboard.DTOs.DashboardFilterDto");

        Assert.NotNull(dtoType);
        Assert.NotNull(dtoType!.GetProperty("DateFrom"));
        Assert.NotNull(dtoType.GetProperty("DateTo"));
        Assert.NotNull(dtoType.GetProperty("CurrencyIds"));
        Assert.NotNull(dtoType.GetProperty("WarehouseIds"));
        Assert.NotNull(dtoType.GetProperty("DocumentTypes"));
        Assert.NotNull(dtoType.GetProperty("StatusIds"));
    }

    [Fact]
    public void Aggregate_contract_contains_all_dashboard_sections()
    {
        var dtoType = ApplicationAssembly.GetType(
            "Application.Features.Dashboard.DTOs.DashboardOverviewDto");

        Assert.NotNull(dtoType);
        foreach (var property in new[]
        {
            "Filters", "Cash", "Relationships", "Tasks", "Receivables",
            "Payables", "Tax", "ElectronicDocuments"
        })
            Assert.NotNull(dtoType!.GetProperty(property));
    }

    [Fact]
    public void Task_calendar_unavailable_payload_matches_frontend_contract()
    {
        var payload = JsonSerializer.Serialize(new
        {
            sourceStatus = "NOT_AVAILABLE",
            total = 0,
            completed = 0,
            pending = 0,
            overdue = 0,
            items = Array.Empty<object>()
        });

        using var json = JsonDocument.Parse(payload);
        Assert.Equal("NOT_AVAILABLE", json.RootElement.GetProperty("sourceStatus").GetString());
        Assert.Equal(0, json.RootElement.GetProperty("total").GetInt32());
        Assert.Empty(json.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task Task_calendar_service_returns_only_unavailable_empty_data()
    {
        var response = await new TaskCalendarService().GetAsync(new DashboardFilterDto());

        Assert.Equal("NOT_AVAILABLE", response.SourceStatus);
        Assert.Equal(0, response.Total);
        Assert.Equal(0, response.Completed);
        Assert.Equal(0, response.Pending);
        Assert.Equal(0, response.Overdue);
        Assert.Empty(response.Items);
    }

    [Fact]
    public void Dashboard_filters_preserve_requested_filter_values()
    {
        var filter = new DashboardFilterDto
        {
            DateFrom = new DateTime(2026, 1, 1),
            DateTo = new DateTime(2026, 1, 31),
            CurrencyIds = [1, 2],
            WarehouseIds = [1],
            DocumentTypes = ["SALE"],
            StatusIds = [3]
        };

        Assert.True(filter.HasCurrencyFilter);
        Assert.True(filter.HasWarehouseFilter);
        Assert.True(filter.HasDocumentTypeFilter);
        Assert.True(filter.HasStatusFilter);
        Assert.True(filter.IncludesDocumentType("sale"));
        Assert.False(filter.IncludesDocumentType("purchase"));
    }

    [Fact]
    public void Dashboard_response_contract_does_not_expose_marking_values()
    {
        var dashboardTypes = new[]
        {
            typeof(DashboardOverviewDto),
            typeof(DashboardCashDto),
            typeof(DashboardRelationshipsDto),
            typeof(DashboardReceivablesPayablesDto),
            typeof(DashboardDebtDto),
            typeof(DashboardTaxSummaryDto),
            typeof(DashboardElectronicDocumentsDto)
        };

        Assert.DoesNotContain(
            dashboardTypes.SelectMany(type => type.GetProperties()),
            property => property.Name.Contains("Marking", StringComparison.OrdinalIgnoreCase));
    }
}
