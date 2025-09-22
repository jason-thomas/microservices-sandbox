using System;
using Azure;
using Azure.Messaging.EventGrid;
using Draft.Worldpay.Parser.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var configuration = builder.Configuration;

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddOptions<EventGridPublisherOptions>()
    .Bind(configuration.GetSection(EventGridPublisherOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.TopicEndpoint), "EventGrid:TopicEndpoint is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "EventGrid:AccessKey is required.")
    .Validate(options => options.BatchSize > 0 && options.BatchSize <= 100, "EventGrid:BatchSize must be between 1 and 100.")
    .ValidateOnStart();

builder.Services.AddSingleton<EventGridPublisherClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<EventGridPublisherOptions>>().Value;

    return new EventGridPublisherClient(
        new Uri(options.TopicEndpoint!, UriKind.Absolute),
        new AzureKeyCredential(options.AccessKey!));
});

builder.Build().Run();
