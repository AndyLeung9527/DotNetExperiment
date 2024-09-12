using Lib;
using Microsoft.AspNetCore.Mvc;

namespace AspNetWebApiServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/api/users/{id}", ([FromRoute] string id) => new User { Account = $"a{id}", Password = $"s{id}" });

            app.MapPost("/api/users", ([FromBody] User user) => user);

            app.Run();
        }
    }
}
