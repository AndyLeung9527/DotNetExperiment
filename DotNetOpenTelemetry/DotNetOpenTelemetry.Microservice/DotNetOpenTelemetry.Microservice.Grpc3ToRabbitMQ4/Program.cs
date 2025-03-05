﻿using DotNetOpenTelemetry.Microservice.Grpc3ToRabbitMQ4.Services;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace DotNetOpenTelemetry.Microservice.Grpc3ToRabbitMQ4;

internal class Program
{
    public static readonly ActivitySource ActivitySource = new(nameof(Grpc3ToRabbitMQ4));
    public static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();

        builder.Services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(Grpc3ToRabbitMQ4)))
                .AddAspNetCoreInstrumentation(options => options.Filter = httpContext => !httpContext.Request.Path.Equals("/"))
                .AddSource(nameof(Grpc3ToRabbitMQ4))
                .AddConsoleExporter()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri("http://localhost:4317/");
                }));

        var app = builder.Build();

        app.MapGrpcService<CasualService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}