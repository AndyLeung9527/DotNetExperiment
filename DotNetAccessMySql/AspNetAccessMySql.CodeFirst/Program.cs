using AspNetAccessMySql.CodeFirst.DbContexts;
using AspNetAccessMySql.CodeFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetAccessMySql.CodeFirst
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql("Server=127.0.0.1;Port=3306;Database=test1;Uid=root;Pwd=root;", MySqlServerVersion.LatestSupportedServerVersion));

            var app = builder.Build();

            app.MapGet("/", ([FromServices] AppDbContext dbContext) => dbContext.Set<Order>().FirstOrDefaultAsync());

            app.Run();
        }
    }
}
