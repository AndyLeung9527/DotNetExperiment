using Microsoft.AspNetCore.Mvc;

namespace AspNetWebApiClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpApi<IUserApi>().ConfigureHttpApi(o =>
            {
                o.UseLogging = true;
                o.HttpHost = new Uri("http://localhost:5201/");
            });

            var app = builder.Build();

            app.MapGet("/get", async ([FromServices] IUserApi userApi) => await userApi.GetAsync("123"));

            app.MapGet("/post", async ([FromServices] IUserApi userApi) => await userApi.PostAysnc(new Lib.User { Account = "456", Password = "789" }));

            app.Run();
        }
    }
}
