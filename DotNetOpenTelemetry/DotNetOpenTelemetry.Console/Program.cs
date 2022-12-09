namespace DotNetOpenTelemetry;

using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

partial class Program
{
    private static ActivitySource _source = new ActivitySource("DotNetOpenTelemetry", "1.0.0");
    static async Task Main(string[] args)
    {
        //Console.WriteLine("************************Microsoft .NET documentation demo which using opentelemetry***********************");
        //await Main1(args);
        //Console.WriteLine("**********************************************************************************************************");
        //Console.WriteLine();
        //Console.WriteLine("************************Microsoft .NET documentation demo which using custom logic*************************");
        //await Main2(args);
        //Console.WriteLine("***********************************************************************************************************");
        //Console.WriteLine();
        //Console.WriteLine("**********************************Opentelemetry demo on github(Log part)**********************************");
        //await Main3(args);
        //Console.WriteLine("**********************************************************************************************************");
        //Console.WriteLine();
        //Console.WriteLine("**********************************Opentelemetry demo on github(Metrics part)**********************************");
        //await Main4(args);
        //Console.WriteLine("**************************************************************************************************************");
        //Console.WriteLine();
        Console.WriteLine("***********************************Opentelemetry demo on github(Traces part)***********************************");
        await Main5(args);
        Console.WriteLine("***************************************************************************************************************");
    }

    static async Task DoSomeWork(string foo, int bar)
    {
        using (Activity? activity = _source.StartActivity("SomeWork"))
        {
            activity?.SetTag("foo", foo);
            activity?.SetTag("bar", bar);
            await StepOne();
            activity?.AddEvent(new ActivityEvent("Part way there"));
            await StepTwo();
            activity?.AddEvent(new ActivityEvent("Done now"));

            // Pretend something went wrong
            activity?.SetTag("otel.status_code", "ERROR");
            activity?.SetTag("otel.status_description", "Use this text give more information abount the error");
        }
    }

    static async Task StepOne()
    {
        using (Activity? activity = _source.StartActivity("StepOne"))
        {
            await Task.Delay(500);
        }
    }

    static async Task StepTwo()
    {
        using (Activity? activity = _source.StartActivity("StepTwo"))
        {
            await Task.Delay(1000);
        }
    }
}
/// <summary>
/// Microsoft .NET documentation demo which using opentelemetry
/// </summary>
partial class Program
{
    static async Task Main1(string[] args)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetryDemo"))
            .AddSource("DotNetOpenTelemetry")
            .AddConsoleExporter()
            .Build();

        await DoSomeWork("banana", 8);
        Console.WriteLine("Example work done");
    }
}
/// <summary>
/// Microsoft .NET documentation demo which using custom logic
/// </summary>
partial class Program
{
    static async Task Main2(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        Console.WriteLine("         {0,-15}{1,-60}{2,-15}", "OperationName", "Id", "Duration");
        ActivitySource.AddActivityListener(new ActivityListener()
        {
            ShouldListenTo = (source) => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => Console.WriteLine("Started: {0,-15}{1,-60}", activity.OperationName, activity.Id),
            ActivityStopped = activity => Console.WriteLine("Stopped: {0,-15}{1,-60}{2,-15}", activity.OperationName, activity.Id, activity.Duration)
        });

        await DoSomeWork("banana", 8);
        Console.WriteLine("Example work done");
    }
}
/// <summary>
/// Opentelemetry demo on github
/// Log part
/// </summary>
partial class Program
{
    static async Task Main3(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddConsoleExporter();
            });
        });

        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Hello from {name} {price}.", "tomato", 2.99);
    }
}
/// <summary>
/// Opentelemetry demo on github
/// Metrics part
/// </summary>
partial class Program
{
    private static readonly Meter MyMeter = new("MyCompany.MyProduct.MyLibrary", "1.0");
    private static readonly Counter<long> MyFruitCounter = MyMeter.CreateCounter<long>("MyFruitCounter");

    static async Task Main4(string[] args)
    {
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("MyCompany.MyProduct.MyLibrary")
            .AddConsoleExporter()
            .Build();

        MyFruitCounter.Add(1, new("name", "apple"), new("color", "red"));
        MyFruitCounter.Add(2, new("name", "lemon"), new("color", "yellow"));
        MyFruitCounter.Add(1, new("name", "lemon"), new("color", "yellow"));
        MyFruitCounter.Add(2, new("name", "apple"), new("color", "green"));
        MyFruitCounter.Add(5, new("name", "apple"), new("color", "red"));
        MyFruitCounter.Add(4, new("name", "lemon"), new("color", "yellow"));
    }
}
/// <summary>
/// Opentelemetry demo on github
/// Traces part
/// </summary>
partial class Program
{
    private static readonly ActivitySource MyActivitySource = new("MyCompany.MyProduct.MyLibrary");
    static async Task Main5(string[] args)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("MyCompany.MyProduct.MyLibrary")
            .AddConsoleExporter()
            .Build();

        using (var activity = MyActivitySource.StartActivity("SayHello"))
        {
            activity?.SetTag("foo", 1);
            activity?.SetTag("bar", "Hello, World!");
            activity?.SetTag("baz", new int[] { 1, 2, 3 });
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
    }
}