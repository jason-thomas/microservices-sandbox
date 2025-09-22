using System.Collections.Generic;

namespace MicroSubscriber.Options;

public sealed class EventGridNamespaceOptions
{
    public const string SectionName = "EventGrid";

    public string NamespaceEndpoint { get; set; } = string.Empty;

    public CredentialOptions Credential { get; set; } = new();

    public int? MaxEventsPerReceive { get; set; }

    public int? MaxWaitTimeSeconds { get; set; }

    public List<EventGridSubscriptionOptions> Subscriptions { get; } = new();

    public sealed class CredentialOptions
    {
        public string? Key { get; set; }

        public bool UseDefaultAzureCredential { get; set; }
    }
}
