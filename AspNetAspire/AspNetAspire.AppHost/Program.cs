using Custom.Hosting;// 在Aspire引用时需加IsAspireProjectResource="false", 以标注此类库非服务类

var builder = DistributedApplication.CreateBuilder(args);

//参数资源, 从配置文件appsettings.json或secret.json获取
var username = builder.AddParameter("custom-username");
var password = builder.AddParameter("custom-password");

var customResource = builder.AddCustom("customresource", userName: username, password: password);// 添加自定义容器资源

builder.AddProject<Projects.Custom_Api>("customapi")// 增加服务
        .WithReference(customResource);// 引用容器资源
                                       //.WaitFor(customResource);

builder.Build().Run();

/*
 Aspire生成清单命令:
dotnet run --project AspNetAspire.AppHost/AspNetAspire.AppHost.csproj -- --publisher manifest --output-path aspire-manifest.json
 */