
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Abstractions;
using QuickApi.Abstractions.Cqrs;
using QuickApi.Extensions.Mediatr.Cqrs;

namespace QuickApi.Extensions.Mediatr;

public static class MinimalApiOptionsExtensions
{
    public static MinimalApiOptions AddMediatR(this MinimalApiOptions options)
    {
        options.AddConfiguration(services =>
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            services.AddScoped<IMessage, MessageService>();
        });

        return options;
    }
}