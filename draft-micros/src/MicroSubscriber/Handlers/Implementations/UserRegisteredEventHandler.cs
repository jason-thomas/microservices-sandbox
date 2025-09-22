using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;
using Microsoft.Extensions.Logging;
using MicroSubscriber.Events;

namespace MicroSubscriber.Handlers.Implementations;

public sealed class UserRegisteredEventHandler : EventGridEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public override string EventName => "UserRegistered";

    public override Task HandleAsync(UserRegisteredEvent eventData, CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handled UserRegistered event for {UserId} ({Email}).",
            eventData.UserId,
            eventData.Email);

        return Task.CompletedTask;
    }
}
