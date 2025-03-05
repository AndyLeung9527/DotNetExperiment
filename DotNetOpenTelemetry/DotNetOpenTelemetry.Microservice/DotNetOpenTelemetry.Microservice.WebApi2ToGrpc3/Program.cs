﻿using DotNetOpenTelemetry.Microservice.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace DotNetOpenTelemetry.Microservice.WebApi2ToGrpc3;

internal class Program
{
    static readonly ActivitySource ActivitySource = new(nameof(WebApi2ToGrpc3));
    static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpcClient<Casual.CasualClient>(options =>
        {
            options.Address = new Uri("https://localhost:13921");
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(WebApi2ToGrpc3)))
                .AddAspNetCoreInstrumentation(options => options.Filter = httpContext => !httpContext.Request.Path.Equals("/"))
                .AddSource(nameof(WebApi2ToGrpc3))
                .AddConsoleExporter()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri("http://localhost:4317/");
                }));

        var app = builder.Build();

        app.MapGet("/Receive/{content}", async ([FromRoute] string content, Casual.CasualClient casualClient, HttpContext httpContext) =>
        {
            var parentContext = Propagator.Extract(default, httpContext.Request.Headers, (headers, key) =>
            {
                return headers.TryGetValue(key, out var value) ? value : StringValues.Empty;
            });
            Baggage.Current = parentContext.Baggage;

            var activityName = $"{nameof(WebApi2ToGrpc3)}_Receive";
            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Server, parentContext.ActivityContext);

            ActivityContext contextToInject = default;
            if (activity != null)
                contextToInject = activity.Context;
            else if (Activity.Current != null)
                contextToInject = Activity.Current.Context;

            Metadata headers = new Metadata();
            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), headers, (headers, key, value) =>
            {
                headers.Add(key, value);
            });

            content = $"{content}->{nameof(WebApi2ToGrpc3)}";
            var transmitResult = await casualClient.TransmitAsync(new TransmitRequest { Content = content }, headers: headers);

            return Results.Ok(transmitResult.Result);
        });

        app.Run();
    }
}