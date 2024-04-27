using Grpc.Core;

namespace DotNetGrpc.Server.Services;

public class BidirectionalStreamingService : BidirectionalStreaming.BidirectionalStreamingBase
{
    public override async Task PingPongHello(IAsyncStreamReader<Serve> requestStream, IServerStreamWriter<Catch> responseStream, ServerCallContext context)
    {
        try
        {
            //获取接收请求头header
            var header1 = context.RequestHeaders.Get("header1")?.Value;

            //设置发送响应头header
            await context.WriteResponseHeadersAsync(new Metadata { { "header2", "content2" } });

            long round = 0L;

            context.CancellationToken.Register(() => {
                Console.WriteLine($"结束, {context.Peer} : {round}");
                //统计次数
                context.ResponseTrailers.Add("round", round.ToString());
                //设置响应状态码
                context.Status = new Status(StatusCode.OK, string.Empty);
            });

            while (!context.CancellationToken.IsCancellationRequested)
            {
                var asyncRequests = requestStream.ReadAllAsync(context.CancellationToken);
                await foreach (var req in asyncRequests)
                {
                    var content = Random.Shared.Next();
                    await responseStream.WriteAsync(new Catch
                    {
                        Id = req.Id,
                        Content = content
                    });
                    Console.WriteLine($" {context.Peer} : 第{req.Id}次服务端收到 {req.Content}, 第{req.Id + 1}次发送{content}");
                    round++;
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
        finally
        {
            Console.WriteLine($"PingPongHello结束");
        }
    }
}

