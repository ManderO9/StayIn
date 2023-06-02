namespace Stayin.Core;

public class UserDeletedEvent : BaseEvent
{
    #region Public Properties

    /// <summary>
    /// The id of the deleted user
    /// </summary>
    public required string UserId { get; set; }

    #endregion

    /// <inheritdoc/>
    public override Task Handle(IDataAccess dataAccess)
    {
        // Ignore this event
        return Task.CompletedTask;
    }
}
