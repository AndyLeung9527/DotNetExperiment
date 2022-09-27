using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(typeof(Program));

var app = builder.Build();

var mediator = app.Services.GetService<IMediator>();

var r = await mediator.Send(new Ping { Id = 1 });
await mediator.Publish(new Pinged { Id = 2 });

app.MapGet("/", () => $"Ping's id is {r}");

app.Run();

#region µ¥²¥
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

#region ¶à²¥
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