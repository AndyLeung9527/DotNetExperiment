namespace DotNetOpenTelemetry.Microservice.Grpc3ToRabbitMQ4;

using OpenTelemetry;
using DotNetOpenTelemetry.Microservice.Grpc3ToRabbitMQ4.Services;
using OpenTelemetry.Trace;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;

internal class Program
{
    public static readonly ActivitySource ActivitySource = new(nameof(Grpc3ToRabbitMQ4));
    public static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();

        builder.Services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(Grpc3ToRabbitMQ4)))
                .AddAspNetCoreInstrumentation(options => options.Filter = httpContext => !httpContext.Request.Path.Equals("/"))
                .AddSource(nameof(Grpc3ToRabbitMQ4))
                .AddConsoleExporter()
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = "192.168.5.217";
                    options.AgentPort = 6831;
                }));

        var app = builder.Build();

        app.MapGrpcService<CasualService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}