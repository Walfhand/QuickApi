using FluentAssertions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Abstractions;
using QuickApi.Engine.Web;
using QuickApi.Engine.Web.Endpoints;
using QuickApi.Tests.Unit.Web.Fakes;

namespace QuickApi.Tests.Unit.Web;

public class MinimalApiOptionsTests
{
    [Fact]
    public void SetBaseApiPath_ShouldUpdateProvider()
    {
        var original = ApiPathProvider.BaseApiPath;
        var options = new MinimalApiOptions();
        try
        {
            options.SetBaseApiPath("api/v2");
            ApiPathProvider.BaseApiPath.Should().Be("api/v2");
        }
        finally
        {
            ApiPathProvider.BaseApiPath = original;
        }
    }

    [Fact]
    public void AddConfiguration_ShouldRegisterCustomServices()
    {
        var services = new ServiceCollection();
        var addConfiguration = typeof(MinimalApiOptions).GetMethod("AddConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
        addConfiguration.Should().NotBeNull("AddConfiguration is internal but expected to be invokable via reflection for extensibility validation");

        services.AddMinimalEndpoints(options =>
        {
            addConfiguration.Invoke(options, [
                (Action<IServiceCollection>)(s => s.AddSingleton<ITestService, TestService>())
            ]);
        });

        var provider = services.BuildServiceProvider();
        provider.GetService<ITestService>().Should().NotBeNull();
    }

    [Fact]
    public void AddMinimalEndpoints_ShouldRespectConfiguredLifetime()
    {
        var services = new ServiceCollection();

        services.AddMinimalEndpoints(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Singleton;
        });

        var descriptor = services.First(d =>
            d.ServiceType == typeof(IMinimalEndpoint) &&
            d.ImplementationType == typeof(DummyEndpoint));

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    private interface ITestService;
    private class TestService : ITestService;
}
