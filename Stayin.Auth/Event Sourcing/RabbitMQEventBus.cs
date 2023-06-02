using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Diagnostics;
using System.Reflection;
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

    /// <summary>
    /// Logger to log messages
    /// </summary>
    private readonly ILogger<RabbitMQEventBus> mLogger;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="queueName">The name of the queue to get messages from</param>
    /// <param name="uri">The URI to the RabbitMQ server to get messages from</param>
    public RabbitMQEventBus(string queueName, string uri, ILogger<RabbitMQEventBus> logger)
    {
        mQueueName = queueName;
        mUri = uri;
        mLogger = logger;
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
        // Get the current namespace
        var namespaceName = typeof(BaseEvent).Namespace;

        // Get the type of the message
        var type = Type.GetType(namespaceName + "." + messageType);

        // If it doesn't exist
        if(type is null)
        {
            // Throw
            Debugger.Break();
            throw new NotImplementedException();
        }

        // Return a deserialized object of that event
        return (BaseEvent)JsonSerializer.Deserialize(message.Span, type)!;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<List<BaseEvent>> GetNewEvents(IDataAccess dataAccess)
    {
        // Create the connection factory with the given URI
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        try
        {
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

            // Load existing event ids from the database
            var existingEvents = await dataAccess.LoadExistingEventIdsAsync();

            // Loop
            do
            {
                // Get a message
                message = channel.BasicGet(mQueueName, false);

                // If the message is not null
                if(message != null)
                {
                    // If the event has not been already consumed
                    if(!existingEvents.Any(x => x == message.BasicProperties.MessageId))
                    {
                        // Create a new event
                        var newEvent = GetEventFromBytes(message.Body, message.BasicProperties.Type);

                        // Add the message to the list of messages to return
                        messages.Add(newEvent);

                        // Add the event to the database
                        await dataAccess.CreateEventAsync(newEvent);
                    }

                    // Set the current message tag as the last one received
                    lastDeliveryTag = message.DeliveryTag;
                }

                // Until we have a message that is null
            } while(message != null);

            // Save the changes to the database
            await dataAccess.SaveChangesAsync();

            // Acknowledge all the received messages
            channel.BasicNack(lastDeliveryTag, true, true);

            // Return the messages
            return messages;
        }
        catch(Exception ex)
            when(ex is DbUpdateConcurrencyException || ex is DbUpdateException || ex is BrokerUnreachableException)
        {
            // Log it
            mLogger.LogError(ex, "Failed to get new events");

            // Return empty list
            return new();
        }

    }

    /// <inheritdoc/>
    public Task<List<BaseEvent>> GetAllEvents()
    {
        // Create the connection factory with the given URI
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        try
        {
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
        catch(BrokerUnreachableException brokerUnreachableException)
        {
            // Log it
            mLogger.LogError(brokerUnreachableException, "Failed to get all events");

            // Return empty list
            return Task.FromResult(new List<BaseEvent>());
        }

    }

    /// <inheritdoc/>
    public Task<bool> Publish<T>(T message) where T : BaseEvent
    {
        // Get the body of the message to publish
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // Create the connection factory
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        try
        {
            // Create the connection to use to access the RabbitMQ server
            using var connection = factory.CreateConnection();

            // Create a channel
            using var channel = connection.CreateModel();

            // Declare the queue we are gonna use to publish messages
            channel.QueueDeclare(mQueueName, true, false, false, null);

            // Create the basic properties to set for the message to publish
            var properties = channel.CreateBasicProperties();

            // Get the type of the event to publish
            var type = typeof(T);

            // Set the message type
            properties.Type = type.Name;

            // Set the id of published message
            properties.MessageId = message.EventId;

            // Set message as persistent
            properties.Persistent = true;

            // Publish a message
            channel.BasicPublish("", mQueueName, properties, body);
        }
        catch(BrokerUnreachableException brokerUnreachableException)
        {
            // Log it
            mLogger.LogError(brokerUnreachableException, "Failed to publish a message");

            // Return failed
            return Task.FromResult(false);
        }

        // Return successful
        return Task.FromResult(true);
    }

    #endregion
}
