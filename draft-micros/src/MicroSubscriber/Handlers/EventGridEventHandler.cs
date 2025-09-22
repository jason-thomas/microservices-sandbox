using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;

namespace MicroSubscriber.Handlers;

public abstract class EventGridEventHandler<TEvent> : IEventGridEventHandler<TEvent>
{
    public abstract string EventName { get; }

    public Type PayloadType => typeof(TEvent);

    public Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cloudEvent);

        var payload = ConvertPayload(cloudEvent);
        return HandleAsync(payload, cloudEvent, cancellationToken);
    }

    public abstract Task HandleAsync(TEvent eventData, CloudEvent cloudEvent, CancellationToken cancellationToken);

    protected virtual TEvent ConvertPayload(CloudEvent cloudEvent)
    {
        if (cloudEvent.Data is null)
        {
            throw new InvalidOperationException($"CloudEvent data for '{EventName}' is null.");
        }

        var payload = cloudEvent.Data.ToObjectFromJson<TEvent>();
        if (payload is null)
        {
            throw new InvalidOperationException($"CloudEvent payload for '{EventName}' could not be deserialized to {typeof(TEvent).Name}.");
        }

        return payload;
    }
}
