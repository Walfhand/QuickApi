using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Abstractions;
using QuickApi.Engine.Web.Endpoints;
using Scrutor;

namespace QuickApi.Engine.Web;

public static class MinimalApiExtensions
{
    public static IServiceCollection AddMinimalEndpoints(this IServiceCollection services, Action<MinimalApiOptions>? configure = null)
    {
        var options = new MinimalApiOptions();
        var callerAssembly = Assembly.GetCallingAssembly();
        configure?.Invoke(options);
        var assembliesToScan = ResolveAssembliesToScan(options, callerAssembly);

        services.Scan(scan =>
        {
            scan.FromAssemblies(assembliesToScan)
                .AddClasses(classes => classes.AssignableTo<IMinimalEndpoint>()
                    .Where(type => !type.IsAbstract))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMinimalEndpoint>()
                .WithLifetime(options.ServiceLifetime);
        });
        
        foreach (var configuration in options.GetConfigurations())
            configuration(services);
        return services;
    }
    
    
    public static IEndpointRouteBuilder UseMinimalEndpoints(this IEndpointRouteBuilder builder)
    {
        using var scope = builder.ServiceProvider.CreateScope();
        var endpoints = scope.ServiceProvider.GetServices<IMinimalEndpoint>();

        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(builder);

        return builder;
    }

    private static IReadOnlyCollection<Assembly> ResolveAssembliesToScan(
        MinimalApiOptions options,
        Assembly callerAssembly)
    {
        var configuredAssemblies = MinimalApiOptionsAssemblyRegistry.GetAssemblies(options);
        if (configuredAssemblies.Count != 0)
            return configuredAssemblies;

        var defaults = new HashSet<Assembly>();
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is not null && !entryAssembly.IsDynamic)
            defaults.Add(entryAssembly);

        if (!callerAssembly.IsDynamic)
            defaults.Add(callerAssembly);

        if (defaults.Count != 0)
            return defaults.ToList().AsReadOnly();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic)
                defaults.Add(assembly);
        }

        return defaults.ToList().AsReadOnly();
    }
}
