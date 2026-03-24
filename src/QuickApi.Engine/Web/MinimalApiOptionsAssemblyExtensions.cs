using System.Reflection;
using System.Runtime.CompilerServices;
using QuickApi.Abstractions;

namespace QuickApi.Engine.Web;

public static class MinimalApiOptionsAssemblyExtensions
{
    public static MinimalApiOptions AddAssembly(this MinimalApiOptions options, Assembly assembly)
    {
        if (!assembly.IsDynamic)
            MinimalApiOptionsAssemblyRegistry.Add(options, assembly);
        return options;
    }

    public static MinimalApiOptions AddAssemblies(this MinimalApiOptions options, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
            options.AddAssembly(assembly);
        return options;
    }
}

internal static class MinimalApiOptionsAssemblyRegistry
{
    private static readonly ConditionalWeakTable<MinimalApiOptions, HashSet<Assembly>> Store = new();

    public static void Add(MinimalApiOptions options, Assembly assembly)
    {
        var assemblies = Store.GetOrCreateValue(options);
        assemblies.Add(assembly);
    }

    public static IReadOnlyCollection<Assembly> GetAssemblies(MinimalApiOptions options)
    {
        if (!Store.TryGetValue(options, out var assemblies) || assemblies.Count == 0)
            return [];

        return assemblies.ToList().AsReadOnly();
    }
}
