// See https://aka.ms/new-console-template for more information
using DotNetGrpc.Client;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

#region Normal
GrpcRequestTest grpcRequestTest = new GrpcRequestTest();
grpcRequestTest.CreateOrder();
#endregion

#region IOC
IServiceCollection services = new ServiceCollection();
services.AddTransient<GrpcRequestIOCTest>();
services.AddGrpcClient<Order.OrderClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
}).ConfigureChannel(grpcOptions =>
{

});

IServiceProvider serviceProvider = services.BuildServiceProvider();
var grpcRequestIOCTest = serviceProvider.GetRequiredService<GrpcRequestIOCTest>();
grpcRequestIOCTest.CreateOrder();
#endregion

#region 下载文件
{
    string sourceFileFullName = @"D:\demo.txt";
    string targetFileFullName = @"D:\demo_copy.txt";
    using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
    {
        MaxReceiveMessageSize = null,
        MaxRetryAttempts = 3,
        HttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }
    });
    var client = new DownloadFile.DownloadFileClient(channel);
    var readFileReply = client.ReadFile(new ReadFileRequest { FileFullName = sourceFileFullName });
    using var fileStream = File.Open(targetFileFullName, FileMode.Create, FileAccess.Write);
    var received = 0L;
    while (await readFileReply.ResponseStream.MoveNext(CancellationToken.None))
    {
        var current = readFileReply.ResponseStream.Current;
        var buffer = current.Content.ToByteArray();
        fileStream.Seek(received, SeekOrigin.Begin);
        await fileStream.WriteAsync(buffer);
        received += buffer.Length;
        received = Math.Min(received, current.TotalSize ?? default);
    }
}
#endregion

#region 双向流式
{
    var serverAddress = "https://localhost:5001";
    var handler = new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        //tcp心跳探活
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        //启用并发tcp连接
        EnableMultipleHttp2Connections = true
    };
    using var channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
    {
        Credentials = ChannelCredentials.SecureSsl,
        MaxReceiveMessageSize = 1024 * 1024 * 10,
        MaxSendMessageSize = 1024 * 1024 * 10,
        HttpHandler = handler
    });
    var client = new BidirectionalStreaming.BidirectionalStreamingClient(channel);
    AsyncDuplexStreamingCall<Serve, Catch>? duplexCall = null;
    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}, 开始请求");
    using (var cancellationTokenSource = new CancellationTokenSource(5 * 1000))
    {
        try
        {
            //设置发送头header
            duplexCall = client.PingPongHello(new Metadata { { "header1", "content1" } }, null, cancellationTokenSource.Token);

            //获取接收响应头
            var header2 = (await duplexCall.ResponseHeadersAsync).Get("header2")?.Value;

            var content = Random.Shared.Next();
            await duplexCall.RequestStream.WriteAsync(new Serve { Id = 1, Content = content });
            await foreach (var resp in duplexCall.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"第{resp.Id}次发送，客户端发送{content},客户端收到 {resp.Content}");
                content = Random.Shared.Next();
                await duplexCall.RequestStream.WriteAsync(new Serve { Id = resp.Id + 1, Content = content });
            }
            //Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}请求结束");
            //if (duplexCall != null)
            //{
            //    //接受响应尾
            //    var tr = duplexCall.GetTrailers();
            //    var round = tr.Get("round")?.Value.ToString();
            //    Console.Write($" 进行了{round}次交互）");
            //}
        }
        catch (RpcException ex)
        {
            var trailers = ex.Trailers;
            _ = trailers.GetValue("round");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}请求（5s）结束，{ex.Message}");
        }
    }
}
#endregion

Console.ReadLine();