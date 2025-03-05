using RabbitMQ.Client;

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
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(3),
            VirtualHost = "/"
        };
    }

    public static Task<IConnection> CreateConnectionAsync()
    {
        return _connectionFactory.CreateConnectionAsync();
    }

    public static async Task<IChannel> CreateModelAndDeclareQueueAsync(IConnection connection)
    {
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: TestQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        return channel;
    }
}