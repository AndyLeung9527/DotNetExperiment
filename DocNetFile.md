## 一、基础知识点

### 1..NetCore、.NetFramework、.NetStandard区别

分别新建.NetCore控制台和.NetFramework控制台，并执行

```c#
Console.WriteLine(typeof(FileStream).Assembly.Location);
```

可以看到分别输出

``` 
C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.4\System.Private.CoreLib.dll
```

``` 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll
```

再新建一个.NetStandard类库，编写同样代码，并于.NetCore控制台和.NetFramework控制台中引用执行，发现结果如上一致

其中FileStream的程序集引用路径

.NetCore：

```
C:\Users\LWB\.nuget\packages\netstandard.library\2.0.3\build\netstandard2.0\ref\netstandard.dll
```

.NetFramework：

```
C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll
```

.NetStandard：

```
C:\Users\LWB\.nuget\packages\netstandard.library\2.0.3\build\netstandard2.0\ref\netstandard.dll
```

根据结果可得出结论，对同一个类名，.NetCore和.NetFramework分别做出了不同的实现，它们来自不同的程序集。而.NetStandard只是标准规范不是实现，它可以被.NetCore、.NetFramework、Xamarin等引用，并根据引用框架再进行实现。因此若需要编写一个公共的类库，尽量使用.NetStandard，并且尽量使用低版本

csproj文件，程序集下的类，.NetFramework使用的是包含，而.NetCore使用的是排除

### 2.集成开发环境

IDE可选用如下：

* .Net CLI(命令行)
* Visual Studio
* Jetbrains Rider(收费)
* Visual Studio Code

### 3.async/await

* 异步方法的返回值一般是Task<T>，T是真正的返回值类型。一般惯例异步方法命名以Async结尾
* 即使方法没有返回值，也最好把返回值声明为非泛型的Task
* 不支持异步方法，可用GetAwaiter().GetResult()，但是有死锁风险，不建议用
* 对于无需用到异步方法结果，只将其结果作为返回值返回时，可以不使用async/await而直接返回Task<T>，从而减少语法糖的使用，提高性能
* 异步方法中暂停，使用await Task.Delay()

委托中使用async\await语法示例

```c#
ThreadPool.QueueUserWorkItem(async (obj) =>
{
    await Task.CompletedTask;
});
```

本质：

* async/await是语法糖，async的方法最终编译成一个类，根据await调用切分为多个状态，对async的调用会被拆分为对MoveNext的调用，类似于状态机的调用，当遇到await，则主线程return返回。
* await调用的等待期间，框架会把当前的线程返回给线程池，等异步方法调用执行完毕后，框架会从线程池再取一个出来执行后续的代码
* 可通过Thread.CurrentThread.ManagedThreadId打印当前线程来验证，分别打印主线程，await方法内线程，await方法后线程的Id
* 异步方法的代码并不会自动在新线程中执行，除非把代码放到新线程中执行
* 具体可以反编译到c#4.0或以下看到实现代码。

缺点：

* 异步方法会生成一个类，运行效率没有普通方法高

* 可能会占用非常多的线程

其他：

* 接口方法或抽象方法不能修饰为async

### 4.yield

yield关键字属于语法糖，使用它可以让代码具有更高可读性和更好性能，当我们需要返回IEnumerable类型的时候，直接yield返回数据就可以了

```c#
public IEnumerable<int> YieldDemo()
{
    int[] array = { 1, 2, 3, 4, 5 };
    foreach (int i in array)
    {
        if (i == 4)
            yield break;
        yield return i;
    }

    yield return 6;
    yield return 7;
}
```

async方法中使用yield，返回值声明为IAsyncEnumerable<T>而非Task<T>，遍历时则使用await foreach

```c#
static async Task Main(string[] args)
{
    await foreach (var y in YieldDemoAsync())
        Console.WriteLine(y);
}

static async IAsyncEnumerable<int> YieldDemoAsync()
{
    int[] array = { 1, 2, 3, 4, 5 };
    foreach (int i in array)
    {
        if (i == 4)
            yield break;
        yield return i;
    }

    yield return 6;
    yield return 7;
}
```


### 5.CancellationTokenSource

CancellationTokenSource对象通过其 Token 属性提供取消令牌，并通过调用取消 Cancel 消息或 CancelAfter方法发送取消消息，对象实现 [IDisposable](https://docs.microsoft.com/zh-cn/dotnet/api/system.idisposable?view=net-6.0) 接口，使用完该类型的实例后，应直接或间接释放它。CancellationToken对象，指示是否请求取消

```c#
static async Task Main(string[] args)
{
    using CancellationTokenSource cts = new CancellationTokenSource();
    cts.CancelAfter(3 * 1000);

    await Task.Run(async() =>
    {
        int i = 0;
        while (!cts.IsCancellationRequested)
        {
            Console.WriteLine($"{++i}");
            await Task.Delay(1 * 1000);
        }
        Console.WriteLine("退出");
    });
}
```

### 6.框架定义特性

* Conditional

```c#
#define UseDemo//宏, 被定义时, 特性标记为[Conditional("UseDemo")]的方法才会执行, 其中特性参数conditionString需要和宏的命名一致
using System.Diagnostics;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Demo();
            Console.ReadLine();
        }
        [Conditional("UseDemo")]
        static void Demo()
        {
            Console.WriteLine("Demo() has been executed.");
        }
    }
}
```

* CallerFilePath、CallerMemberName、CallerLineNumber

```c#
static void Main(string[] args)
{
    Printf();
    Console.ReadLine();
}
/// <summary>
/// 打印调用方信息
/// </summary>
/// <param name="callerFilePath">调用方源代码所在文件路径</param>
/// <param name="callerMemberName">调用方成员名称,方法名或者属性名</param>
/// <param name="callerLineNumber">调用方所在源代码的行数</param>
static void Printf([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = 0)
{
    Console.WriteLine($"CallerFilePath:{callerFilePath}, CallerMemberName:{callerMemberName}, CallerLineNumber:{callerLineNumber}");
}
```

### 7.依赖注入

依赖注入(Dependency Injection, DI)是控制反转(Inversion Of Control, IOC)思想的实现方式。简化模块的组装过程，降低模块之间的耦合度。

相关概念：

* 服务容器，负责管理注册的服务
* 服务，已注册的对象
* 对象生命周期，瞬态(Transient)，范围(Scoped)，单例(Singleton)

使用：

引用nuget包：Microsoft.Extensions.DependencyInjection

```c#
using Microsoft.Extensions.DependencyInjection;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddScoped(typeof(Test));//注册服务

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var test = serviceProvider.GetService<Test>();//获取服务
            test?.Printf();

            Console.ReadLine();
        }
    }

    internal class Test
    {
        public void Printf() => Console.WriteLine($"{nameof(Printf)} Invoke");
    }
}
```

生命周期：

* 如果一个类实现了IDisposable接口，则离开作用域之后容器会自动调用对象的Dispose方法

* 不要在长生命周期的对象中引用比它短的生命周期的对象，在Asp.Net Core中，这样做默认会抛异常

* 选择：如果类无状态，建议为Singleton；如果类有状态，且有Scope控制，建议为Scope，通常有Scope控制下的代码都是运行在同一个线程中，没有并发修改问题；而使用Transient则要谨慎

各生命周期示例如下：

```c#
internal class Program
{
    static void Main(string[] args)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<SingletonTest>();
        services.AddScoped<ScopeTest>();
        services.AddTransient<TransientTest>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        //Scope1
        using (var scope1 = serviceProvider.CreateScope())
        {
            Console.WriteLine("This is scope1.");
            var singletonTest1 = scope1.ServiceProvider.GetService<SingletonTest>();
            var scopeTest1 = scope1.ServiceProvider.GetService<ScopeTest>();
            var transientTest1 = scope1.ServiceProvider.GetService<TransientTest>();
        }

        //Scope2
        using (var scope2 = serviceProvider.CreateScope())
        {
            Console.WriteLine("This is scope2.");
            var singletonTest2 = scope2.ServiceProvider.GetService<SingletonTest>();
            var scopeTest2 = scope2.ServiceProvider.GetService<ScopeTest>();
            var transientTest2 = scope2.ServiceProvider.GetService<TransientTest>();
        }

        Console.ReadLine();
    }
}
internal class SingletonTest : IDisposable
{
    public SingletonTest() => Console.WriteLine($"{nameof(SingletonTest)} has been constructed.");
    public void Dispose() => Console.WriteLine($"{nameof(SingletonTest)} has been released.");
}
internal class ScopeTest : IDisposable
{
    public ScopeTest() => Console.WriteLine($"{nameof(ScopeTest)} has been constructed.");
    public void Dispose() => Console.WriteLine($"{nameof(ScopeTest)} has been released.");
}
internal class TransientTest : IDisposable
{
    public TransientTest() => Console.WriteLine($"{nameof(TransientTest)} has been constructed.");
    public void Dispose() => Console.WriteLine($"{nameof(TransientTest)} has been released.");
}
```

IServiceProvider服务定位器方法：

* T? GetService<T>如果获取不到对象，则返回null
* T GetRequiredService<T>如果获取不到对象，则抛异常
* IEnumerable<T> GetServices<T>获取多个满足条件的服务

### 8.配置(结合IOptions)

引用nuget包：Microsoft.Extensions.Configuration和Microsoft.Extensions.Configuration.Json

* 原始方法

```c#
static void Main(string[] args)
{
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddJsonFile("config.json", optional: false, reloadOnChange: true);

    IConfigurationRoot configurationRoot = configurationBuilder.Build();
    var str1 = configurationRoot["str1"];
    var str2 = configurationRoot.GetSection("str2:value").Value;

    Console.ReadLine();
}
```

* 配置绑定到类

引用nuget包：Microsoft.Extensions.Configuration.Binder

```c#
internal class Program
{
    static void Main(string[] args)
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("config.json", optional: false, reloadOnChange: true);

        IConfigurationRoot configurationRoot = configurationBuilder.Build();
        ConfigObj str2 = configurationRoot.GetSection("str2").Get<ConfigObj>();

        Console.ReadLine();
    }
}
internal class ConfigObj
{
    public string value { get; set; }
}
```

* 选项方式(推荐)，结合依赖注入：

引用nuget包：Microsoft.Extensions.Options

```c#
namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("config.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configurationRoot = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddScoped<Obj>();
            services.AddOptions().Configure<ConfigObj>(e => configurationRoot.GetSection("str2").Bind(e));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            Obj obj = scope.ServiceProvider.GetService<Obj>();

            Console.ReadLine();
        }
    }
    internal class ConfigObj
    {
        public string value { get; set; }
    }
    internal class Obj
    {
        public Obj(IOptionsSnapshot<ConfigObj> options)
        {
            Console.WriteLine($"{nameof(Obj)} has been constructed. options:{System.Text.Json.JsonSerializer.Serialize(options.Value)}");
        }
    }
}
```

当应用运行中修改配置文件时，IOptions不会读取到新的值；IOptionsMonitor会在配置改变时马上读取新的值；IOptionsSnapshot会在一个范围内(比如Asp.Net Core同一个请求中)保持一致，建议使用

* 命令行配置

引用nuget包：Microsoft.Extensions.Configuration.CommandLine

```c#
static void Main(string[] args)
{
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddCommandLine(args);

    IConfigurationRoot configurationRoot = configurationBuilder.Build();
    var conn = configurationRoot.GetSection("conn").Value;

    Console.WriteLine($"Conn:{conn}");

    Console.ReadLine();
}
```

使用命令行执行exe时，传入参数则可，支持多种格式，比如conn=127.0.01，--conn=127.0.0.1，--conn 127.0.0.1，/conn=127.0.0.1，/conn 127.0.0.1，注意键值之间加空格，格式不要混用

* 环境变量配置

引用nuget包：Microsoft.Extensions.Configuration.EnvironmentVariables

```c#
static void Main(string[] args)
{
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddEnvironmentVariables();//configurationBuilder.AddEnvironmentVariables("prefix");//匹配前缀

    IConfigurationRoot configurationRoot = configurationBuilder.Build();
    var environmentVariables = configurationRoot.AsEnumerable();

    foreach (var e in environmentVariables)
        Console.WriteLine($@"{e.Key}:{e.Value}");

    Console.ReadLine();
}
```

由于有可能和系统中的环境变量命名冲突，因此建议用有prefix参数的AddEnvironmentVariables方法，读取配置时prefix参数会被忽略

* 自定义读取配置
  * IConfigurationProvider
  
    开发一个直接或间接实现IConfigurationProvider接口的类XXXConfigurationProvider，一般继承自ConfigurationProvider。框架已有ConfigurationProvider派生出两个抽象类：1.FileConfigurationProvider，从文件读取时可使用，重写Load方法，2.StreamConfigurationProvider，从流对象读取时可使用，重写Load方法
    
  * IConfigurationSource
    
    再开发一个实现了IConfigurationSource接口的类XXXConfigurationSource，框架已有1.FileConfigurationSource，从文件读取时可使用，重写Build方法，需注意在方法内返回IConfigurationProvider前最好调用一下EnsureDefaults方法，作用是当用户没有提供IFileProvider时能提供一个默认值，2.StreamConfigurationSource，从流对象读取时使用，包括内存流、文件流、网络流等，重写Build方法
    
  

以上可以看出，FileConfigurationProvider和FileConfigurationSource，StreamConfigurationProvider和StreamConfigurationSource是属于配套使用。

读取txt文件作为配置示例：

```c#
internal class Program
{
    static void Main(string[] args)
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddTxtFile("config.txt", false, true);

        IConfigurationRoot configurationRoot = configurationBuilder.Build();
        var csvVariables = configurationRoot.AsEnumerable();

        foreach (var e in csvVariables)
            Console.WriteLine($@"{e.Key}:{e.Value}");

        Console.ReadLine();
    }
}
public class TxtConfigurationSource : FileConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new TxtConfigurationProvider(this);
    }
}
public class TxtConfigurationProvider : FileConfigurationProvider
{
    public TxtConfigurationProvider(FileConfigurationSource source) : base(source)
    {
    }

    public override void Load(Stream stream)
    {
        using StreamReader streamReader = new StreamReader(stream);
        string strLine = null;
        while ((strLine = streamReader.ReadLine()) != null)
        {
            var strs = strLine.Split('=');
            if (strs.Length > 1)
                Data[strs[0]] = strs[1];
        }
    }
}
public static class TxtConfigurationExtensions
{
    public static IConfigurationBuilder AddTxtFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
    {
        return builder.Add<TxtConfigurationSource>(s =>
        {
            s.FileProvider = null;
            s.Path = path;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
            //s.ResolveFileProvider();可选
        });
    }
}
```


## 二、性能调优篇

### 1.Span/Memory

在定义中，Span就是一个简单的值类型。它真正的价值，在于允许我们与任何类型的连续内存一起工作。 这些所谓的连续内存，包括： 1. 非托管内存缓冲区 2. 数组和子串 3. 字符串和子字符串 在使用中，Span确保了内存和数据安全，而且几乎没有开销。示例：

```c#
string content = "https://www.bing.com/";

Stopwatch stopWatch = new Stopwatch();
stopWatch.Start();
for (int j = 0; j < 100000; j++)
{
    content.Substring(10);
}
stopWatch.Stop();
Console.WriteLine("String:\tTime Elapsed:\t" + stopWatch.ElapsedMilliseconds.ToString("N0") + "ms");


ReadOnlySpan<char> span = content.AsSpan();//content.ToCharArray();
stopWatch.Restart();
for (int j = 0; j < 100000; j++)
{
    span.Slice(10);
}
stopWatch.Stop();
Console.WriteLine("Span:\tTime Elapsed:\t" + stopWatch.ElapsedMilliseconds.ToString("N0") + "ms");
```

限制：

* Span只能存储到执行栈上
* Span不能被装箱到堆上

* Span不能实现任何接口
* Span不能用于异步方法
* Span不能用作泛型类型参数

在以上限制场景中，可换用Memory，使用方法相似

```c#
string content = "https://www.bing.com/";
Stopwatch stopWatch = new Stopwatch();
ReadOnlyMemory<char> memory = content.AsMemory();
stopWatch.Start();
for (int j = 0; j < 100000; j++)
{
    memory.Slice(10);
}
stopWatch.Stop();
Console.WriteLine("Span:\tTime Elapsed:\t" + stopWatch.ElapsedMilliseconds.ToString("N0") + "ms");
```

### 2.高精度定时器

调用Run()开始，Stop()结束

```c#
public class HighPrecisionTimer
    {
        [DllImport("winmm")]
        static extern uint timeGetTime();

        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);

        [DllImport("winmm")]
        static extern uint timeEndPeriod(int t);

        private readonly int _interval;
        private readonly Action _action;
        private Thread _timerthread;
        public HighPrecisionTimer(int interval, Action action)
        {
            _interval = interval;
            _action = action;
        }
        public void Run()
        {
            _timerthread = new Thread(Job);

            timeBeginPeriod(1);

            _timerthread.Start();
        }

        private void Job()
        {
            uint timerstart = timeGetTime();
            while (true)
            {
                uint i = 0;
                while (i < _interval)//时间间隔(ms)
                {
                    i = timeGetTime() - timerstart;
                }
                timerstart = timeGetTime();
                Task.Run(_action);//需要循环运行的函数
            }
        }

        public void Stop()
        {
            if (_timerthread != null)
            {
                _timerthread.Abort();

                timeEndPeriod(1);
            }
        }
    }
```

