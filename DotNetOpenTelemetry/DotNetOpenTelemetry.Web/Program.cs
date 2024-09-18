using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DotNetOpenTelemetry.Web;

/// <summary>
/// Opentelemetry demo on github
/// ASP.NET Core part
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Custom metrics
        var customMeter = new Meter("Custom.Example", "1.0.0");
        var counter = customMeter.CreateCounter<int>("custom.count");

        // Custom ActivitySource
        var customActivitySource = new ActivitySource("Custom.Example");

        var builder = WebApplication.CreateBuilder(args);
        var tracingOtlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
        var otel = builder.Services.AddOpenTelemetry();

        // Configure OpenTelemetry Resources with the application name
        otel.ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName));

        // Add metrics for ASP.NET Core and custom metrics and export to console
        otel.WithMetrics(metrics => metrics
            // Metrics provider from OpenTelemetry
            .AddAspNetCoreInstrumentation()
            // Metrics provides by ASP.NET Core in .NET 8
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddMeter(customMeter.Name)
            .AddConsoleExporter());

        // Add Tracing for ASP.NET Core and custom ActivitySource and export to console
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddSource(customActivitySource.Name);
            if (tracingOtlpEndpoint != null)
            {
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
            }
            else tracing.AddConsoleExporter();
        });

        var app = builder.Build();

        app.MapGet("/", () =>
        {
            counter.Add(1);

            using var activity = customActivitySource.StartActivity("CustomActivity");
            activity?.SetTag("greeting", "Hello World!");

            return "Hello World!";
        });

        app.Run();
    }
}