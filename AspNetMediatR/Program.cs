using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

var app = builder.Build();

var mediator = app.Services.GetRequiredService<IMediator>();

var r = await mediator.Send(new Ping { Id = 1 });
await mediator.Publish(new Pinged { Id = 2 });

app.MapGet("/", async (IMediator m) => { await m.Send(new Ping { Id = 1 }); return $"Ping's id is {r}"; });

app.Run();

#region 单播消息
public class Ping : IRequest<int>
{
    public int Id { get; set; }
}
public class PingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now} Handler:{nameof(PingedHandler1)}, Ping's id is {request.Id}");
        return Task.FromResult(request.Id);
    }
}
#endregion

#region 多播消息
public class Pinged : INotification
{
    public int Id { get; set; }
}
public class PingedHandler1 : INotificationHandler<Pinged>
{
    public Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now} Handler:{nameof(PingedHandler1)}, Ping's id is {notification.Id}");
        return Task.FromResult(notification.Id);
    }
}
public class PingedHandler2 : INotificationHandler<Pinged>
{
    public Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now} Handler:{nameof(PingedHandler2)}, Ping's id is {notification.Id}");
        return Task.FromResult(notification.Id);
    }
}
#endregion

#region IPipelineBehavior
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{DateTime.Now} LoggingBehavior start.");
        var response = await next();
        _logger.LogInformation($"{DateTime.Now} LoggingBehavior end.");
        return response;
    }
}
#endregion