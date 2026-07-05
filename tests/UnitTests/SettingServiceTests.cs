using Application.Abstractions;
using Application.Features.Settings;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class SettingServiceTests
{
    [Fact]
    public async Task GetValueAsync_Generic_ShouldParseIntegerValue()
    {
        var settings = new List<SystemSetting>
        {
            CreateSetting(1, "MAX_LOGIN_ATTEMPTS", "5", 1, "security")
        };
        var service = CreateService(settings);

        var result = await service.GetValueAsync<int>(" max_login_attempts ");

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task GetValueAsync_Generic_ShouldParseJsonValue()
    {
        var settings = new List<SystemSetting>
        {
            CreateSetting(1, "EMAIL_OPTIONS", "{\"Provider\":\"smtp\",\"Port\":587}", 3, "email")
        };
        var service = CreateService(settings);

        var result = await service.GetValueAsync<EmailOptions>("EMAIL_OPTIONS");

        Assert.True(result.IsSuccess);
        Assert.Equal("smtp", result.Value.Provider);
        Assert.Equal(587, result.Value.Port);
    }

    [Fact]
    public async Task UpdateAsync_ReadOnlySetting_ShouldFail()
    {
        var settings = new List<SystemSetting>
        {
            CreateSetting(1, "EMAIL_PROVIDER", "smtp", 0, "email", isEditable: false)
        };
        var service = CreateService(settings);

        var result = await service.UpdateAsync("EMAIL_PROVIDER", "ses");

        Assert.False(result.IsSuccess);
        Assert.Equal("Setting.ReadOnly", result.Error.Code);
        Assert.Equal("smtp", settings.Single().Value);
    }

    [Fact]
    public async Task GetAllAsync_Category_ShouldReturnOnlyActiveGlobalSettings()
    {
        var settings = new List<SystemSetting>
        {
            CreateSetting(1, "SUPPORT_EMAIL", "support@example.com", 0, "email"),
            CreateSetting(2, "EMAIL_PROVIDER", "smtp", 0, "email", isEditable: false),
            CreateSetting(3, "GENERAL_CURRENCY", "UZS", 0, "general"),
            CreateSetting(4, "EMAIL_PASSIVE", "disabled@example.com", 0, "email", stateId: StateIdConst.PASSIVE),
            CreateSetting(5, "EMAIL_ORG", "org@example.com", 0, "email", organizationId: 77)
        };
        var service = CreateService(settings);

        var result = await service.GetAllAsync(" EMAIL ");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "EMAIL_PROVIDER", "SUPPORT_EMAIL" }, result.Value.Select(x => x.Code).ToArray());
        Assert.All(result.Value, item => Assert.Equal("email", item.Category));
    }

    private static SettingService CreateService(List<SystemSetting> settings, bool hasGlobalAccess = true) =>
        new(
            new FakeUserContext { HasGlobalAccess = hasGlobalAccess },
            new InMemoryQueryRepository<SystemSetting>(settings),
            new InMemoryCommandRepository<SystemSetting>(settings),
            NullLogger<SettingService>.Instance,
            new FakeUnitOfWork());

    private static SystemSetting CreateSetting(
        long id,
        string code,
        string value,
        short valueType,
        string category,
        bool isEditable = true,
        short stateId = StateIdConst.ACTIVE,
        int? organizationId = null) =>
        new()
        {
            Id = id,
            Code = code,
            Value = value,
            ValueType = valueType,
            Category = category,
            Description = $"{code} description",
            IsEditable = isEditable,
            OrganizationId = organizationId,
            StateId = stateId,
            CreatedDate = DateTime.UtcNow
        };

    private sealed record EmailOptions(string Provider, int Port);
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}
