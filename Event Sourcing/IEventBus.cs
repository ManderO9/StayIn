namespace Stayin.Auth;

/// <summary>
/// An event bus for publishing and consuming messages
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Returns the list of all messages available in the event store
    /// </summary>
    /// <returns></returns>
    Task<List<(ReadOnlyMemory<byte> message, string messageType)>> GetAllMessages();

    /// <summary>
    /// Publishes a message to the event bus
    /// </summary>
    /// <typeparam name="T">The type of the message to publish</typeparam>
    /// <param name="message">The message to publish</param>
    /// <returns></returns>
    Task Publish<T>(T message) where T : BaseEvent;

}
