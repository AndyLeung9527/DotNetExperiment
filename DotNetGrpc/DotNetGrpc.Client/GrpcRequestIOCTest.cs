namespace DotNetGrpc.Client;

using DotNetGrpc.Server;

/// <summary>
/// gRpc请求测试(IOC)
/// </summary>
public class GrpcRequestIOCTest
{
    readonly Order.OrderClient _orderClient;
    public GrpcRequestIOCTest(Order.OrderClient orderClient)
    {
        _orderClient = orderClient;
    }

    public void CreateOrder()
    {
        var reply = _orderClient.CreateOrder(new CreateRequest
        {
            OrderNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
            OrderName = "冰箱22款",
            Price = 1688
        });

        Console.WriteLine($"结果:{reply.Result},message:{reply.Message}");
    }
}
