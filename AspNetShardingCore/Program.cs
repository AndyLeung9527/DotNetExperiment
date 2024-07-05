using AspNetShardingCore.DbContexts;
using AspNetShardingCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ShardingCore;

namespace AspNetShardingCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddShardingDbContext<AppDbContext>()
                .UseRouteConfig(op =>
                {
                    op.AddShardingTableRoute<OrderTableRoute>();
                    op.AddShardingDataSourceRoute<OrderDataSourceRoute>();
                })
                .UseConfig((sp, op) =>
                {
                    op.UseShardingQuery((con, b) =>
                    {
                        b.UseMySql(con, MySqlServerVersion.LatestSupportedServerVersion);
                    });
                    op.UseShardingTransaction((con, b) =>
                    {
                        b.UseMySql(con, MySqlServerVersion.LatestSupportedServerVersion);
                    });
                    op.AddDefaultDataSource("db0", "Server=127.0.0.1;Port=3306;Database=db0;Uid=root;Pwd=root;");
                    op.AddExtraDataSource(sp => new Dictionary<string, string>()
                    {
                        { "db1", "Server=127.0.0.1;Port=3306;Database=db1;Uid=root;Pwd=root;"},
                        { "db2", "Server=127.0.0.1;Port=3306;Database=db2;Uid=root;Pwd=root;"}
                    });
                    op.UseShardingMigrationConfigure(b =>
                    {
                        b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
                    });
                })
                .AddShardingCore();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var defaultShardingDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (defaultShardingDbContext.Database.GetPendingMigrations().Any())
                {
                    defaultShardingDbContext.Database.Migrate();
                }
            }

            app.Services.UseAutoTryCompensateTable();

            app.MapGet("/", ([FromServices] AppDbContext dbContext) => dbContext.Set<Order>().ToListAsync());

            app.Run();
        }
    }
}
