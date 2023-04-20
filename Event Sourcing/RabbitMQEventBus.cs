using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Stayin.Auth;

/// <summary>
/// RabbitMQ implementation of an event bus 
/// </summary>
public class RabbitMQEventBus : IEventBus
{
    #region Private Members

    /// <summary>
    /// The name of the queue to get messages from
    /// </summary>
    private string mQueueName;

    /// <summary>
    /// The URI of the RabbitMQ message bus
    /// </summary>
    private string mUri;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="queueName">The name of the queue to get messages from</param>
    /// <param name="uri">The URI to the RabbitMQ server to get messages from</param>
    public RabbitMQEventBus(string queueName, string uri)
    {
        mQueueName = queueName;
        mUri = uri;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Return a <see cref="BaseEvent"/> using the possibleEvent of the message and it's bytes
    /// </summary>
    /// <param name="message">The message to deserialize to a base event</param>
    /// <param name="messageType">The possibleEvent of the message</param>
    /// <returns></returns>
    /// <exception cref="UnreachableException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    private BaseEvent GetEventFromBytes(ReadOnlyMemory<byte> message, string messageType)
    {
        // TODO: change this to a more elegant way
        // The list of all events that we handle in our application
        var availableEvents = new List<(string typeName, Type type)>() {
            (nameof(UserCreatedEvent), typeof(UserCreatedEvent)),
            (nameof(UserDeletedEvent), typeof(UserDeletedEvent)),
        };

        // For each event in the available events
        foreach(var possibleEvent in availableEvents)
        {
            // If the type of the event matches
            if(possibleEvent.typeName == messageType)

                // Return a deserialized object of that event
                return (BaseEvent)JsonSerializer.Deserialize(message.Span, possibleEvent.type)!;
        }

        // If we got here, there was an event that we didn't implement or there was an error somewhere
        Debugger.Break();
        throw new NotImplementedException();
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<List<BaseEvent>> GetNewEvents(IDataAccess dataAccess)
    {
        // Create the connection factory with the given URI
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        // Create a connection to the server
        using var connection = factory.CreateConnection();

        // Create a channel for communication
        using var channel = connection.CreateModel();

        // Create the list of messages to return
        var messages = new List<BaseEvent>();

        // The message to get in each request
        BasicGetResult? message = null;

        // The tag of the last delivered message so we can acknowledge them
        ulong lastDeliveryTag = 0;

        // Loop
        do
        {
            // Get a message
            message = channel.BasicGet(mQueueName, false);

            // If the message is not null
            if(message != null)
            {
                // If the event has not been handled yet
                if(await dataAccess.AddToConsumedEventsIfNotAlreadyAsync(message.BasicProperties.MessageId))
                    // Add the message to the list of messages to return
                    messages.Add(GetEventFromBytes(message.Body, message.BasicProperties.Type));

                // Set the current message tag as the last one received
                lastDeliveryTag = message.DeliveryTag;
            }

            // Until we have a message that is null
        } while(message != null);

        // Acknowledge all the received messages
        channel.BasicNack(lastDeliveryTag, true, true);

        // Return the messages
        return messages;
    }

    /// <inheritdoc/>
    public Task<List<BaseEvent>> GetAllEvents()
    {
        // Create the connection factory with the given URI
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        // Create a connection to the server
        using var connection = factory.CreateConnection();

        // Create a channel for communication
        using var channel = connection.CreateModel();

        // Create the list of messages to return
        var messages = new List<BaseEvent>();

        // The message to get in each request
        BasicGetResult? message = null;

        // The tag of the last delivered message so we can acknowledge them
        ulong lastDeliveryTag = 0;
        
        // Loop
        do
        {
            // Get a message
            message = channel.BasicGet(mQueueName, false);

            // If the message is not null
            if(message != null)
            {
                // Add the message to the list of messages to return
                messages.Add(GetEventFromBytes(message.Body, message.BasicProperties.Type));

                // Set the current message tag as the last one received
                lastDeliveryTag = message.DeliveryTag;
            }

            // Until we have a message that is null
        } while(message != null);

        // Acknowledge all the received messages
        channel.BasicNack(lastDeliveryTag, true, true);

        // Return the messages
        return Task.FromResult(messages);
    }

    /// <inheritdoc/>
    public Task Publish<T>(T message) where T : BaseEvent
    {
        // TODO: try catch exception like this one
        // RabbitMQ.Client.Exceptions.BrokerUnreachableException

        // Get the body of the message to publish
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // Create the connection factory
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        // Create the connection to use to access the RabbitMQ server
        using var connection = factory.CreateConnection();

        // Create a channel
        using var channel = connection.CreateModel();

        // Declare the queue we are gonna use to publish messages
        channel.QueueDeclare(mQueueName, true, false, false, null);

        // Create the basic properties to set for the message to publish
        var properties = channel.CreateBasicProperties();

        // Get the possibleEvent of the event to publish
        var type = typeof(T);

        // Set the message possibleEvent
        properties.Type = type.Name;

        // Set the id of published message
        properties.MessageId = Guid.NewGuid().ToString();

        // Set message as persistent
        properties.Persistent = true;

        // Publish a message
        channel.BasicPublish("", mQueueName, properties, body);

        // Return completed task
        return Task.CompletedTask;
    }

    #endregion
}
