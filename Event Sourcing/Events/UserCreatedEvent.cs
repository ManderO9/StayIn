namespace Stayin.Auth;

/// <summary>
/// The event that will get published when a user got created
/// </summary>
public class UserCreatedEvent : BaseEvent
{
    public required string UserId { get; set; }

    /// <inheritdoc/>
    public override Task Handle(IDataAccess dataAccess)
    {
        // Ignore this event
        return Task.CompletedTask;
    }
}
