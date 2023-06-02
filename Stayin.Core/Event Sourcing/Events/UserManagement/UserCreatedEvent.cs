namespace Stayin.Core;

/// <summary>
/// The event that will get published when a user got created
/// </summary>
public class UserCreatedEvent : BaseEvent
{
    #region Public Properties

    /// <summary>
    /// The id of the created user
    /// </summary>
    public required string UserId { get; set; }
    
    /// <summary>
    /// The username of the user
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The email of the user
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The phone number of the user
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    #endregion

    /// <inheritdoc/>
    public override Task Handle(IDataAccess dataAccess)
    {
        // Ignore this event
        return Task.CompletedTask;
    }
}
