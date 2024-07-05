using AspNetShardingCore.Models;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace AspNetShardingCore.DbContexts;

public class AppDbContext : AbstractShardingDbContext, IShardingTableDbContext
{
    public AppDbContext(DbContextOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// empty impl if use sharding table
    /// </summary>
    public IRouteTail? RouteTail { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>().HasKey(o => o.Id);
    }
}

