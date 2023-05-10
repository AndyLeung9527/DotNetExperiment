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
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenTelemetry()
            // Add tracing
            .WithTracing(builder => builder
                .AddAspNetCoreInstrumentation((options) =>
                {
                    options.Filter = httpContext =>
                    {
                        // Only collect telemetry about HTTP GET reuquests
                        return httpContext.Request.Method.Equals("GET");
                    };
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("requestProtocol", httpRequest.Protocol);
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("requestLength", httpResponse.ContentLength);
                    };
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("exceptionType", exception.GetType().ToString());
                    };
                })
                .AddConsoleExporter())
            // Add metrics
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation((options) =>
                {
                    options.Filter = (metricName, httpContext) =>
                    {
                        // Only collect telemetry about HTTP GET reuquests
                        return httpContext.Request.Method.Equals("GET");
                    };
                    options.Enrich = (string metricName, HttpContext httpContext, ref TagList tags) =>
                    {

                    };
                })
                .AddConsoleExporter());

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}