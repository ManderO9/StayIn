namespace Stayin.Auth;

/// <summary>
/// Base event for all events that will be published/consumed by our application
/// </summary>
public class BaseEvent
{
    /// <summary>
    /// The unique identifier of this event
    /// </summary>
    public required string EventId { get; set; }

    /// <summary>
    /// The time at which this event was created and published
    /// </summary>
    public required DateTimeOffset PublishedTime { get; set; }
}
