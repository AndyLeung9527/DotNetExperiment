namespace AspNetShardingCore;

using AspNetShardingCore.Models;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Helpers;
using System.Collections.Generic;

/// <summary>
/// 分库路由
/// </summary>
public class OrderDataSourceRoute : AbstractShardingOperatorVirtualDataSourceRoute<Order, string>
{
    /// <summary>
    /// 根据id能否被7整除分为两个库
    /// </summary>
    public override string ShardingKeyToDataSourceName(object shardingKey)
    {
        if (shardingKey == null) throw new InvalidOperationException("sharding key can't null");
        var stringHashCode = ShardingCoreHelper.GetStringHashCode(shardingKey.ToString());
        return $"db{Math.Abs(stringHashCode) % 3}";
    }

    public override List<string> GetAllDataSourceNames()
    {
        return new List<string> { "db0", "db1", "db2" };
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 使用实体id进行分库(保证分片键不会被修改)
    /// </summary>
    public override void Configure(EntityMetadataDataSourceBuilder<Order> builder)
    {
        builder.ShardingProperty(o => o.Id);
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var t = ShardingKeyToDataSourceName(shardingKey);
        return shardingOperator switch
        {
            ShardingOperatorEnum.Equal => tail => tail == t,
            _ => tail => true
        };
    }
}
