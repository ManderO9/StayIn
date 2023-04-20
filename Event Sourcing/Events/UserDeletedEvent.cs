namespace Stayin.Auth;

public class UserDeletedEvent : BaseEvent
{
    public required string UserId { get; set; }

    /// <inheritdoc/>
    public override Task Handle(IDataAccess dataAccess)
    {
        Console.WriteLine($"{nameof(UserDeletedEvent)}: {EventId}" );
        // Ignore this event
        return Task.CompletedTask;
    }
}
