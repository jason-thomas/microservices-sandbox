using System.Collections.Generic;

namespace MicroSubscriber.Options;

public sealed class EventGridSubscriptionOptions
{
    public string Name { get; set; } = string.Empty;

    public string TopicName { get; set; } = string.Empty;

    public string SubscriptionName { get; set; } = string.Empty;

    public List<string> EventTypes { get; } = new();

    public int? MaxEventsPerReceive { get; set; }

    public int? MaxWaitTimeSeconds { get; set; }
}
