namespace AspNetShardingCore;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// 迁移数据库脚本生成
/// </summary>
public class ShardingMySqlMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
{
    private readonly IShardingRuntimeContext _shardingRuntimeContext;

    public ShardingMySqlMigrationsSqlGenerator([NotNull] MigrationsSqlGeneratorDependencies dependencies, [NotNull] ICommandBatchPreparer commandBatchPreparer, [NotNull] IMySqlOptions options, [NotNull] IShardingRuntimeContext shardingRuntimeContext)
        : base(dependencies, commandBatchPreparer, options)
    {
        _shardingRuntimeContext = shardingRuntimeContext;
    }

    protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
        var oldCmds = builder.GetCommandList().ToList();
        base.Generate(operation, model, builder);
        var newCmds = builder.GetCommandList().ToList();
        var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

        MigrationHelper.Generate(_shardingRuntimeContext, operation, builder, Dependencies.SqlGenerationHelper, addCmds);
    }
}
