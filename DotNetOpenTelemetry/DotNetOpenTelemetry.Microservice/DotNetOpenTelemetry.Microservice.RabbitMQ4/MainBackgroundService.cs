namespace DotNetOpenTelemetry.Microservice.RabbitMQ4;

using OpenTelemetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

public class MainBackgroundService : BackgroundService
{
    IConnection? _connection;
    IModel? _model;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = RabbitMqHelper.CreateConnection();
        _model = RabbitMqHelper.CreateModelAndDeclareQueue(_connection);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _model?.Dispose();
        _connection?.Dispose();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_model);

        consumer.Received += (bc, ea) =>
        {
            var parentContext = Program.Propagator.Extract(default, ea.BasicProperties, (props, key) =>
            {
                if (props.Headers.TryGetValue(key, out var value))
                {
                    var bytes = value as byte[];
                    return new[] { Encoding.UTF8.GetString(bytes) };
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
        };

        _model.BasicConsume(queue: RabbitMqHelper.TestQueueName, autoAck: true, consumer: consumer);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
