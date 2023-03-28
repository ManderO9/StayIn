namespace Stayin.Auth;

public class UserDeletedEvent : BaseEvent
{
    public required string UserId { get; set; }

    public Task Handle(ApplicationDbContext db)
    {
        Console.WriteLine("User has been delted");
        Console.WriteLine("Id: " + UserId);
        return Task.CompletedTask;
    }
}
