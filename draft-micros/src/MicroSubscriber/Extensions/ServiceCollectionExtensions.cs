using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicroSubscriber.Handlers;
using MicroSubscriber.Options;
using MicroSubscriber.Services;

namespace MicroSubscriber.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventGridNamespaceSubscriber(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EventGridNamespaceOptions>()
            .Bind(configuration.GetSection(EventGridNamespaceOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.NamespaceEndpoint),
                "Event Grid namespace endpoint is required.")
            .ValidateOnStart();

        services.AddSingleton<IEventGridNamespaceSubscriber, EventGridNamespaceSubscriber>();
        services.AddHostedService<EventGridNamespaceBackgroundService>();

        return services;
    }

    public static IServiceCollection AddEventGridHandler<TEvent, THandler>(this IServiceCollection services)
        where THandler : class, IEventGridEventHandler<TEvent>
    {
        services.AddSingleton<THandler>();
        services.AddSingleton<IEventGridEventHandler>(sp => sp.GetRequiredService<THandler>());
        services.AddSingleton<IEventGridEventHandler<TEvent>>(sp => sp.GetRequiredService<THandler>());

        return services;
    }
}
