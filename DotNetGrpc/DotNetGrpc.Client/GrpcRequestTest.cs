namespace DotNetGrpc.Client;

using DotNetGrpc.Server;
using Grpc.Net.Client;

/// <summary>
/// gRpc请求测试
/// </summary>
public class GrpcRequestTest
{
    public void CreateOrder()
    {
        string url = "https://localhost:7242";
        using (var channel = GrpcChannel.ForAddress(url))
        {
            var client = new Order.OrderClient(channel);
            var reply = client.CreateOrder(new CreateRequest
            {
                OrderNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderName = "冰箱22款",
                Price = 1688
            });

            Console.WriteLine($"结果:{reply.Result},message:{reply.Message}");
        }
    }
}