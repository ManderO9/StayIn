namespace Stayin.Core;

/// <summary>
/// An event bus for publishing and consuming messages
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Returns the list of all events available in the event store
    /// </summary>
    /// <returns></returns>
    /// /// <remarks>
    /// If the operation fails for some reason an empty list is returned and an error is logged
    /// </remarks>
    Task<List<BaseEvent>> GetAllEvents();

    /// <summary>
    /// Returns a list of new events in the event store
    /// </summary>
    /// <param name="dataAccess">The data access service</param>
    /// <returns></returns>
    /// /// <remarks>
    /// If the operation fails for some reason an empty list is returned and an error is logged
    /// </remarks>
    Task<List<BaseEvent>> GetNewEvents(IDataAccess dataAccess);


    /// <summary>
    /// Publishes a new event to the event bus
    /// </summary>
    /// <typeparam name="T">The type of the event to publish</typeparam>
    /// <param name="message">The event to publish</param>
    /// <returns>A <see cref="Task"/> containing <see langword="true"/> if we successfully published the message or <see langword="false"/> if not</returns>
    Task<bool> Publish<T>(T message) where T : BaseEvent;

}
