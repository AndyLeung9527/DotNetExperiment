using Custom.Hosting;// 在Aspire引用时需加IsAspireProjectResource="false", 以标注此类库非服务类

var builder = DistributedApplication.CreateBuilder(args);

//参数资源, 从配置文件appsettings.json或secret.json获取
var username = builder.AddParameter("custom-username");
var password = builder.AddParameter("custom-password");

var customResource = builder.AddCustom("customresource", userName: username, password: password);// 添加自定义容器资源

var api = builder.AddProject<Projects.Custom_Api>("customapi")// 增加服务
            .WithReference(customResource)// 引用容器资源
            .WithCommand// 在操作面板的Actions中增加一个自定义资源命令
            (
                name: "clear-cache",// 调用命令的名称
                displayName: "Clear Cache",// 控制面板中显示的命令的名称
                executeCommand: context => Task.FromResult(CommandResults.Success()),// 调用命令时运行
                commandOptions: new CommandOptions
                {
                    Description = "Clear the cache",// 控制面板中显示的命令的描述
                    Parameter = null,// 调用后台传递的参数, 也可在命令中使用context.Parameters["param"]获取
                    ConfirmationMessage = "Are you sure you want to clear the cache?",// 在执行命令前弹出确认消息
                    IconName = "AnimalRabbitOff",// 控制面板中显示的图标的名称
                    IconVariant = IconVariant.Filled,// 控制面板中显示的图标的形式
                    IsHighlighted = true,// 是否高亮显示
                    UpdateState = context => ResourceCommandState.Enabled,// 调用回调以确定命令的"启用"状态
                }
            )
            .WithHttpCommand// 在操作面板的Actions中增加一个自定义http资源命令
            (
                path: "/httpCommand",// http请求路径
                displayName: "Http Command",
                commandOptions: new HttpCommandOptions
                {
                    Description = "Http Command",
                    // 发送请求前的回调函数
                    PrepareRequest = context =>
                    {
                        context.Request.Headers.Add("X-Invalidation-Key", "123456");
                        return Task.CompletedTask;
                    }
                }
            );
//.WaitFor(customResource);

// 添加url
api.WithUrl($"{api.GetEndpoint("https")}/", "Visit hello word");
// 自定义Endpoint的url
api.WithUrlForEndpoint("https", url =>
{
    url.DisplayText = "HTTPS";
});


// 参数资源, 从配置文件appsettings.json或secret.json获取
var nginxVersion = builder.AddParameter("nginxversion");
var container1 = builder.AddDockerfile("mynginx", "dockerfiles/1")// 将Dockerfile添加到资源, 相对路径为当前.csproj的路径
                        .WithBuildArg("NGINX_VERSION", nginxVersion)// 参数资源生成器执行版本, 部署时指定, 对应Dockerfile中的ARG命令; 也可直接传字符串指定版本
                        .WithBindMount("/VolumeMount/Nginx/home/web", "/home/web");// 挂载数据卷, 第一个参数为本地存储位置, 第二个参数为容器中路径
//.WithDockerfile("path");// 通过Dockerfile自定义现有容器资源, 相对路径为当前.csproj的路径
//.WithBuildSecret("ACCESS_TOKEN", accessToken);// 指定构建密钥

var connectionString = builder.AddConnectionString("mysql");// 配置链接字符串, 在配置文件中配置，第二个参数支持代码中生成

// 支持生成docker-compose.yml文件，在项目目录执行'dotnet run --publisher docker-compose --output-path ./docker-compose'，过程中会生成docker镜像
builder.AddDockerComposePublisher();

builder.Build().Run();

/*
Aspire生成清单命令:
dotnet run --project AspNetAspire.AppHost/AspNetAspire.AppHost.csproj -- --publisher manifest --output-path aspire-manifest.json
*/