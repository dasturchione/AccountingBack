using Application;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace UnitTests;

public sealed class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersEachQueryBuilderExactlyOnce()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var queryBuilders = services
            .Where(descriptor => IsQueryBuilder(descriptor.ServiceType))
            .ToArray();

        Assert.NotEmpty(queryBuilders);
        Assert.All(queryBuilders, descriptor => Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime));
        Assert.Equal(
            queryBuilders.Length,
            queryBuilders.Select(descriptor => descriptor.ServiceType).Distinct().Count());
    }

    private static bool IsQueryBuilder(Type serviceType)
    {
        if (!serviceType.IsGenericType)
            return false;

        var genericType = serviceType.GetGenericTypeDefinition();
        return genericType == typeof(ICriteriaBuilder<,>)
            || genericType == typeof(IProjectionBuilder<,>)
            || genericType == typeof(IOrderByBuilder<,>);
    }
}
