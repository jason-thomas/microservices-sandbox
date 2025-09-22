namespace Draft.Worldpay.Parser.Options;

public sealed class EventGridPublisherOptions
{
    public const string SectionName = "EventGrid";

    public string? TopicEndpoint { get; set; }

    public string? AccessKey { get; set; }

    public string EventType { get; set; } = "worldpay.echeck.returnrecord";

    public string SubjectPrefix { get; set; } = "/worldpay/echeck/returns";

    public string DataVersion { get; set; } = "1.0";

    public int BatchSize { get; set; } = 100;
}
