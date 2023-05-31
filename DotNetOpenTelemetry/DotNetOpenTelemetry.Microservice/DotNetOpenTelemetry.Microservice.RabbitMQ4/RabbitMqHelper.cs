using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotNetOpenTelemetry.Microservice.RabbitMQ4;

public static class RabbitMqHelper
{
    public const string DefaultExchangeName = "";
    public const string TestQueueName = "TestQueue";

    readonly static ConnectionFactory _connectionFactory;

    static RabbitMqHelper()
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = "192.168.5.217",
            UserName = "guest",
            Password = "guest",
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(3),
            VirtualHost = "/"
        };
    }

    public static IConnection CreateConnection()
    {
        return _connectionFactory.CreateConnection();
    }

    public static IModel CreateModelAndDeclareQueue(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: TestQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        return channel;
    }
}