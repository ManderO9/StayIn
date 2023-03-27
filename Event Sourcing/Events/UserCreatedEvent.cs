namespace Stayin.Auth;

public class UserCreatedEvent : BaseEvent
{
    public required string UserId { get; set; }
}
