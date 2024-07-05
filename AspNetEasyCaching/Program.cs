using EasyCaching.Core;
using EasyCaching.InMemory;
using Microsoft.AspNetCore.Mvc;

namespace AspNetEasyCaching
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEasyCaching(options =>
            {
                // use memory cache with a simple way
                options.UseInMemory();

                // use memory cache with your own configuration
                options.UseInMemory(config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = 60,
                        // total count of cache items, default value is 10000
                        SizeLimit = 100,
                        // enable deep clone when reading object from cache or not, default value is true.
                        // if you need to modify the data after you read from cache, don't forget the enable deep clone, otherwise, the cached data will be modified.
                        // deep clone will hurt the performance, so if you don't need it, you should disable.
                        EnableReadDeepClone = true,
                        // enable deep clone when writing object to cache or not, default value is false.
                        EnableWriteDeepClone = false
                    };
                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = 120;
                    // whether enable logging, default is false
                    config.EnableLogging = false;
                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = 5000;
                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = 300;
                }, "custom");

                //use memory cache with configuration in the appsettings.json
                options.UseInMemory(builder.Configuration, "config", "easycaching:inmemory");
            });

            var app = builder.Build();

            app.MapGet("/", async ([FromServices] IEasyCachingProvider provider) =>
            {
                // remove
                provider.Remove("demo");

                // set
                provider.Set("demo", "123", TimeSpan.FromMinutes(1));

                // get
                var res = provider.Get("demo", () => "456", TimeSpan.FromMinutes(1));

                // get without data retriever
                res = provider.Get<string>("demo");

                // remove async
                await provider.RemoveAsync("demo");

                // set async
                await provider.SetAsync("demo", "123", TimeSpan.FromMinutes(1));

                // get async
                res = await provider.GetAsync("demo", () => Task.FromResult("456"), TimeSpan.FromMinutes(1));

                // get without data retriever Async
                res = await provider.GetAsync<string>("demo");

                return res.HasValue ? res.Value : "empty";
            });

            app.Run();
        }
    }
}
