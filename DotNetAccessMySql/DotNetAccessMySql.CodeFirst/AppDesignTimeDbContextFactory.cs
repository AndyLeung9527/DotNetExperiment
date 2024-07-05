namespace DotNetAccessMySql.CodeFirst;

using DotNetAccessMySql.CodeFirst.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] arg)
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql("Server=127.0.0.1;Port=3306;Database=test1;Uid=root;Pwd=root;", MySqlServerVersion.LatestSupportedServerVersion).Options;
        return new AppDbContext(contextOptions);
    }
}

