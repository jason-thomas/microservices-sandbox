namespace MicroSubscriber.Events;

public sealed record UserRegisteredEvent(string UserId, string Email);
