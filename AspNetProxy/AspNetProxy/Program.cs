namespace AspNetProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient<ProxyService>().ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                MaxConnectionsPerServer = int.MaxValue,
                UseCookies = false
            });

            var app = builder.Build();

            app.UseMiddleware<ProxyMiddleware>();

            app.MapGet("/", () => "This is the proxy site!");

            app.Run();
        }
    }
}
