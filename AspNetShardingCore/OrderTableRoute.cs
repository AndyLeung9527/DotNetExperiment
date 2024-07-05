namespace AspNetShardingCore;

using AspNetShardingCore.Models;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

/// <summary>
/// 分表路由
/// </summary>
public class OrderTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<Order>
{
    public OrderTableRoute()
        : base(2, 3)
    {
    }

    /// <summary>
    /// 使用实体id进行分表(保证分片键不会被修改)
    /// </summary>
    public override void Configure(EntityMetadataTableBuilder<Order> builder)
    {
        builder.ShardingProperty(o => o.Id);
    }
}
