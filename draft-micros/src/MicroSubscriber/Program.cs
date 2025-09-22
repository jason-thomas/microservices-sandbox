using Microsoft.Extensions.Hosting;
using MicroSubscriber.Events;
using MicroSubscriber.Extensions;
using MicroSubscriber.Handlers.Implementations;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddEventGridNamespaceSubscriber(builder.Configuration)
    .AddEventGridHandler<UserRegisteredEvent, UserRegisteredEventHandler>()
    .AddEventGridHandler<OrderCancelledEvent, OrderCancelledEventHandler>();

var host = builder.Build();

await host.RunAsync();
