using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MicroSubscriber.Services;

public sealed class EventGridNamespaceBackgroundService : BackgroundService
{
    private readonly IEventGridNamespaceSubscriber _subscriber;
    private readonly ILogger<EventGridNamespaceBackgroundService> _logger;

    public EventGridNamespaceBackgroundService(
        IEventGridNamespaceSubscriber subscriber,
        ILogger<EventGridNamespaceBackgroundService> logger)
    {
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Event Grid namespace subscription listeners.");
        await _subscriber.RunAsync(stoppingToken).ConfigureAwait(false);
        _logger.LogInformation("Event Grid namespace subscription listeners stopped.");
    }
}
