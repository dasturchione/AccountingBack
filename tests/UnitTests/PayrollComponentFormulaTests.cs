using Application.Features.Pay.Components;
using Domain.Entities;

namespace UnitTests;

public sealed class PayrollComponentFormulaTests
{
    [Fact]
    public void Orders_components_by_dependency_before_sort_order()
    {
        var baseComponent = new PayComponent { Id = 10, Code = "SALARY", SortOrder = 20 };
        var bonus = new PayComponent { Id = 20, Code = "BONUS", SortOrder = 1, DependsOnComponentId = 10 };

        var ordered = PayrollComponentFormulaPolicy.Order([bonus, baseComponent]);

        Assert.Equal([10, 20], ordered.Select(x => x.Id));
    }

    [Fact]
    public void Rejects_dependency_cycles()
    {
        var first = new PayComponent { Id = 1, Code = "A", DependsOnComponentId = 2 };
        var second = new PayComponent { Id = 2, Code = "B", DependsOnComponentId = 1 };

        Assert.Throws<InvalidOperationException>(() => PayrollComponentFormulaPolicy.Order([first, second]));
    }

    [Fact]
    public void Applies_floor_and_cap_after_formula_calculation()
    {
        Assert.Equal(100m, PayrollComponentFormulaPolicy.ApplyCaps(40m, 100m, 200m));
        Assert.Equal(200m, PayrollComponentFormulaPolicy.ApplyCaps(260m, 100m, 200m));
        Assert.Equal(75m, PayrollComponentFormulaPolicy.ApplyCaps(75m, null, null));
    }
}
