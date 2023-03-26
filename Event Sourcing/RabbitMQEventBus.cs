using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Stayin.Auth;

public class RabbitMQEventBus
{
    #region Private Members


    private const string mQueueName = "DefaultQueue";

    private const string mHostName = "localhost";

    /// <summary>
    /// The connection to the RabbitMQ server
    /// </summary>
    IConnection? mConnection;

    /// <summary>
    /// The channel to publish and consumer messages from the RabbitMQ server
    /// </summary>
    IModel? mChannel;

    /// <summary>
    /// Message consumer
    /// </summary>
    EventingBasicConsumer? mConsumer;


    #endregion

    public Task Publish<T>(T message)
    {
        // TODO: try catch exception like this one
        // RabbitMQ.Client.Exceptions.BrokerUnreachableException

        // Get the body of the message to publish
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // Create the connection factory
        var factory = new ConnectionFactory() { HostName = mHostName };

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

        return Task.CompletedTask;
    }


    public Task StartConsuming(EventHandler<BasicDeliverEventArgs> handle)
    {
        // Create the connection factory
        var factory = new ConnectionFactory() { HostName = mHostName };

        // Create the connection to use to access the RabbitMQ server
        mConnection = factory.CreateConnection();

        // Create a channel
        mChannel= mConnection.CreateModel();

        // Declare the queue we are gonna use to publish messages
        mChannel.QueueDeclare(mQueueName, true, false, false, null);

        // Create the event consumer
        mConsumer = new EventingBasicConsumer(mChannel);

        // Set the method that handles new messages
        mConsumer.Received += handle;

        // Start consuming
        mChannel.BasicConsume(mQueueName, false, mConsumer);

        return Task.CompletedTask;
    }

    public Task StopConsuming()
    {
        mConsumer = null;
        mChannel?.Dispose();
        mConnection?.Dispose();

        return Task.CompletedTask;
    }

}
