namespace DotNetOpenTelemetry.Microservice.Grpc3ToRabbitMQ4;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public static class RabbitMqHelper
{
    public const string DefaultExchangeName = "";
    public const string TestQueueName = "TestQueue";

    readonly static ConnectionFactory _connectionFactory;

    static RabbitMqHelper()
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = "192.168.2.113",
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