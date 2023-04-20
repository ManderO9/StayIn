namespace Stayin.Auth;

public class UserDeletedEvent : BaseEvent
{
    public required string UserId { get; set; }

    /// <inheritdoc/>
    public override Task Handle(IDataAccess dataAccess)
    {
        // Ignore this event
        return Task.CompletedTask;
    }
}
