using DotNetAccessMySql.CodeFirst.DbContexts;
using DotNetAccessMySql.CodeFirst.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetAccessMySql.CodeFirst
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql("Server=127.0.0.1;Port=3306;Database=test1;Uid=root;Pwd=root;", MySqlServerVersion.LatestSupportedServerVersion).Options;
            using var context = new AppDbContext(contextOptions);

            var order = await context.Set<Order>().FirstOrDefaultAsync();
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(order));
        }
    }
}
