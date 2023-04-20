namespace Stayin.Auth;

/// <summary>
/// An event bus for publishing and consuming messages
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Returns the list of all events available in the event store
    /// </summary>
    /// <returns></returns>
    Task<List<BaseEvent>> GetAllEvents();

    /// <summary>
    /// Returns a list of new events in the event store
    /// </summary>
    /// <param name="dataAccess">The data access service</param>
    /// <returns></returns>
    Task<List<BaseEvent>> GetNewEvents(IDataAccess dataAccess);


    /// <summary>
    /// Publishes a new event to the event bus
    /// </summary>
    /// <typeparam name="T">The type of the event to publish</typeparam>
    /// <param name="message">The event to publish</param>
    /// <returns></returns>
    Task Publish<T>(T message) where T : BaseEvent;

}
