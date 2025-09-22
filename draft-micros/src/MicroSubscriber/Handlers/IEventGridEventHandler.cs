using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;

namespace MicroSubscriber.Handlers;

public interface IEventGridEventHandler
{
    string EventName { get; }

    Type PayloadType { get; }

    Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken);
}

public interface IEventGridEventHandler<in TEvent> : IEventGridEventHandler
{
    Task HandleAsync(TEvent eventData, CloudEvent cloudEvent, CancellationToken cancellationToken);
}
