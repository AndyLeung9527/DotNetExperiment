namespace DotNetOpenTelemetry.Microservice.Grpc3ToRabbitMQ4.Services;

using DotNetOpenTelemetry.Microservice.Protos;
using Grpc.Core;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

public class CasualService : Casual.CasualBase
{
    public override Task<TransmitResult> Transmit(TransmitRequest request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var parentContext = Program.Propagator.Extract(default, httpContext.Request.Headers, (headers, key) =>
        {
            return new string[] { headers[key] };
        });
        Baggage.Current = parentContext.Baggage;

        var activityName = $"{nameof(Grpc3ToRabbitMQ4)}_Transmit";
        using var activity = Program.ActivitySource.StartActivity(activityName, ActivityKind.Server, parentContext.ActivityContext);
        using var connection = RabbitMqHelper.CreateConnection();
        using var channel = RabbitMqHelper.CreateModelAndDeclareQueue(connection);
        var props = channel.CreateBasicProperties();

        ActivityContext contextToInject = default;
        if (activity != null)
            contextToInject = activity.Context;
        else if (Activity.Current != null)
            contextToInject = Activity.Current.Context;

        Program.Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), props, (props, key, value) =>
        {
            if (props.Headers == null)
                props.Headers = new Dictionary<string, object>();

            props.Headers[key] = value;
        });

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination_kind", "queue");
        activity?.SetTag("messaging.destination", RabbitMqHelper.DefaultExchangeName);
        activity?.SetTag("messaging.rabbitmq.routing_key", RabbitMqHelper.TestQueueName);

        var body = $"{request.Content}->{nameof(Grpc3ToRabbitMQ4)}";

        channel.BasicPublish(
            exchange: RabbitMqHelper.DefaultExchangeName,
            routingKey: RabbitMqHelper.TestQueueName,
            basicProperties: props,
            body: Encoding.UTF8.GetBytes(body));

        return Task.FromResult(new TransmitResult
        {
            Result = body
        });
    }
}