namespace DotNetHosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        var collector = new Collector();
        //使用HostBuilder构建主机
        //不加载环境变量
        //不加载launchSettings.json
        //不加载appsettings.json
        //不设置EnvironmentName
        //因此下述的context.HostingEnvironment.EnvironmentName是Production（默认值）
        //可使用Host.CreateDefaultBuilder(args)代替，它会加载上述内容，开箱即用
        var host = new HostBuilder()
            .ConfigureHostConfiguration(builder => builder.AddCommandLine(args))
            .ConfigureAppConfiguration((context, builder) =>
            {
                //builder.Add(new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource { Path = "appsettings.json" });等同AddJsonFile
                builder.AddJsonFile("appsettings.json", false);
                builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ITemperatureCollector>(collector);
                services.AddSingleton<IHumidityCollector>(collector);
                services.AddSingleton<IAirQualityCollector>(collector);
                services.AddSingleton<AirEnvironmentPublisher>();
                services.AddHostedService<AirEnvironmentService>();
                services.AddOptions();
                services.Configure<AirEnvironmentOptions>(context.Configuration.GetSection("AirEnvironment"));

                services.AddHostedService<LifeTimeDemoService>();
                services.AddHostedService<TimedHostedDemoService>();
            })
            .ConfigureLogging(builder => builder.AddConsole())
            .Build();
        host.Run();
    }
}

public class AirEnvironmentOptions
{
    public long Interval { get; set; }
}
public class AirEnvironmentPublisher
{
    private const string Template = "温度：{temperature, -10}" +
                                    "湿度：{humidity, -10}" +
                                    "空气质量：{airQuality, -10}" +
                                    "时间：{now}";
    private readonly ILogger _logger;
    private readonly Action<ILogger, int, int, int, string, Exception?> _logAction;

    public AirEnvironmentPublisher(ILogger<AirEnvironmentPublisher> logger)
    {
        _logger = logger;
        _logAction = LoggerMessage.Define<int, int, int, string>(LogLevel.Information, 0, Template);
    }

    public void Publish(int temp, int humi, int ariq)
    {
        _logAction(_logger, temp, humi, ariq, DateTime.Now.ToLongTimeString(), null);
    }
}
public class AirEnvironmentService : IHostedService
{
    private readonly ITemperatureCollector _temperatureCollector;
    private readonly IHumidityCollector _humidityCollector;
    private readonly IAirQualityCollector _airQualityCollector;
    private readonly AirEnvironmentPublisher _publisher;
    private readonly AirEnvironmentOptions _options;

    private Timer? _timer;

    public AirEnvironmentService(ITemperatureCollector temperatureCollector, IHumidityCollector humidityCollector, IAirQualityCollector airQualityCollector, AirEnvironmentPublisher publisher, IOptions<AirEnvironmentOptions> options)
    {
        _temperatureCollector = temperatureCollector;
        _humidityCollector = humidityCollector;
        _airQualityCollector = airQualityCollector;
        _publisher = publisher;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(_ =>
        {
            _publisher.Publish(_temperatureCollector.Get(), _humidityCollector.Get(), _airQualityCollector.Get());
        }, null, 0, _options.Interval);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}

/// <summary>
/// 温度
/// </summary>
public interface ITemperatureCollector
{
    int Get();
}
/// <summary>
/// 湿度
/// </summary>
public interface IHumidityCollector
{
    int Get();
}
/// <summary>
/// 空气质量
/// </summary>
public interface IAirQualityCollector
{
    int Get();
}
public class Collector : ITemperatureCollector, IHumidityCollector, IAirQualityCollector
{
    int ITemperatureCollector.Get() => Random.Shared.Next(0, 1000);
    int IHumidityCollector.Get() => Random.Shared.Next(0, 1000);
    int IAirQualityCollector.Get() => Random.Shared.Next(0, 1000);
}

public class LifeTimeDemoService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private IDisposable? _tokenSource;

    public LifeTimeDemoService(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
        _lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine($"{DateTimeOffset.Now} Application Started");
        });
        _lifetime.ApplicationStopping.Register(() =>
        {
            Console.WriteLine($"{DateTimeOffset.Now} Application Stopping");
        });
        _lifetime.ApplicationStopped.Register(() =>
        {
            Console.WriteLine($"{DateTimeOffset.Now} Application Stopped");
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token.Register(_lifetime.StopApplication);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _tokenSource?.Dispose();
        return Task.CompletedTask;
    }
}

public class TimedHostedDemoService : IHostedService
{
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _timerTask;

    private readonly int _interval;

    public TimedHostedDemoService()
    {
        _interval = 1;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(_interval));
        _timerTask = ExecAsync();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timerTask is not null)
        {
            _cts?.Cancel();
            await _timerTask;
            _timer?.Dispose();
            _cts?.Dispose();
            _timerTask = null;
        }
    }

    private async Task ExecAsync()
    {
        try
        {
            while (_timer != null && _cts != null && await _timer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    Console.WriteLine($"后台定时任务执行成功{DateTime.Now:HH:mm:ss}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"后台定时任务执行异常");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"后台定时任务停止");
        }
    }
}