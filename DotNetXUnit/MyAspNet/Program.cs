using Microsoft.AspNetCore.Mvc;

namespace MyAspNet;

// 被测试的Asp.Net项目Program类改为partial, 以便可以被集成测试项目访问
public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/Add", ([FromQuery] int i, [FromQuery] int j) => i + j);
        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
