namespace Stayin.Core;

public class NullEvent : BaseEvent
{
    public override Task Handle(IDataAccess dataAccess)
    {
        // Ignore this event
        return Task.CompletedTask;
    }
}
