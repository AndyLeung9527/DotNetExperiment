namespace AspNetObjectPool;

using Microsoft.Extensions.ObjectPool;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        builder.Services.AddSingleton(sp =>
        {
            var provider = sp.GetRequiredService<ObjectPoolProvider>();
            return provider.Create(new StringBuilderPooledObjectPolicy());
        });

        var app = builder.Build();

        app.MapGet("/", (ObjectPool<StringBuilder> pool) =>
        {
            var sb = pool.Get();
            try
            {
                sb.Append("Hello, World!");
                return sb.ToString();
            }
            finally
            {
                pool.Return(sb);
            }
        });

        app.Run();
    }
}

public class StringBuilderPooledObjectPolicy : PooledObjectPolicy<StringBuilder>
{
    public override StringBuilder Create()
    {
        return new StringBuilder();
    }

    public override bool Return(StringBuilder obj)
    {
        obj.Clear();
        return true;
    }
}