﻿using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DotNetOpenTelemetry.Microservice.WebApi1ToWebApi2;

internal class Program
{
    const string nextServiceName = "WebApi2ToGrpcService3";

    static readonly ActivitySource ActivitySource = new(nameof(WebApi1ToWebApi2));
    static readonly Meter CustomMeter = new(nameof(CustomMeter), "1.0");
    static readonly Counter<long> CustomCounter = CustomMeter.CreateCounter<long>(nameof(CustomCounter));
    static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient(nextServiceName, httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://localhost:13910");
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(WebApi1ToWebApi2)))
                .AddAspNetCoreInstrumentation(options => options.Filter = httpContext => !httpContext.Request.Path.Equals("/"))
                .AddSource(nameof(WebApi1ToWebApi2))
                .AddConsoleExporter()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri("http://localhost:4317/");
                }))
            .WithMetrics(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(WebApi1ToWebApi2)))
                .AddMeter(nameof(CustomMeter))
                .AddConsoleExporter()
                .AddPrometheusExporter());

        var app = builder.Build();

        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        app.MapGet("/Send/{content}", async ([FromRoute] string content, IHttpClientFactory httpClientFactory) =>
        {
            CustomCounter.Add(1, new("method", "GET"), new("url", $"/Send/{content}"));

            var activityName = $"{nameof(WebApi1ToWebApi2)}_Send";
            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Client);

            ActivityContext contextToInject = default;
            if (activity != null)
                contextToInject = activity.Context;
            else if (Activity.Current != null)
                contextToInject = Activity.Current.Context;

            var httpClient = httpClientFactory.CreateClient(nextServiceName);

            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), httpClient.DefaultRequestHeaders, (headers, key, value) =>
            {
                headers.Add(key, value);
            });

            content = $"{content}->{nameof(WebApi1ToWebApi2)}";
            var response = await httpClient.GetAsync($"/Receive/{content}");
            var result = await response.Content.ReadAsStringAsync();
            return Results.Ok(result);
        });

        app.Run();
    }
}