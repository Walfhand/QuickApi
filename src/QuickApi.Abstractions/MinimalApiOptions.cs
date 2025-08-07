using Microsoft.Extensions.DependencyInjection;

namespace QuickApi.Abstractions;

public class MinimalApiOptions
{
    private readonly List<Action<IServiceCollection>> _additionalConfigurations = [];

    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Scoped;
    public static string ApiPath { get; set; } = "api";
    
    internal void AddConfiguration(Action<IServiceCollection> configuration)
    {
        _additionalConfigurations.Add(configuration);
    }
    
    public IReadOnlyList<Action<IServiceCollection>> GetConfigurations() => _additionalConfigurations.AsReadOnly();
}