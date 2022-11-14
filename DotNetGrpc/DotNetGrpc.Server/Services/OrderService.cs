using Grpc.Core;

namespace DotNetGrpc.Server.Services;

public class OrderService : Order.OrderBase
{
    private readonly ILogger<OrderService> _logger;
    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<CreateResult> CreateOrder(CreateRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CreateResult
        {
            Result = true,
            Message = "订单创建成功"
        });
    }
    /// <summary>
    /// 查询订单
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<QueryResult> QueryOrder(QueryRequest request, ServerCallContext context)
    {
        return Task.FromResult(new QueryResult
        {
            Id = request.Id,
            OrderNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
            OrderName = "冰箱",
            Price = 1288
        });
    }
}
