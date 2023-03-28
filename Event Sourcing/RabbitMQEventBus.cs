using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Stayin.Auth;

/// <summary>
/// RabbitMQ implementation of and event bus 
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

    // TODO: delete later
    //private const string mHostName = "localhost";

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

    #region Public Methods

    public Task<List<(ReadOnlyMemory<byte> message, string messageType)>> GetAllMessages()
    {
        // Create the connection factory with the given URI
        var factory = new ConnectionFactory() { Uri = new Uri(mUri) };

        // Create a connection to the server
        using var connection = factory.CreateConnection();

        // Create a channel for communication
        using var channel = connection.CreateModel();

        // Create the list of messages to return
        var messages = new List<(ReadOnlyMemory<byte>, string)>();

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
                messages.Add((message.Body, message.BasicProperties.Type));

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

        // Get the type of the event to publish
        var type = typeof(T);

        // Set the message type
        properties.Type = type.Name;

        // Set message as persistent
        properties.Persistent = true;

        // Publish a message
        channel.BasicPublish("", mQueueName, properties, body);

        // Return completed task
        return Task.CompletedTask;
    }

    #endregion

    // TODO: delete 
    //public Task StartConsuming(Func<BasicDeliverEventArgs, Task> handle)
    //{
    //    // Create the connection factory
    //    var factory = new ConnectionFactory() { Uri = new Uri(mUri) };


    //    // Create the connection to use to access the RabbitMQ server
    //    mConnection = factory.CreateConnection();

    //    // Create a channel
    //    mChannel = mConnection.CreateModel();

    //    // Declare the queue we are gonna use to publish messages
    //    mChannel.QueueDeclare(mQueueName, true, false, false, null);

    //    // Create the event consumer
    //    mConsumer = new EventingBasicConsumer(mChannel);

    //    // Set the method that handles new messages
    //    mConsumer.Received += async (sender, eventArgs) =>
    //    {
    //        // Handle the message
    //        await handle(eventArgs);

    //        // Negative acknowledge the message so it doesn't get deleted from the message store
    //        mChannel.BasicNack(eventArgs.DeliveryTag, false, true);
    //    };

    //    // Start consuming
    //    mChannel.BasicConsume(mConsumer, mQueueName);

    //    return Task.CompletedTask;
    //}

    //public Task StopConsuming()
    //{
    //    mChannel?.Close();
    //    mConnection?.Close();
    //    mConsumer = null;
    //    mChannel?.Dispose();
    //    mConnection?.Dispose();

    //    return Task.CompletedTask;
    //}

}
