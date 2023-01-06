namespace DotNetOpenTelemetry.Microservice.RabbitMQ4;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

internal class Program
{
    public static readonly ActivitySource ActivitySource = new(nameof(RabbitMQ4));
    public static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHostedService<MainBackgroundService>();

        builder.Services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(RabbitMQ4)))
                .AddAspNetCoreInstrumentation(options => options.Filter = httpContext => !httpContext.Request.Path.Equals("/"))
                .AddSource(nameof(RabbitMQ4))
                .AddConsoleExporter())
            .StartWithHost();

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}