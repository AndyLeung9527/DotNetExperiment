using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var connectionMultiplexer = ConnectionMultiplexer.Connect("192.168.2.113:6379");
builder.Services.AddSingleton(connectionMultiplexer);

var app = builder.Build();
app.UseRouting();
app.MapDefaultControllerRoute();

app.Run();
