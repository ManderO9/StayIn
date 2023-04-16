namespace Stayin.Auth;

public class UserCreatedEvent : BaseEvent
{
    public required string UserId { get; set; }

    public Task Handle(ApplicationDbContext db)
    {
        Console.WriteLine($"User created: userId:{UserId}, eventId: {EventId}, event created date: {PublishedTime}");
        return Task.CompletedTask;
    }
}
