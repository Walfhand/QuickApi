using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Abstractions;
using QuickApi.Abstractions.Cqrs;
using QuickApi.Extensions.Mediatr.Cqrs;

namespace QuickApi.Extensions.Mediatr;

public static class MinimalApiOptionsExtensions
{
    public static MinimalApiOptions AddMediatR(this MinimalApiOptions options)
        => options.AddMediatR(Array.Empty<Assembly>());

    public static MinimalApiOptions AddMediatR(this MinimalApiOptions options, params Assembly[] assemblies)
    {
        var callerAssembly = Assembly.GetCallingAssembly();
        options.AddConfiguration(services =>
        {
            var assembliesToScan = ResolveAssembliesToScan(options, callerAssembly, assemblies);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembliesToScan.ToArray()));
            services.AddScoped<IMessage, MessageService>();
        });

        return options;
    }

    private static IReadOnlyCollection<Assembly> ResolveAssembliesToScan(
        MinimalApiOptions _,
        Assembly callerAssembly,
        IReadOnlyCollection<Assembly> providedAssemblies)
    {
        if (providedAssemblies.Count != 0)
            return providedAssemblies.Where(a => !a.IsDynamic).Distinct().ToList().AsReadOnly();

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
