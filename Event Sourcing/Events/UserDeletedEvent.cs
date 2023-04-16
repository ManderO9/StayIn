namespace Stayin.Auth;

public class UserDeletedEvent : BaseEvent
{
    public required string UserId { get; set; }

    public Task Handle(ApplicationDbContext db)
    {
        Console.WriteLine($"User deleted: userId:{UserId}, eventId: {EventId}, event created date: {PublishedTime}");
        return Task.CompletedTask;
    }
}
