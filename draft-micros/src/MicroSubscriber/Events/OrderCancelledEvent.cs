namespace MicroSubscriber.Events;

public sealed record OrderCancelledEvent(string OrderId, string Reason);
