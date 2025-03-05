using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DotNetOpenTelemetry;

partial class Program
{
    /*
    s_activeSources.Add(this);
    s_allListeners.shouldListenTo(activitySource)?activitySource.AddListener(listener)
     */
    private static ActivitySource _source = new ActivitySource("DotNetOpenTelemetry", "1.0.0");

    static async Task Main(string[] args)
    {
        Console.WriteLine("************************Microsoft .NET documentation demo which using opentelemetry***********************");
        await Main1(args);
        Console.WriteLine("**********************************************************************************************************");
        Console.WriteLine();
        Console.WriteLine("**********************************Opentelemetry demo on github(Log part)**********************************");
        await Main2(args);
        Console.WriteLine("**********************************************************************************************************");
        Console.WriteLine();
        Console.WriteLine("**********************************Opentelemetry demo on github(Metrics part)**********************************");
        await Main3(args);
        Console.WriteLine("**************************************************************************************************************");
        Console.WriteLine();
        Console.WriteLine("***********************************Opentelemetry demo on github(Traces part)***********************************");
        await Main4(args);
        Console.WriteLine("***************************************************************************************************************");
        Console.WriteLine();
        Console.WriteLine("************************Microsoft .NET documentation demo which using custom activity listeners*************************");
        await Main5(args);
        Console.WriteLine("************************************************************************************************************************");
        Console.WriteLine();
        Console.WriteLine("************************Microsoft .NET documentation demo which using custom metrics listeners*************************");
        await Main6(args);
        Console.WriteLine("***********************************************************************************************************************");

        Console.ReadLine();
    }

    static async Task DoSomeWork(string foo, int bar)
    {
        /*
        _listeners.Count == 0?null
        samplingResult = _listeners.Sample
        Activity.Create()
          -- new Activity(name)
          -- activity._parentId= parentId;
          -- activity.IsAllDataRequested = samplingResult
          -- Start()
            -- _previousActiveActivity = Current
            -- Parent = _previousActiveActivity
            -- StartTimeUtc = GetUtcNow()
            -- GenerateW3CId()
            -- SetCurrent(this)
            -- _listeners.ActivityStarted
         */
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
            /*
            SetEndTime(GetUtcNow())
            _listeners.ActivityStopped
            SetCurrent(_previousActiveActivity)
             */
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
/// Opentelemetry demo on github
/// Log part
/// </summary>
partial class Program
{
    static async Task Main2(string[] args)
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

    static async Task Main3(string[] args)
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
    static async Task Main4(string[] args)
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
/// <summary>
/// Microsoft .NET documentation demo which using custom activity listeners
/// </summary>
partial class Program
{
    static async Task Main5(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        Console.WriteLine("         {0,-15}{1,-60}{2,-15}", "OperationName", "Id", "Duration");
        /*
        s_allListeners.Add(listener)
        ActivityListener.ShouldListenTo(s_activeSources)?s_activeSources.AddListener(this)
         */
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
/// Microsoft .NET documentation demo which using custom metrics listeners
/// </summary>
partial class Program
{
    /*
    s_allMeters.Add(this);
     */
    static Meter s_meter = new Meter("HatCo.HatStore", "1.0.0");
    /*
    new Counter<T>
    Meter = meter,Name = name
    Publish();
      -- Meter.AddInstrument(this);
      -- s_allStartedListeners<MeterListener>.InstrumentPublished?.Invoke(this, listener);
     */
    static Counter<int> s_hatsSold = s_meter.CreateCounter<int>("hats-sold");
    static Histogram<int> s_orderProcessingTimeMs = s_meter.CreateHistogram<int>("order-processing-time");
    static int s_coatsSold;
    static int s_ordersPending;
    static async Task Main6(string[] args)
    {
        s_meter.CreateObservableCounter("coats-sold", () => s_coatsSold);
        s_meter.CreateObservableGauge("orders-pending", () => s_ordersPending);

        var task = Task.Run(() =>
        {
            while (true)
            {
                // Pretend our store has one transaction each 100ms that each sell 4 hats
                Thread.Sleep(100);
                /*
                _subscriptions<DiagLinkedList<ListenerSubscription>>.MeterListener.NotifyMeasurement(this, measurement);
                  -- meterListener.MeasurementCallback(instrument, (decimal)(object)measurement
                 */
                s_hatsSold.Add(4);

                // Pretend we also sold 3 coats. For an ObservableCounter we track the value in our variable and report it
                // on demand in the callback
                s_coatsSold += 3;

                // Pretend we have some queue of orders that varies over time. The callback for the "orders-pending" gauge will report
                // this value on-demand.
                s_ordersPending = Random.Shared.Next(0, 20);

                // Last we pretend that we measured how long it took to do the transaction (for example we could time it with Stopwatch)
                s_orderProcessingTimeMs.Record(Random.Shared.Next(5, 15));
            }
        });

        // Custom metrics listener
        using (MeterListener meterlistener = new MeterListener())// 回收后注销OnMeasurementRecorded, 释放资源, 观察效果可先取消using
        {
            meterlistener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "HatCo.HatStore")
                {
                    /*
                    DiagLinkedList<Instrument> _enabledMeasurementInstruments.Add(instrument);
                    instrument.EnableMeasurement(new ListenerSubscription(this, state), out oldStateStored);
                      -- _subscriptions<DiagLinkedList<ListenerSubscription>>.Add(MeterListener)
                    MeasurementsCompleted?.Invoke(instrument!, oldState);
                     */
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            meterlistener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
            /*
            s_allStartedListeners<MeterListener>.Add(this);
            InstrumentPublished?.Invoke(s_allMeters._instruments,this);
             */
            meterlistener.Start();

            static void OnMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
            {
                Console.WriteLine($"{instrument.Name} recorded measurement {measurement}");
            }
        }
    }
}