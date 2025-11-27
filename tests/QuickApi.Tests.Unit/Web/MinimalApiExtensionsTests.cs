using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Engine.Web;
using QuickApi.Engine.Web.Endpoints;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using QuickApi.Tests.Unit.Web.Fakes;

namespace QuickApi.Tests.Unit.Web;

public class MinimalApiExtensionsTests
{
    [Fact]
    public void AddMinimalEndpoints_ShouldRegisterEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddMinimalEndpoints();
        var provider = services.BuildServiceProvider();
        
        // Assert
        var endpoints = provider.GetServices<IMinimalEndpoint>();
        endpoints.Should().ContainSingle(e => e is DummyEndpoint);
    }
    
    [Fact]
    public void AddMinimalEndpoints_WithCustomOptions_ShouldApplyConfigurations()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationApplied = false;
        
        // Act
        services.AddMinimalEndpoints(options =>
        {
            configurationApplied = true;
        });
        
        // Assert
        configurationApplied.Should().BeTrue();
    }

    [Fact]
    public void UseMinimalEndpoints_ShouldMapAllRegisteredEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMinimalEndpoint, DummyEndpoint>();
        var endpoint1 = Substitute.For<IMinimalEndpoint>();
        var endpoint2 = Substitute.For<IMinimalEndpoint>();
        
        services.AddScoped<IMinimalEndpoint>(_ => endpoint1);
        services.AddScoped<IMinimalEndpoint>(_ => endpoint2);
        
        var serviceProvider = services.BuildServiceProvider();
        var builder = Substitute.For<IEndpointRouteBuilder>();
        builder.ServiceProvider.Returns(serviceProvider);
        
        // Act
        builder.UseMinimalEndpoints();
        
        // Assert
        endpoint1.Received(1).MapEndpoint(builder);
        endpoint2.Received(1).MapEndpoint(builder);
    }
}
