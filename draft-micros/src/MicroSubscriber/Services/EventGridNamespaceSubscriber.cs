using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Messaging.EventGrid.Namespaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicroSubscriber.Handlers;
using MicroSubscriber.Options;

namespace MicroSubscriber.Services;

public interface IEventGridNamespaceSubscriber
{
    Task RunAsync(CancellationToken cancellationToken);
}

public sealed class EventGridNamespaceSubscriber : IEventGridNamespaceSubscriber
{
    private const int DefaultMaxEventsPerReceive = 10;
    private const int DefaultMaxWaitTimeSeconds = 60;

    private readonly EventGridNamespaceOptions _options;
    private readonly IReadOnlyCollection<IEventGridEventHandler> _handlers;
    private readonly TokenCredential? _tokenCredential;
    private readonly ILogger<EventGridNamespaceSubscriber> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public EventGridNamespaceSubscriber(
        IOptions<EventGridNamespaceOptions> options,
        IEnumerable<IEventGridEventHandler> handlers,
        ILogger<EventGridNamespaceSubscriber> logger,
        ILoggerFactory loggerFactory,
        TokenCredential? tokenCredential = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _handlers = handlers?.ToArray() ?? throw new ArgumentNullException(nameof(handlers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _tokenCredential = tokenCredential;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (_options.Subscriptions.Count == 0)
        {
            _logger.LogWarning("No Event Grid namespace subscriptions configured. Background service will idle.");
            await AwaitCancellationAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!Uri.TryCreate(_options.NamespaceEndpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException("Event Grid namespace endpoint is not a valid absolute URI.");
        }

        var listenerTasks = new List<Task>();

        foreach (var subscription in _options.Subscriptions)
        {
            if (string.IsNullOrWhiteSpace(subscription.TopicName) || string.IsNullOrWhiteSpace(subscription.SubscriptionName))
            {
                _logger.LogWarning(
                    "Skipping subscription configuration because topic or subscription name is missing (Name: {Name}).",
                    subscription.Name);
                continue;
            }

            var handlerMap = BuildHandlerMap(subscription);
            if (handlerMap.Count == 0)
            {
                _logger.LogWarning(
                    "No handlers registered for subscription {Subscription}.",
                    subscription.SubscriptionName);
                continue;
            }

            var receiverClient = CreateReceiverClient(endpoint, subscription);
            var listenerLogger = _loggerFactory.CreateLogger<EventGridSubscriptionListener>();
            var listener = new EventGridSubscriptionListener(
                receiverClient,
                subscription,
                handlerMap,
                listenerLogger,
                ResolveMaxEvents(subscription),
                ResolveMaxWaitTime(subscription));

            listenerTasks.Add(listener.ListenAsync(cancellationToken));
        }

        if (listenerTasks.Count == 0)
        {
            _logger.LogWarning("No Event Grid subscription listeners were started. Background service will idle.");
            await AwaitCancellationAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await Task.WhenAll(listenerTasks).ConfigureAwait(false);
    }

    private static async Task AwaitCancellationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when the host is shutting down.
        }
    }

    private EventGridReceiverClient CreateReceiverClient(Uri endpoint, EventGridSubscriptionOptions subscription)
    {
        if (_tokenCredential is not null)
        {
            return new EventGridReceiverClient(endpoint, subscription.TopicName, subscription.SubscriptionName, _tokenCredential);
        }

        var key = _options.Credential.Key;
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Event Grid namespace credential key is not configured.");
        }

        return new EventGridReceiverClient(endpoint, subscription.TopicName, subscription.SubscriptionName, new AzureKeyCredential(key));
    }

    private IReadOnlyDictionary<string, IEventGridEventHandler> BuildHandlerMap(EventGridSubscriptionOptions subscription)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var map = new Dictionary<string, IEventGridEventHandler>(comparer);

        var requestedEventTypes = subscription.EventTypes.Count > 0
            ? new HashSet<string>(subscription.EventTypes, comparer)
            : null;

        foreach (var handler in _handlers)
        {
            if (requestedEventTypes is not null && !requestedEventTypes.Contains(handler.EventName))
            {
                continue;
            }

            if (map.TryGetValue(handler.EventName, out var existingHandler))
            {
                _logger.LogWarning(
                    "Duplicate handlers found for event type {EventType}. Keeping {ExistingHandler} and ignoring {NewHandler}.",
                    handler.EventName,
                    existingHandler.GetType().Name,
                    handler.GetType().Name);
                continue;
            }

            map[handler.EventName] = handler;
        }

        if (requestedEventTypes is not null)
        {
            var missing = requestedEventTypes.Except(map.Keys, comparer).ToArray();
            if (missing.Length > 0)
            {
                _logger.LogWarning(
                    "No handlers registered for event types {EventTypes} on subscription {Subscription}.",
                    string.Join(", ", missing),
                    subscription.SubscriptionName);
            }
        }

        return map;
    }

    private int ResolveMaxEvents(EventGridSubscriptionOptions subscription)
    {
        var candidate = subscription.MaxEventsPerReceive ?? _options.MaxEventsPerReceive ?? DefaultMaxEventsPerReceive;
        return Math.Clamp(candidate, 1, 100);
    }

    private TimeSpan ResolveMaxWaitTime(EventGridSubscriptionOptions subscription)
    {
        var seconds = subscription.MaxWaitTimeSeconds ?? _options.MaxWaitTimeSeconds ?? DefaultMaxWaitTimeSeconds;
        seconds = Math.Clamp(seconds, 10, 120);
        return TimeSpan.FromSeconds(seconds);
    }
}
