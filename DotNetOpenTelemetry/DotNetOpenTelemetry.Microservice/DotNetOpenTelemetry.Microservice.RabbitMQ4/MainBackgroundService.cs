namespace DotNetOpenTelemetry.Microservice.RabbitMQ4;

using OpenTelemetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

public class MainBackgroundService : BackgroundService
{
    IConnection? _connection;
    IChannel? _channel;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await RabbitMqHelper.CreateConnectionAsync();
        _channel = await RabbitMqHelper.CreateModelAndDeclareQueueAsync(_connection);
        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Dispose();
        _connection?.Dispose();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += (bc, ea) =>
        {
            var parentContext = Program.Propagator.Extract(default, ea.BasicProperties, (props, key) =>
            {
                if (props.Headers?.TryGetValue(key, out var value) ?? false)
                {
                    var bytes = value as byte[];
                    if (bytes != null)
                    {
                        return new[] { Encoding.UTF8.GetString(bytes) };
                    }
                }

                return Enumerable.Empty<string>();
            });
            Baggage.Current = parentContext.Baggage;

            var activityName = $"{nameof(RabbitMQ4)}_Receive";
            using var activity = Program.ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);

            var message = Encoding.UTF8.GetString(ea.Body.Span.ToArray());
            message = $"{message}->{nameof(RabbitMQ4)}";
            activity?.SetTag("message", message);

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", RabbitMqHelper.DefaultExchangeName);
            activity?.SetTag("messaging.rabbitmq.routing_key", RabbitMqHelper.TestQueueName);

            Thread.Sleep(100);
            return Task.CompletedTask;
        };

        await _channel!.BasicConsumeAsync(queue: RabbitMqHelper.TestQueueName, autoAck: true, consumer: consumer);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
