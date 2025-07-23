using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace Custom.Hosting;

public static class CustomResourceBuilderExtensions
{
    private const string UserEnvVarName = "CUSTOM_USER";
    private const string PasswordEnvVarName = "CUSTOM_PASS";

    public static IResourceBuilder<CustomResource> AddCustom(this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null,
        int? smtpPort = null,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null)
    {
        var passwordParameter = password?.Resource ??
            ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var resource = new CustomResource(name, userName?.Resource, passwordParameter);

        // 订阅AppHost事件
        // 事件按顺序发布
        // 1. BeforeStartEvent（主机启动前触发）
        // 2. AfterEndpointsAllocatedEvent（主机分配终结点后触发）
        // 3. AfterResourcesCreatedEvent（主机创建资源后触发）
        builder.Eventing.Subscribe<BeforeStartEvent>(
            static (@event, cancellationToken) =>
            {
                return Task.CompletedTask;
            });

        // 订阅Resource事件
        builder.Eventing.Subscribe<ResourceReadyEvent>(
            resource,
            (@event, cancellationToken) =>
            {
                var logger = @event.Services.GetRequiredService<ResourceLoggerService>().GetLogger(resource);

                return Task.CompletedTask;
            });

        return builder.AddResource(resource)
                        .WithImage("library/redis")// 镜像路径/镜像名
                        .WithImageRegistry("docker.io")
                        .WithImageTag("latest")// 镜像版本
                        .WithImagePullPolicy(ImagePullPolicy.Default)// 镜像拉取策略
                        .WithHttpEndpoint(targetPort: 1080, port: httpPort, name: CustomResource.HttpEndpointName)
                        .WithEndpoint(targetPort: 1025, port: smtpPort, name: CustomResource.SmtpEndpointName)
                        .WithEnvironment(context =>
                        {
                            context.EnvironmentVariables[UserEnvVarName] = resource.UserNameReference;
                            context.EnvironmentVariables[PasswordEnvVarName] = resource.PasswordParameter;
                        });
    }
}
