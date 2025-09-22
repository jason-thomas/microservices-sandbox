using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;
using Microsoft.Extensions.Logging;
using MicroSubscriber.Events;

namespace MicroSubscriber.Handlers.Implementations;

public sealed class OrderCancelledEventHandler : EventGridEventHandler<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public override string EventName => "OrderCancelled";

    public override Task HandleAsync(OrderCancelledEvent eventData, CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handled OrderCancelled event for {OrderId}. Reason: {Reason}.",
            eventData.OrderId,
            eventData.Reason);

        return Task.CompletedTask;
    }
}
