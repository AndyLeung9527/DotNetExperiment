namespace DotNetOpenTelemetry.Microservice.WebApi1ToWebApi2;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using System.Net.Http.Headers;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

internal class Program
{
    const string nextServiceName = "WebApi2ToGrpcService3";

    static readonly ActivitySource ActivitySource = new(nameof(WebApi1ToWebApi2));
    static readonly Meter CustomMeter = new(nameof(CustomMeter), "1.0");
    static readonly Counter<long> CustomCounter = CustomMeter.CreateCounter<long>(nameof(CustomCounter));
    static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    static async Task Main(string[] args)
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
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = "192.168.5.217";
                    options.AgentPort = 6831;
                }))
            .WithMetrics(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(WebApi1ToWebApi2)))
                .AddMeter(nameof(CustomMeter))
                .AddConsoleExporter());

        var app = builder.Build();

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

            var response = await httpClient.GetAsync($"/Receive/{content}");
            var result = await response.Content.ReadAsStringAsync();
            return Results.Ok(result);
        });

        app.Run();
    }
}