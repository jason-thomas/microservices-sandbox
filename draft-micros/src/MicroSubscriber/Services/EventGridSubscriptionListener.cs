using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;
using Azure.Messaging.EventGrid.Namespaces;
using Microsoft.Extensions.Logging;
using MicroSubscriber.Handlers;
using MicroSubscriber.Options;

namespace MicroSubscriber.Services;

public sealed class EventGridSubscriptionListener
{
    private readonly EventGridReceiverClient _receiverClient;
    private readonly EventGridSubscriptionOptions _options;
    private readonly IReadOnlyDictionary<string, IEventGridEventHandler> _handlers;
    private readonly ILogger<EventGridSubscriptionListener> _logger;
    private readonly int _maxEventsPerReceive;
    private readonly TimeSpan _maxWaitTime;
    private readonly TimeSpan _retryDelay;

    public EventGridSubscriptionListener(
        EventGridReceiverClient receiverClient,
        EventGridSubscriptionOptions options,
        IReadOnlyDictionary<string, IEventGridEventHandler> handlers,
        ILogger<EventGridSubscriptionListener> logger,
        int maxEventsPerReceive,
        TimeSpan maxWaitTime,
        TimeSpan? retryDelay = null)
    {
        _receiverClient = receiverClient ?? throw new ArgumentNullException(nameof(receiverClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxEventsPerReceive = maxEventsPerReceive;
        _maxWaitTime = maxWaitTime;
        _retryDelay = retryDelay ?? TimeSpan.FromSeconds(5);
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ReceiveResult? receiveResult = await ReceiveAsync(cancellationToken);
                if (receiveResult is null || receiveResult.Details is null || receiveResult.Details.Count == 0)
                {
                    continue;
                }

                var ackTokens = new List<string>();
                var releaseTokens = new List<string>();

                foreach (var detail in receiveResult.Details)
                {
                    var cloudEvent = detail.Event;
                    var lockToken = detail.BrokerProperties?.LockToken;

                    if (cloudEvent is null)
                    {
                        _logger.LogWarning(
                            "Received a null CloudEvent for topic {Topic} subscription {Subscription}.",
                            _options.TopicName,
                            _options.SubscriptionName);
                        if (lockToken is not null)
                        {
                            releaseTokens.Add(lockToken);
                        }

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(cloudEvent.Type))
                    {
                        _logger.LogWarning(
                            "CloudEvent type not found for subscription {Subscription}.",
                            _options.SubscriptionName);
                        if (lockToken is not null)
                        {
                            releaseTokens.Add(lockToken);
                        }

                        continue;
                    }

                    if (!_handlers.TryGetValue(cloudEvent.Type, out var handler))
                    {
                        _logger.LogWarning(
                            "No handler registered for event type {EventType} on subscription {Subscription}.",
                            cloudEvent.Type,
                            _options.SubscriptionName);
                        if (lockToken is not null)
                        {
                            releaseTokens.Add(lockToken);
                        }

                        continue;
                    }

                    try
                    {
                        await handler.HandleAsync(cloudEvent, cancellationToken).ConfigureAwait(false);
                        if (lockToken is not null)
                        {
                            ackTokens.Add(lockToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error handling event type {EventType} on subscription {Subscription}.",
                            cloudEvent.Type,
                            _options.SubscriptionName);
                        if (lockToken is not null)
                        {
                            releaseTokens.Add(lockToken);
                        }
                    }
                }

                await SettleAsync(ackTokens, releaseTokens, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ignore cancellation requested during shutdown.
        }
    }

    private async Task<ReceiveResult?> ReceiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _receiverClient
                .ReceiveAsync(_maxEventsPerReceive, _maxWaitTime, cancellationToken)
                .ConfigureAwait(false);

            return response.Value;
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to receive events for topic {Topic} subscription {Subscription}.",
                _options.TopicName,
                _options.SubscriptionName);

            await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
            return null;
        }
    }

    private async Task SettleAsync(
        List<string> ackTokens,
        List<string> releaseTokens,
        CancellationToken cancellationToken)
    {
        if (ackTokens.Count > 0)
        {
            try
            {
                await _receiverClient.AcknowledgeAsync(ackTokens, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to acknowledge {Count} events for subscription {Subscription}.",
                    ackTokens.Count,
                    _options.SubscriptionName);
            }
        }

        if (releaseTokens.Count > 0)
        {
            try
            {
                await _receiverClient.ReleaseAsync(releaseTokens, null, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to release {Count} events for subscription {Subscription}.",
                    releaseTokens.Count,
                    _options.SubscriptionName);
            }
        }
    }
}

