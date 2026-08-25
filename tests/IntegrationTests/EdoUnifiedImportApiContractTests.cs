using Application.Features.Integration.Edo.UnifiedImport;
using WebApi.Controllers.Integration;

public sealed class EdoUnifiedImportApiContractTests
{
    [Fact]
    public void UnifiedRoutesAreDefinedWithoutProviderWriteRoutes()
    {
        var routes = typeof(EdoUnifiedImportController)
            .GetMethods()
            .SelectMany(method => method
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute), true)
                .Cast<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .Select(attribute => attribute.Template)
            .Where(template => template is not null)
            .Select(template => template!)
            .ToArray();

        Assert.Contains("plan", routes);
        Assert.Contains("apply-batch", routes);
        Assert.Contains("{batchId:long}", routes);
        Assert.Contains("refresh-status", routes);
        Assert.DoesNotContain(routes, route => route.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyResponseDoesNotExposeRawProviderPayloadOrMarkingValues()
    {
        var names = typeof(EdoUnifiedImportBatchDocumentDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(names, name => name.Contains("Raw", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, name => name.Equals("MarkingCodes", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("MarkingCount", names);
        Assert.Contains("MarkingVerificationState", names);
        Assert.Contains("SentOverrideApplied", names);
        Assert.Contains("ProviderStatus", names);
    }

    [Fact]
    public void PlanAcceptsRepeatedProviderDocumentIdQuerySelection()
    {
        var parameter = typeof(EdoUnifiedImportController)
            .GetMethod(nameof(EdoUnifiedImportController.GetPlan))!
            .GetParameters()
            .Single(x => x.ParameterType == typeof(string[])
                && x.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromQueryAttribute), true).Length > 0);

        Assert.Equal(typeof(string[]), parameter.ParameterType);
        Assert.NotNull(parameter.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromQueryAttribute), true).SingleOrDefault());

        var sentFlag = typeof(EdoUnifiedImportController)
            .GetMethod(nameof(EdoUnifiedImportController.GetPlan))!
            .GetParameters()
            .Single(x => x.Name == "allowSentDocuments");
        Assert.NotNull(sentFlag.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromQueryAttribute), true).SingleOrDefault());

        var unmatchedFlag = typeof(EdoUnifiedImportController)
            .GetMethod(nameof(EdoUnifiedImportController.GetPlan))!
            .GetParameters()
            .Single(x => x.Name == "allowUnmatchedMarkings");
        Assert.Equal(typeof(bool), unmatchedFlag.ParameterType);
        Assert.True(unmatchedFlag.HasDefaultValue);
        Assert.False((bool)unmatchedFlag.DefaultValue!);
        Assert.NotNull(unmatchedFlag.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromQueryAttribute), true).SingleOrDefault());
    }

    [Fact]
    public void PostPlanAcceptsJsonRequestBodyAndKeepsGetPlan()
    {
        var method = typeof(EdoUnifiedImportController)
            .GetMethod(nameof(EdoUnifiedImportController.PostPlan))!;
        var httpPost = method
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.HttpPostAttribute), true)
            .Cast<Microsoft.AspNetCore.Mvc.HttpPostAttribute>()
            .Single();
        var requestParameter = method.GetParameters()
            .Single(x => x.ParameterType == typeof(EdoUnifiedImportPlanRequestDto));

        Assert.Equal("plan", httpPost.Template);
        Assert.NotNull(requestParameter.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute), true).SingleOrDefault());
        Assert.Contains(
            typeof(EdoUnifiedImportController).GetMethods()
                .SelectMany(x => x.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.HttpGetAttribute), true))
                .Cast<Microsoft.AspNetCore.Mvc.HttpGetAttribute>(),
            attribute => attribute.Template == "plan");
    }

    [Fact]
    public void PlanMappingSnapshotExposesOnlySafeDeterministicSelections()
    {
        var itemNames = typeof(EdoUnifiedImportPlanItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var lineNames = typeof(EdoUnifiedImportPlanLineDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.Contains("CounterpartyId", itemNames);
        Assert.Contains("ContractCandidateIds", itemNames);
        Assert.Contains("CurrencyId", itemNames);
        Assert.Contains("WarehouseId", itemNames);
        Assert.Contains("Lines", itemNames);
        Assert.Contains("ProductId", lineNames);
        Assert.Contains("UnitId", lineNames);
        Assert.Contains("VatRateId", lineNames);
        Assert.Contains("ProductTableIds", lineNames);
        Assert.Contains("MarkingRequired", lineNames);
        Assert.DoesNotContain(lineNames, name => name.Contains("MarkingCode", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(nameof(EdoUnifiedImportPlanDto.AllowUnmatchedMarkings),
            typeof(EdoUnifiedImportPlanDto).GetProperties().Select(property => property.Name));
    }

    [Fact]
    public void PartialMarkingPlanCarriesQuantitySizedProductTableSelectionWithoutRawMarkings()
    {
        var line = new EdoUnifiedImportPlanLineDto
        {
            LineNumber = 1,
            ProductId = 158,
            Quantity = 13,
            MarkingRequired = true,
            ProductTableIds = [2909, 2910, 2911, 2912, 2913, 2914, 2915, 2916, 2917, 2918, 2919, 2920, 2921]
        };

        var json = System.Text.Json.JsonSerializer.Serialize(line);

        Assert.Equal(13, line.ProductTableIds.Count);
        Assert.Contains(2919, line.ProductTableIds);
        Assert.Contains(2920, line.ProductTableIds);
        Assert.Contains(2921, line.ProductTableIds);
        Assert.DoesNotContain("MarkingCodes", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MarkingNumber", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SaleTotalsFailuresExposeDistinctSafeCodes()
    {
        Assert.Equal(
            "SALE_SOURCE_LINE_TOTALS_MISMATCH",
            Application.Features.SaleDocs.EdoSalePreflight.EdoSaleAmountValidation.LineTotalsMismatchCode);
        Assert.Equal(
            "SALE_SOURCE_AGGREGATE_TOTALS_MISMATCH",
            Application.Features.SaleDocs.EdoSalePreflight.EdoSaleAmountValidation.AggregateTotalsMismatchCode);
    }
}
